using System.Collections.Generic;

namespace Demo.EnrichedSearch.Shared.Models
{
    public class SearchResponse
    {
        public long TotalCount { get; set; }

        // The current page being displayed.
        public int currentPage { get; set; }

        // The total number of pages of results.
        public int pageCount { get; set; }

        public int? previousPage { get; set; }

        public int? nextPage { get; set; }

        public List<Hotel> ResultList { get; set; }

        public Dictionary<string, List<string>> Facets { get; set; }
    }
}
