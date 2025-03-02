using System.Diagnostics;

#nullable enable

namespace TimeClockApp.Shared.Exceptions
{
    public static class Log
    {
        public static void WriteLine(string? data)
        {
#if WINDOWS && DEBUG
            Debug.WriteLine(data);
#elif WINDOWS && TRACE
			Trace.WriteLine(data);
#else
			Console.WriteLine(data);
#endif
        }

        public static void WriteLine(string? data, string? category)
        {
#if WINDOWS && DEBUG
            Debug.WriteLine(data, category);
#elif WINDOWS && TRACE
			Trace.WriteLine(data, category);
#else
			Console.WriteLine(data);
#endif
        }
    }
}
