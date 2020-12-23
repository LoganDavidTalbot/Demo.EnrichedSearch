namespace Demo.EnrichedSearch.Shared.Models
{
    public class SearchBody
    {
        public string Search { get; set; }
        public int Page { get; set; } = 1;
        public string FacetFilter { get; set; } = "";

    }
}
