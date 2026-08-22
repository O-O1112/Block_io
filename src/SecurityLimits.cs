using System;
using System.Text;

namespace BlockEngine
{
    internal static class SecurityLimits
    {
        internal const int MaxCapturedOutputChars = 1024 * 1024;
        internal const int MaxConcurrentRequests = 4;
        internal const int MaxImportFiles = 256;
        internal const long MaxImportedBytes = 32L * 1024L * 1024L;
        internal const long MaxScriptBytes = 32L * 1024L * 1024L;
        internal const long MaxJsonBytes = 4L * 1024L * 1024L;
        internal const int RequestReadTimeoutSeconds = 30;

        internal static void AppendOutput(StringBuilder builder, string text)
        {
            if (builder == null || string.IsNullOrEmpty(text)) return;
            lock (builder)
            {
                if (builder.Length >= MaxCapturedOutputChars) return;
                int remaining = MaxCapturedOutputChars - builder.Length;
                if (text.Length <= remaining)
                {
                    builder.Append(text);
                    return;
                }

                builder.Append(text, 0, remaining);
                const string marker = "\n[output truncated]\n";
                if (builder.Length >= marker.Length)
                {
                    builder.Remove(builder.Length - marker.Length, marker.Length);
                }
                builder.Append(marker);
            }
        }
    }
}
