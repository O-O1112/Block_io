using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BlockEngine
{
    // Structured native Block interpreter. The syntax is the one already exposed
    // by the snippets: compound statements end with a standalone "block" line.
    internal static class NativeBlockProgram
    {
        private const int MaxLoopIterations = 10000;
        private const int MaxCallDepth = 64;

        private static readonly Regex Assignment = new Regex(@"^([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex IfHeader = new Regex(@"^if\s+(.+):$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WhileHeader = new Regex(@"^while\s+(.+):$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ForHeader = new Regex(@"^for\s+([A-Za-z_][A-Za-z0-9_]*)\s+in\s+(.+):$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FuncHeader = new Regex(@"^func\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)\s*:$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex Identifier = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        public static void Execute(string code, Dictionary<string, object> state, Action<string> output)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            if (state == null) throw new ArgumentNullException("state");
            if (output == null) output = Console.Write;
            string[] lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int index = 0;
            string terminator;
            List<Statement> program = ParseSequence(lines, ref index, true, out terminator);
            if (!string.IsNullOrEmpty(terminator)) throw Error(0, "Unexpected '" + terminator + "' at top level.");
            ExecuteStatements(program, new Context(state, output));
        }

        private static List<Statement> ParseSequence(string[] lines, ref int index, bool allowElse, out string terminator)
        {
            List<Statement> result = new List<Statement>();
            terminator = null;
            while (index < lines.Length)
            {
                int lineNumber = index + 1;
                string line = lines[index].Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//")) { index++; continue; }
                if (string.Equals(line, "block", StringComparison.OrdinalIgnoreCase)) { index++; terminator = "block"; return result; }
                if (string.Equals(line, "else:", StringComparison.OrdinalIgnoreCase))
                {
                    if (!allowElse) throw Error(lineNumber, "Unexpected else block.");
                    terminator = "else";
                    return result;
                }

                Match match = IfHeader.Match(line);
                if (match.Success)
                {
                    index++;
                    string bodyEnd;
                    List<Statement> thenBody = ParseSequence(lines, ref index, true, out bodyEnd);
                    List<Statement> elseBody = new List<Statement>();
                    if (bodyEnd == "else")
                    {
                        index++;
                        string elseEnd;
                        elseBody = ParseSequence(lines, ref index, false, out elseEnd);
                        if (elseEnd != "block") throw Error(lineNumber, "if/else block must end with 'block'.");
                    }
                    else if (bodyEnd != "block") throw Error(lineNumber, "if block must end with 'block'.");
                    result.Add(new IfStatement(lineNumber, match.Groups[1].Value, thenBody, elseBody));
                    continue;
                }

                match = WhileHeader.Match(line);
                if (match.Success)
                {
                    index++;
                    string bodyEnd;
                    List<Statement> body = ParseSequence(lines, ref index, false, out bodyEnd);
                    if (bodyEnd != "block") throw Error(lineNumber, "while block must end with 'block'.");
                    result.Add(new WhileStatement(lineNumber, match.Groups[1].Value, body));
                    continue;
                }

                match = ForHeader.Match(line);
                if (match.Success)
                {
                    index++;
                    string bodyEnd;
                    List<Statement> body = ParseSequence(lines, ref index, false, out bodyEnd);
                    if (bodyEnd != "block") throw Error(lineNumber, "for block must end with 'block'.");
                    result.Add(new ForStatement(lineNumber, match.Groups[1].Value, match.Groups[2].Value, body));
                    continue;
                }

                match = FuncHeader.Match(line);
                if (match.Success)
                {
                    List<string> parameters = new List<string>();
                    string parameterText = match.Groups[2].Value.Trim();
                    if (parameterText.Length > 0)
                    {
                        foreach (string raw in parameterText.Split(','))
                        {
                            string parameter = raw.Trim();
                            if (!Identifier.IsMatch(parameter)) throw Error(lineNumber, "Invalid function parameter: " + parameter);
                            parameters.Add(parameter);
                        }
                    }
                    index++;
                    string bodyEnd;
                    List<Statement> body = ParseSequence(lines, ref index, false, out bodyEnd);
                    if (bodyEnd != "block") throw Error(lineNumber, "func block must end with 'block'.");
                    result.Add(new FunctionStatement(lineNumber, match.Groups[1].Value, parameters, body));
                    continue;
                }

                if (line.StartsWith("else", StringComparison.OrdinalIgnoreCase)) throw Error(lineNumber, "else must be written as 'else:'.");
                result.Add(new SimpleStatement(lineNumber, line));
                index++;
            }
            return result;
        }

        private static void ExecuteStatements(List<Statement> statements, Context context)
        {
            foreach (Statement statement in statements)
            {
                FunctionStatement function = statement as FunctionStatement;
                if (function != null) { context.Functions[function.Name] = function; continue; }

                SimpleStatement simple = statement as SimpleStatement;
                if (simple != null) { ExecuteSimple(simple, context); continue; }

                IfStatement conditional = statement as IfStatement;
                if (conditional != null)
                {
                    ExecuteStatements(ToBool(Evaluate(conditional.Condition, context, conditional.Line)) ? conditional.ThenBody : conditional.ElseBody, context);
                    continue;
                }

                WhileStatement loop = statement as WhileStatement;
                if (loop != null)
                {
                    int count = 0;
                    while (ToBool(Evaluate(loop.Condition, context, loop.Line)))
                    {
                        if (++count > MaxLoopIterations) throw Error(loop.Line, "while loop exceeded the 10,000 iteration limit.");
                        ExecuteStatements(loop.Body, context);
                    }
                    continue;
                }

                ForStatement forLoop = statement as ForStatement;
                if (forLoop != null)
                {
                    IEnumerable values = Evaluate(forLoop.Iterable, context, forLoop.Line) as IEnumerable;
                    if (values == null) throw Error(forLoop.Line, "for expression is not iterable.");
                    int count = 0;
                    foreach (object value in values)
                    {
                        if (++count > MaxLoopIterations) throw Error(forLoop.Line, "for loop exceeded the 10,000 iteration limit.");
                        context.State[forLoop.Variable] = value;
                        ExecuteStatements(forLoop.Body, context);
                    }
                }
            }
        }

        private static void ExecuteSimple(SimpleStatement statement, Context context)
        {
            string line = statement.Code;
            if (string.Equals(line, "pass", StringComparison.OrdinalIgnoreCase)) return;
            if (line.StartsWith("print", StringComparison.OrdinalIgnoreCase))
            {
                if (!line.EndsWith(")", StringComparison.Ordinal) || line.IndexOf('(') < 0) throw Error(statement.Line, "Invalid print statement.");
                int open = line.IndexOf('(');
                string args = line.Substring(open + 1, line.Length - open - 2);
                List<string> parts = SplitArguments(args, statement.Line);
                List<string> values = new List<string>();
                foreach (string part in parts) values.Add(FormatValue(Evaluate(part, context, statement.Line)));
                context.Output(string.Join(" ", values) + Environment.NewLine);
                return;
            }
            if (line.StartsWith("return", StringComparison.OrdinalIgnoreCase) && (line.Length == 6 || char.IsWhiteSpace(line[6])))
            {
                string expression = line.Length == 6 ? "null" : line.Substring(6).Trim();
                throw new ReturnSignal(expression.Length == 0 ? null : Evaluate(expression, context, statement.Line));
            }
            Match assignment = Assignment.Match(line);
            if (assignment.Success)
            {
                context.State[assignment.Groups[1].Value] = Evaluate(assignment.Groups[2].Value, context, statement.Line);
                return;
            }
            Evaluate(line, context, statement.Line); // bare function call
        }

        private static List<string> SplitArguments(string text, int lineNumber)
        {
            List<string> parts = new List<string>();
            StringBuilder current = new StringBuilder();
            bool quoted = false;
            char quote = '\0';
            bool escaped = false;
            int depth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (escaped) { current.Append(c); escaped = false; continue; }
                if (quoted && c == '\\') { current.Append(c); escaped = true; continue; }
                if (c == '\'' || c == '"') { if (!quoted) { quoted = true; quote = c; } else if (quote == c) quoted = false; current.Append(c); continue; }
                if (!quoted && c == '(') depth++;
                if (!quoted && c == ')') depth--;
                if (!quoted && depth == 0 && c == ',') { parts.Add(current.ToString().Trim()); current.Clear(); } else current.Append(c);
            }
            if (quoted || depth != 0) throw Error(lineNumber, "Unbalanced expression arguments.");
            if (current.Length > 0 || parts.Count > 0) parts.Add(current.ToString().Trim());
            return parts;
        }

        private static object Evaluate(string expression, Context context, int lineNumber)
        {
            ExpressionParser parser = new ExpressionParser(expression, context, lineNumber);
            object value = parser.ParseExpression();
            if (!parser.IsAtEnd) throw Error(lineNumber, "Unexpected token: " + parser.Remaining);
            return value;
        }

        private static object InvokeFunction(string name, List<object> arguments, Context context, int lineNumber)
        {
            if (string.Equals(name, "range", StringComparison.OrdinalIgnoreCase))
            {
                if (arguments.Count < 1 || arguments.Count > 3) throw Error(lineNumber, "range expects 1 to 3 arguments.");
                double start = arguments.Count == 1 ? 0 : Number(arguments[0]);
                double end = arguments.Count == 1 ? Number(arguments[0]) : Number(arguments[1]);
                double step = arguments.Count < 3 ? 1 : Number(arguments[2]);
                if (Math.Abs(step) < double.Epsilon) throw Error(lineNumber, "range step cannot be zero.");
                List<object> values = new List<object>();
                int count = 0;
                if (step > 0) for (double i = start; i < end && count++ < MaxLoopIterations; i += step) values.Add(NormalizeNumber(i));
                else for (double i = start; i > end && count++ < MaxLoopIterations; i += step) values.Add(NormalizeNumber(i));
                return values;
            }

            FunctionStatement function;
            if (!context.Functions.TryGetValue(name, out function)) throw Error(lineNumber, "Unknown function: " + name);
            if (context.CallDepth >= MaxCallDepth) throw Error(lineNumber, "Function call depth exceeded.");
            if (arguments.Count != function.Parameters.Count) throw Error(lineNumber, string.Format("Function {0} expects {1} argument(s), got {2}.", name, function.Parameters.Count, arguments.Count));

            Dictionary<string, object> old = new Dictionary<string, object>(StringComparer.Ordinal);
            Dictionary<string, bool> existed = new Dictionary<string, bool>(StringComparer.Ordinal);
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                string parameter = function.Parameters[i];
                object oldValue;
                existed[parameter] = context.State.TryGetValue(parameter, out oldValue);
                if (existed[parameter]) old[parameter] = oldValue;
                context.State[parameter] = arguments[i];
            }

            context.CallDepth++;
            object returnValue = null;
            try { ExecuteStatements(function.Body, context); }
            catch (ReturnSignal signal) { returnValue = signal.Value; }
            finally
            {
                context.CallDepth--;
                foreach (string parameter in function.Parameters)
                    if (existed[parameter]) context.State[parameter] = old[parameter]; else context.State.Remove(parameter);
            }
            return returnValue;
        }

        private static object NormalizeNumber(double value) { return Math.Abs(value - Math.Round(value)) < 0.0000000001 ? (object)(long)Math.Round(value) : value; }
        private static double Number(object value)
        {
            if (value is bool) return (bool)value ? 1 : 0;
            if (value == null) return 0;
            double result;
            if (double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out result)) return result;
            throw new InvalidOperationException("Expected a numeric value in native Block expression.");
        }
        private static bool ToBool(object value) { if (value == null) return false; if (value is bool) return (bool)value; if (value is string) return !string.IsNullOrEmpty((string)value); return Math.Abs(Number(value)) > double.Epsilon; }
        private static object Add(object left, object right) { return left is string || right is string ? (object)(FormatValue(left) + FormatValue(right)) : NormalizeNumber(Number(left) + Number(right)); }
        private static bool Equal(object left, object right)
        {
            if (left == null || right == null) return left == null && right == null;
            double a, b;
            if (double.TryParse(Convert.ToString(left, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out a) && double.TryParse(Convert.ToString(right, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out b)) return Math.Abs(a - b) < 0.0000000001;
            return string.Equals(left.ToString(), right.ToString(), StringComparison.Ordinal);
        }
        private static int Compare(object left, object right)
        {
            double a, b;
            if (double.TryParse(Convert.ToString(left, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out a) && double.TryParse(Convert.ToString(right, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out b)) return a.CompareTo(b);
            return string.Compare(Convert.ToString(left, CultureInfo.InvariantCulture), Convert.ToString(right, CultureInfo.InvariantCulture), StringComparison.Ordinal);
        }
        private static string FormatValue(object value)
        {
            if (value == null) return "null";
            if (value is bool) return (bool)value ? "true" : "false";
            IFormattable formattable = value as IFormattable;
            return formattable == null ? value.ToString() : formattable.ToString(null, CultureInfo.InvariantCulture);
        }
        private static InvalidOperationException Error(int line, string message) { return new InvalidOperationException(line > 0 ? string.Format("Native Block error at line {0}: {1}", line, message) : "Native Block error: " + message); }

        private abstract class Statement { protected Statement(int line) { Line = line; } public int Line; }
        private sealed class SimpleStatement : Statement { public SimpleStatement(int line, string code) : base(line) { Code = code; } public string Code; }
        private sealed class IfStatement : Statement { public IfStatement(int line, string condition, List<Statement> thenBody, List<Statement> elseBody) : base(line) { Condition = condition; ThenBody = thenBody; ElseBody = elseBody; } public string Condition; public List<Statement> ThenBody; public List<Statement> ElseBody; }
        private sealed class WhileStatement : Statement { public WhileStatement(int line, string condition, List<Statement> body) : base(line) { Condition = condition; Body = body; } public string Condition; public List<Statement> Body; }
        private sealed class ForStatement : Statement { public ForStatement(int line, string variable, string iterable, List<Statement> body) : base(line) { Variable = variable; Iterable = iterable; Body = body; } public string Variable; public string Iterable; public List<Statement> Body; }
        private sealed class FunctionStatement : Statement { public FunctionStatement(int line, string name, List<string> parameters, List<Statement> body) : base(line) { Name = name; Parameters = parameters; Body = body; } public string Name; public List<string> Parameters; public List<Statement> Body; }
        private sealed class Context
        {
            public Context(Dictionary<string, object> state, Action<string> output) { State = state; Output = output; Functions = new Dictionary<string, FunctionStatement>(StringComparer.OrdinalIgnoreCase); }
            public Dictionary<string, object> State; public Action<string> Output; public Dictionary<string, FunctionStatement> Functions; public int CallDepth;
        }
        private sealed class ReturnSignal : Exception { public ReturnSignal(object value) { Value = value; } public object Value; }

        private sealed class ExpressionParser
        {
            private readonly string text; private readonly Context context; private readonly int line; private int position;
            public ExpressionParser(string value, Context owner, int sourceLine) { text = value ?? ""; context = owner; line = sourceLine; }
            public bool IsAtEnd { get { Skip(); return position >= text.Length; } }
            public string Remaining { get { return position < text.Length ? text.Substring(position) : ""; } }
            public object ParseExpression() { return ParseOr(); }
            private object ParseOr() { object left = ParseAnd(); while (Match("||")) left = ToBool(left) || ToBool(ParseAnd()); return left; }
            private object ParseAnd() { object left = ParseEquality(); while (Match("&&")) left = ToBool(left) && ToBool(ParseEquality()); return left; }
            private object ParseEquality() { object left = ParseComparison(); while (true) { if (Match("==")) left = Equal(left, ParseComparison()); else if (Match("!=")) left = !Equal(left, ParseComparison()); else return left; } }
            private object ParseComparison()
            {
                object left = ParseTerm();
                while (true) { if (Match("<=")) left = Compare(left, ParseTerm()) <= 0; else if (Match(">=")) left = Compare(left, ParseTerm()) >= 0; else if (Match("<")) left = Compare(left, ParseTerm()) < 0; else if (Match(">")) left = Compare(left, ParseTerm()) > 0; else return left; }
            }
            private object ParseTerm() { object left = ParseFactor(); while (true) { if (Match("+")) left = Add(left, ParseFactor()); else if (Match("-")) left = NormalizeNumber(Number(left) - Number(ParseFactor())); else return left; } }
            private object ParseFactor()
            {
                object left = ParseUnary();
                while (true) { if (Match("*")) left = NormalizeNumber(Number(left) * Number(ParseUnary())); else if (Match("/")) { double divisor = Number(ParseUnary()); if (Math.Abs(divisor) < double.Epsilon) throw Error(line, "Division by zero."); left = Number(left) / divisor; } else if (Match("%")) left = NormalizeNumber(Number(left) % Number(ParseUnary())); else return left; }
            }
            private object ParseUnary() { if (Match("!")) return !ToBool(ParseUnary()); if (Match("-")) return -Number(ParseUnary()); return ParsePrimary(); }
            private object ParsePrimary()
            {
                Skip();
                if (Match("(")) { object value = ParseExpression(); Require(")"); return value; }
                if (position < text.Length && (text[position] == '\'' || text[position] == '"')) return ParseString();
                if (Match("["))
                {
                    List<object> values = new List<object>();
                    if (!Match("]")) { while (true) { values.Add(ParseExpression()); if (Match("]")) break; Require(","); } }
                    return values;
                }
                int start = position;
                while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_' || text[position] == '.')) position++;
                if (start == position) throw Error(line, "Expected a value.");
                string token = text.Substring(start, position - start);
                double number;
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return NormalizeNumber(number);
                if (token.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
                if (token.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
                if (token.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;
                if (Match("("))
                {
                    List<object> arguments = new List<object>();
                    if (!Match(")")) { while (true) { arguments.Add(ParseExpression()); if (Match(")")) break; Require(","); } }
                    return InvokeFunction(token, arguments, context, line);
                }
                object resolvedValue;
                if (!context.State.TryGetValue(token, out resolvedValue)) throw Error(line, "Unknown variable: " + token);
                return resolvedValue;
            }
            private string ParseString()
            {
                char quote = text[position++]; StringBuilder result = new StringBuilder();
                while (position < text.Length) { char c = text[position++]; if (c == quote) return result.ToString(); if (c == '\\' && position < text.Length) { char e = text[position++]; result.Append(e == 'n' ? '\n' : e == 'r' ? '\r' : e == 't' ? '\t' : e); } else result.Append(c); }
                throw Error(line, "Unterminated string.");
            }
            private void Require(string token) { if (!Match(token)) throw Error(line, "Expected '" + token + "'."); }
            private bool Match(string token) { Skip(); if (text.Length - position < token.Length || string.Compare(text, position, token, 0, token.Length, StringComparison.Ordinal) != 0) return false; position += token.Length; return true; }
            private void Skip() { while (position < text.Length && char.IsWhiteSpace(text[position])) position++; }
        }
    }
}
