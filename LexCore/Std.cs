namespace LeXpert.LexCore
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Security.Principal;

    /// <summary>
    /// Contains LexCore wide standard constants, enumerations and functions.
    /// </summary>
    public static class Std
    {
        /// <summary>Maximum number of ASCII characters.</summary>
        public const int MaxNumASCIIChars = 128;
        /// <summary>Maximum allowed word length.</summary>
        public const int MaxWordLength = 64;
        /// <summary>Wildcard character ('?').</summary>
        public const byte WildcardChar = 63;
        /// <summary>Difference between uppercase and lowercase ASCII letters.</summary>
        public const byte UpperToLowerDiff = 32;

        public const byte CR_Char = 13; // '\r'
        public const byte LF_Char = 10; // '\n'
        public const byte USD_Char = 36; // '$'
        public const byte UKP_Char = 35; // '#'

        /// <summary>
        /// Writes a trace line with timestamp and context information.
        /// </summary>
        public static void TraceLine(string traceInfo)
        {
            var utc = DateTime.UtcNow;
            string info = $"{utc.Month}/{utc.Day}/{utc.Year} {utc.Hour}:{utc.Minute}:{utc.Second}.{utc.Millisecond}|"
                        + $"{Process.GetCurrentProcess().Id}|"
                        + $"{AppDomain.CurrentDomain.Id}|"
                        + $"{Thread.CurrentThread.ManagedThreadId}|"
                        + $"{WindowsIdentity.GetCurrent().Name}|"
                        + $"{traceInfo}";
            Trace.WriteLine(info);
        }
    }
}
