using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BlockEngine
{
    // The native Block surface is intentionally small and deterministic in 2.2.0.
    // It provides the state bridge promised by the examples without evaluating host
    // language code or using reflection/dynamic compilation.
    internal static class NativeBlockRuntime
    {
        private static readonly Regex AssignmentPattern = new Regex(
            @"^([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.+)$",
            RegexOptions.Compiled);

        private static readonly Regex IdentifierPattern = new Regex(
            @"^[A-Za-z_][A-Za-z0-9_]*$",
            RegexOptions.Compiled);

        public static void Execute(string code, Dictionary<string, object> state, Action<string> output)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            if (state == null) throw new ArgumentNullException("state");
            if (output == null) output = Console.Write;

            string[] lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//")) continue;

                if (line.StartsWith("print", StringComparison.OrdinalIgnoreCase))
                {
                    ExecutePrint(line, state, output, i + 1);
                    continue;
                }

                Match assignment = AssignmentPattern.Match(line);
                if (assignment.Success)
                {
                    state[assignment.Groups[1].Value] = Evaluate(assignment.Groups[2].Value, state, i + 1);
                    continue;
                }

                if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("while ", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("for ", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("func ", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(string.Format(
                        "Native Block control statement at line {0} is not implemented in 2.2.0. Use an explicit <py> or <js> block for control flow.", i + 1));
                }

                throw new InvalidOperationException(string.Format(
                    "Unsupported native Block statement at line {0}: {1}", i + 1, line));
            }
        }

        public static void Delete(string code, Dictionary<string, object> state, Action<string> output)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (string.IsNullOrWhiteSpace(code)) return;

            string[] lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//")) continue;
                if (line.StartsWith("delete ", StringComparison.OrdinalIgnoreCase)) line = line.Substring(7).Trim();
                else if (line.StartsWith("del ", StringComparison.OrdinalIgnoreCase)) line = line.Substring(4).Trim();

                foreach (string rawName in line.Split(','))
                {
                    string name = rawName.Trim();
                    if (!IdentifierPattern.IsMatch(name))
                        throw new InvalidOperationException(string.Format("Invalid variable name in <del> at line {0}: {1}", i + 1, name));
                    state.Remove(name);
                }
            }
            if (output != null) output("[del] State variables removed." + Environment.NewLine);
        }

        private static void ExecutePrint(string line, Dictionary<string, object> state, Action<string> output, int lineNumber)
        {
            if (!line.EndsWith(")", StringComparison.Ordinal) || line.IndexOf('(') < 0)
                throw new InvalidOperationException(string.Format("Invalid print statement at line {0}.", lineNumber));

            int open = line.IndexOf('(');
            string arguments = line.Substring(open + 1, line.Length - open - 2);
            List<string> parts = SplitArguments(arguments, lineNumber);
            List<string> values = new List<string>();
            foreach (string part in parts)
            {
                object value = Evaluate(part, state, lineNumber);
                values.Add(FormatValue(value));
            }
            output(string.Join(" ", values) + Environment.NewLine);
        }

        private static List<string> SplitArguments(string text, int lineNumber)
        {
            List<string> parts = new List<string>();
            StringBuilder current = new StringBuilder();
            bool quoted = false;
            char quote = '\0';
            int depth = 0;
            bool escaped = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (escaped)
                {
                    current.Append(c);
                    escaped = false;
                    continue;
                }
                if (quoted && c == '\\')
                {
                    current.Append(c);
                    escaped = true;
                    continue;
                }
                if ((c == '\'' || c == '"'))
                {
                    if (!quoted) { quoted = true; quote = c; }
                    else if (quote == c) quoted = false;
                    current.Append(c);
                    continue;
                }
                if (!quoted && c == '(') depth++;
                if (!quoted && c == ')') depth--;
                if (!quoted && depth == 0 && c == ',')
                {
                    parts.Add(current.ToString().Trim());
                    current.Clear();
                }
                else current.Append(c);
            }
            if (quoted || depth != 0)
                throw new InvalidOperationException(string.Format("Unbalanced print arguments at line {0}.", lineNumber));
            if (current.Length > 0 || parts.Count > 0) parts.Add(current.ToString().Trim());
            return parts;
        }

        private static object Evaluate(string expression, Dictionary<string, object> state, int lineNumber)
        {
            ExpressionParser parser = new ExpressionParser(expression, state);
            object value = parser.ParseExpression();
            if (!parser.IsAtEnd)
                throw new InvalidOperationException(string.Format("Unexpected token in native Block expression at line {0}: {1}", lineNumber, parser.Remaining));
            return value;
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "null";
            bool isBool = value is bool;
            if (isBool) return ((bool)value) ? "true" : "false";
            IFormattable formattable = value as IFormattable;
            return formattable == null ? value.ToString() : formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        private sealed class ExpressionParser
        {
            private readonly string _text;
            private readonly Dictionary<string, object> _state;
            private int _position;

            public ExpressionParser(string text, Dictionary<string, object> state)
            {
                _text = text ?? "";
                _state = state;
            }

            public bool IsAtEnd { get { SkipWhitespace(); return _position >= _text.Length; } }
            public string Remaining { get { return _position < _text.Length ? _text.Substring(_position) : ""; } }

            public object ParseExpression() { return ParseOr(); }

            private object ParseOr()
            {
                object left = ParseAnd();
                while (Match("||")) left = ToBool(left) || ToBool(ParseAnd());
                return left;
            }

            private object ParseAnd()
            {
                object left = ParseEquality();
                while (Match("&&")) left = ToBool(left) && ToBool(ParseEquality());
                return left;
            }

            private object ParseEquality()
            {
                object left = ParseComparison();
                while (true)
                {
                    if (Match("==")) left = AreEqual(left, ParseComparison());
                    else if (Match("!=")) left = !AreEqual(left, ParseComparison());
                    else return left;
                }
            }

            private object ParseComparison()
            {
                object left = ParseTerm();
                while (true)
                {
                    if (Match("<=")) left = Compare(left, ParseTerm()) <= 0;
                    else if (Match(">=")) left = Compare(left, ParseTerm()) >= 0;
                    else if (Match("<")) left = Compare(left, ParseTerm()) < 0;
                    else if (Match(">")) left = Compare(left, ParseTerm()) > 0;
                    else return left;
                }
            }

            private object ParseTerm()
            {
                object left = ParseFactor();
                while (true)
                {
                    if (Match("+")) left = Add(left, ParseFactor());
                    else if (Match("-")) left = Number(left) - Number(ParseFactor());
                    else return left;
                }
            }

            private object ParseFactor()
            {
                object left = ParseUnary();
                while (true)
                {
                    if (Match("*")) left = Number(left) * Number(ParseUnary());
                    else if (Match("/"))
                    {
                        double divisor = Number(ParseUnary());
                        if (Math.Abs(divisor) < double.Epsilon) throw new InvalidOperationException("Division by zero in native Block expression.");
                        left = Number(left) / divisor;
                    }
                    else if (Match("%")) left = Number(left) % Number(ParseUnary());
                    else return left;
                }
            }

            private object ParseUnary()
            {
                if (Match("!")) return !ToBool(ParseUnary());
                if (Match("-")) return -Number(ParseUnary());
                return ParsePrimary();
            }

            private object ParsePrimary()
            {
                SkipWhitespace();
                if (Match("("))
                {
                    object result = ParseExpression();
                    Require(")");
                    return result;
                }
                if (_position < _text.Length && (_text[_position] == '\'' || _text[_position] == '"'))
                    return ParseString();

                int start = _position;
                while (_position < _text.Length && (char.IsLetterOrDigit(_text[_position]) || _text[_position] == '_' || _text[_position] == '.')) _position++;
                if (start == _position) throw new InvalidOperationException("Expected a value in native Block expression.");
                string token = _text.Substring(start, _position - start);
                double number;
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return number;
                if (token.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
                if (token.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
                if (token.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;
                object value;
                if (!_state.TryGetValue(token, out value)) throw new InvalidOperationException("Unknown native Block variable: " + token);
                return value;
            }

            private string ParseString()
            {
                char quote = _text[_position++];
                StringBuilder result = new StringBuilder();
                while (_position < _text.Length)
                {
                    char c = _text[_position++];
                    if (c == quote) return result.ToString();
                    if (c == '\\' && _position < _text.Length)
                    {
                        char escaped = _text[_position++];
                        if (escaped == 'n') result.Append('\n');
                        else if (escaped == 'r') result.Append('\r');
                        else if (escaped == 't') result.Append('\t');
                        else result.Append(escaped);
                    }
                    else result.Append(c);
                }
                throw new InvalidOperationException("Unterminated string in native Block expression.");
            }

            private void Require(string token)
            {
                if (!Match(token)) throw new InvalidOperationException("Expected '" + token + "' in native Block expression.");
            }

            private bool Match(string token)
            {
                SkipWhitespace();
                if (_text.Length - _position < token.Length) return false;
                if (string.Compare(_text, _position, token, 0, token.Length, StringComparison.Ordinal) != 0) return false;
                _position += token.Length;
                return true;
            }

            private void SkipWhitespace() { while (_position < _text.Length && char.IsWhiteSpace(_text[_position])) _position++; }

            private static double Number(object value)
            {
                if (value is bool) return (bool)value ? 1 : 0;
                if (value == null) return 0;
                double result;
                if (double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out result)) return result;
                throw new InvalidOperationException("Expected a numeric value in native Block expression.");
            }

            private static bool ToBool(object value)
            {
                if (value == null) return false;
                if (value is bool) return (bool)value;
                if (value is string) return !string.IsNullOrEmpty((string)value);
                return Math.Abs(Number(value)) > double.Epsilon;
            }

            private static object Add(object left, object right)
            {
                if (left is string || right is string) return FormatValue(left) + FormatValue(right);
                return Number(left) + Number(right);
            }

            private static bool AreEqual(object left, object right)
            {
                if (left == null || right == null) return left == null && right == null;
                if (left is bool || right is bool) return ToBool(left) == ToBool(right);
                double leftNumber, rightNumber;
                if (double.TryParse(Convert.ToString(left, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out leftNumber) &&
                    double.TryParse(Convert.ToString(right, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out rightNumber))
                    return Math.Abs(leftNumber - rightNumber) < double.Epsilon;
                return string.Equals(left.ToString(), right.ToString(), StringComparison.Ordinal);
            }

            private static int Compare(object left, object right)
            {
                double leftNumber, rightNumber;
                if (double.TryParse(Convert.ToString(left, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out leftNumber) &&
                    double.TryParse(Convert.ToString(right, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out rightNumber))
                    return leftNumber.CompareTo(rightNumber);
                return string.Compare(Convert.ToString(left, CultureInfo.InvariantCulture), Convert.ToString(right, CultureInfo.InvariantCulture), StringComparison.Ordinal);
            }
        }
    }
}
