using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Demo.EnrichedSearch.Shared.Models;
using Demo.EnrichedSearch.Shared.Models.AiSearch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public class AiSearchService : IAiSearchService
    {
        private readonly SearchClient _searchclient;
        public AiSearchService(SearchClient searchclient)
        {
            _searchclient = searchclient;
        }
        public async Task<AiSearchResponse> RunQueriesAsync(string searchQuery, int page)
        {
            SearchResults<DemoIndex> response;
            var options = new SearchOptions
            {
                Filter = "",

                SearchMode = SearchMode.All,
                // Skip past results that have already been returned.
                Skip = (page - 1) * GlobalVariables.ResultsPerPage,

                // Take only the next page worth of results.
                Size = GlobalVariables.ResultsPerPage,

                // Include the total number of results.
                IncludeTotalCount = true
            };

            // Enter Hotel property names into this list, so only these values will be returned.
            options.Select.Add("id");
            options.Select.Add("fileName");
            options.Select.Add("fileLocation");

            response = await _searchclient.SearchAsync<DemoIndex>(searchQuery, options);

            var demIndexShorts = new List<DemoIndexShort>();
            await foreach (var demo in response.GetResultsAsync())
            {
                demIndexShorts.Add(new DemoIndexShort()
                {
                    Id = demo.Document.Id,
                    FileName = demo.Document.FileName,
                    FileLocation = demo.Document.FileLocation
                });
            }

            var model = new AiSearchResponse()
            {
                ResultList = demIndexShorts,
                TotalCount = response.TotalCount.Value
            };
            model.pageCount = ((int)response.TotalCount + GlobalVariables.ResultsPerPage - 1) / GlobalVariables.ResultsPerPage;

            // This variable communicates the page number being displayed to the view.
            model.currentPage = page;
            if (model.currentPage - 1 < 1)
            {
                model.previousPage = null;
            }
            else
            {
                model.previousPage = model.currentPage - 1;
            }

            if (model.currentPage + 1 > model.pageCount)
            {
                model.nextPage = null;
            }
            else
            {
                model.nextPage = model.currentPage + 1;
            }

            return model;
        }
    }
}
