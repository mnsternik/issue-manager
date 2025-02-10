using IssueManager.Models.ViewModels.Requests;

namespace IssueManager.Utilities
{
    public static class RouteHelper
    {
        public static Dictionary<string, string> ToRouteValues(this RequestSearchFilters filters, int pageIndex)
        {
            var routeValues = new Dictionary<string, string> { { "pageIndex", pageIndex.ToString() } };

            foreach (var prop in typeof(RequestSearchFilters).GetProperties())
            {
                var value = prop.GetValue(filters);
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    routeValues[prop.Name] = value.ToString()!; 
                }
            }

            return routeValues; 
        }
    }
}
