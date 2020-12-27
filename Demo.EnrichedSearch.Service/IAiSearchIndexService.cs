using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public interface IAiSearchIndexService
    {
        Task<bool> CreateIndexAsync(string indexName);
        Task<bool> DeleteIndexAsync(string indexName);
        Task<SearchIndexStatistics> GetIndexStatisticsAsync(string indexName);
    }
}
