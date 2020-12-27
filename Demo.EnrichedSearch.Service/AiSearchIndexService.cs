using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Demo.EnrichedSearch.Shared.Models.AiSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public class AiSearchIndexService : IAiSearchIndexService
    {
        private readonly SearchIndexClient _indexClient;
        public AiSearchIndexService(SearchIndexClient indexClient)
        {
            _indexClient = indexClient;
        }
        public async Task<bool> CreateIndexAsync(string indexName)
        {
            FieldBuilder builder = new FieldBuilder();
            var index = new SearchIndex(indexName)
            {
                Fields = builder.Build(typeof(DemoIndex))
            };

            try
            {
                _indexClient.GetIndex(index.Name);
                _indexClient.DeleteIndex(index.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                //if the specified index not exist, 404 will be thrown.
            }

            try
            {
                await _indexClient.CreateOrUpdateIndexAsync(index);
            }
            catch (RequestFailedException ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            var index = await _indexClient.DeleteIndexAsync(indexName);

            return index.Status == (int)HttpStatusCode.OK;
        }

        public async Task<SearchIndexStatistics> GetIndexStatisticsAsync(string indexName)
        {
            try
            {
                return await _indexClient.GetIndexStatisticsAsync(indexName);
            }
            catch (RequestFailedException e) when (e.Status == (int)HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
