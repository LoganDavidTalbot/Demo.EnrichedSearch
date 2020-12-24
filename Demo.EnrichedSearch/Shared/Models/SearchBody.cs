namespace Demo.EnrichedSearch.Shared.Models
{
    public class SearchBody
    {
        public string Search { get; set; }
        public int Page { get; set; } = 1;
        public string SelectedCategory { get; set; }
        public string SelectedTag { get; set; }
        public string FacetFilter { get
            {
                var filter = "";
                if (!string.IsNullOrWhiteSpace(SelectedCategory))
                    filter += $"Category eq '{SelectedCategory}'";
                if (!string.IsNullOrWhiteSpace(SelectedCategory) && !string.IsNullOrWhiteSpace(SelectedTag))
                    filter += " and ";
                if (!string.IsNullOrWhiteSpace(SelectedTag))
                    filter += $"Tags/any(t: t eq '{SelectedTag}')";
                return filter;
            }
        }

    }
}
