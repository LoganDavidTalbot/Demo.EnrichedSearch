using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public interface ISearchService
    {
        Task<Shared.Models.SearchResponse> RunQueriesAsync(string searchQuery, int page, string facetFilter);
        Task<List<string>> AutoCompleteAndSuggestAsync(string term);
    }
}
