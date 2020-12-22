using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public interface ISearchService
    {
        Task<Shared.Models.SearchResponse> RunQueriesAsync(string searchQuery);
    }
}
