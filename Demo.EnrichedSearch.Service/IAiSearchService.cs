using Demo.EnrichedSearch.Shared.Models.AiSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public interface IAiSearchService
    {
        Task<AiSearchResponse> RunQueriesAsync(string searchQuery, int page);
    }
}
