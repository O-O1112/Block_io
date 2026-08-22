using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlockEngine
{
    // State-only native operations are kept separate from the executable Block
    // language implementation in NativeBlockProgram.
    internal static class NativeStateOperations
    {
        private static readonly Regex IdentifierPattern = new Regex(
            @"^[A-Za-z_][A-Za-z0-9_]*$",
            RegexOptions.Compiled);

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
    }
}
