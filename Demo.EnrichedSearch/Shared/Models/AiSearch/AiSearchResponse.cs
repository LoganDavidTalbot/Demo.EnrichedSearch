using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Shared.Models.AiSearch
{
    public class AiSearchResponse
    {
        public long TotalCount { get; set; }

        // The current page being displayed.
        public int currentPage { get; set; }

        // The total number of pages of results.
        public int pageCount { get; set; }

        public int? previousPage { get; set; }

        public int? nextPage { get; set; }

        public List<DemoIndexShort> ResultList { get; set; }
    }
}
