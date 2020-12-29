using Azure.Search.Documents.Indexes.Models;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public interface IAiSearchIndexService
    {
        Task<string> CreateIndexAndIndexerAsync();
        Task<bool> DeleteIndexAsync();
        Task<SearchIndexStatistics> GetIndexStatisticsAsync();
    }
}
