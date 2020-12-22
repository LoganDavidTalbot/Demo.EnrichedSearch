using Azure.Search.Documents.Indexes.Models;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public interface ISearchIndexService
    {
        Task CreateIndexAsync(string indexName);
        Task UploadDocumentsAsync();
        Task<SearchIndex> GetIndexAsync(string indexName);
        Task<bool> DeleteIndexAsync(string indexName);
        Task<SearchIndexStatistics> GetIndexStatisticsAsync(string indexName);
    }
}
