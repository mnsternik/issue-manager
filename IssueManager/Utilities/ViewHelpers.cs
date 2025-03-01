using IssueManager.Models;

namespace IssueManager.Utilities
{
    public class ViewHelpers
    {
        public static string GetStatusColor(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Open => "#3b82f6",    // Blue
                RequestStatus.InProgress => "#f59e0b", // Yellow
                RequestStatus.Resolved => "#10b981",   // Green
                RequestStatus.Closed => "#6b7280",     // Gray
                _ => "#6b7280"
            };
        }

        public static string GetPriorityColor(RequestPriority priority)
        {
            return priority switch
            {
                RequestPriority.Low => "#10b981",     // Green
                RequestPriority.Medium => "#f59e0b",   // Yellow
                RequestPriority.High => "#ef4444",     // Red
                RequestPriority.Critical => "#7f1d1d",  // Dark Red
                _ => "#6b7280"
            };
        }

        public static string Truncate(string text, int maxLength = 30)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
        }
    }
}
