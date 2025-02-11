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
                if (value != null)
                {
                    if (prop.PropertyType == typeof(DateTime?))
                    {
                        var dateValue = (DateTime?)value;
                        if (dateValue.HasValue)
                        {
                            // Use ISO 8601 format for culture-invariant parsing
                            routeValues[prop.Name] = dateValue.Value.ToString("yyyy-MM-dd");
                        }
                    }
                    else if (!string.IsNullOrEmpty(value.ToString()))
                    {
                        routeValues[prop.Name] = value.ToString()!;
                    }
                }
            }
            return routeValues;
        }
    }
}


