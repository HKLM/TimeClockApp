using System.Diagnostics;
using System.Text;

#nullable enable

namespace TimeClockApp.Shared.Exceptions
{
    public static class FlattenAggregateException
    {
        public static void ShowAggregateException(AggregateException ex)
        {
            StringBuilder builder = new();

            System.Collections.ObjectModel.ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
            builder.AppendLine("======================");
            builder.AppendLine($"Aggregate Exception: Count {innerExceptions.Count}");

            foreach (Exception inner in innerExceptions)
            {
                builder.AppendLine($"Continuation Exception: {inner!.Message}");
            }
            builder.Append("======================");

            Log.WriteLine(builder.ToString());
        }

        public static void ShowAggregateException(AggregateException ex, string? category)
        {
            StringBuilder builder = new();

            System.Collections.ObjectModel.ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
            builder.AppendLine("======================");
            builder.AppendLine($"Aggregate Exception: Count {innerExceptions.Count}");

            foreach (Exception inner in innerExceptions)
            {
                builder.AppendLine($"Continuation Exception: {inner!.Message}");
            }
            builder.Append("======================");

            Log.WriteLine(builder.ToString(), category);
        }

        /// <summary>
        /// Flattens the AggregateException and returns that as a string
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="category"></param>
        /// <returns>exception as a string</returns>
        public static string ShowAggregateExceptionForPopup(AggregateException ex, string? category)
        {
            StringBuilder builder = new();

            System.Collections.ObjectModel.ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
            builder.AppendLine("======================");
            builder.AppendLine($"Aggregate Exception: Count {innerExceptions.Count}");

            foreach (Exception inner in innerExceptions)
            {
                builder.AppendLine($"Continuation Exception: {inner!.Message}");
            }
            builder.Append("======================");

            Log.WriteLine(builder.ToString(), category);
            return builder.ToString();
        }
    }
}
