using System.Collections.Generic;

namespace Demo.EnrichedSearch.Shared.Models
{
    public class SearchResponse
    {
        public long Count { get; set; }

        public List<Hotel> Data { get; set; }
    }
}
