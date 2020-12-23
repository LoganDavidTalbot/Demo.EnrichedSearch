using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Demo.EnrichedSearch.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public class SearchService : ISearchService
    {
        private readonly SearchClient _searchclient;

        public SearchService(SearchClient searchclient)
        {
            _searchclient = searchclient;
        }

        public async Task<SearchResponse> RunQueriesAsync(string searchQuery, int page, string facetFilter)
        {
            SearchResults<Hotel> response;
            var options = new SearchOptions
            {
                Filter = facetFilter,

                SearchMode = SearchMode.All,
                // Skip past results that have already been returned.
                Skip = (page-1) * GlobalVariables.ResultsPerPage,

                // Take only the next page worth of results.
                Size = GlobalVariables.ResultsPerPage,

                // Include the total number of results.
                IncludeTotalCount = true
            };

            // Return information on the text, and number, of facets in the data.
            options.Facets.Add("Category,count:20");
            options.Facets.Add("Tags,count:20");

            // Enter Hotel property names into this list, so only these values will be returned.
            options.Select.Add("HotelId");
            options.Select.Add("HotelName");
            options.Select.Add("Description");
            options.Select.Add("Address/City");
            options.Select.Add("Category");
            options.Select.Add("Tags");

            response = await _searchclient.SearchAsync<Hotel>(searchQuery, options);

            var hotels = new List<Hotel>();
            await foreach (var hotel in response.GetResultsAsync())
            {
                hotels.Add(hotel.Document);
            }

            var model = new SearchResponse()
            {
                ResultList = hotels,
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
                model.previousPage = model.currentPage-1;
            }

            if (model.currentPage + 1 > model.pageCount)
            {
                model.nextPage = null;
            }
            else
            {
                model.nextPage = model.currentPage + 1;
            }

            // Convert Facets
            model.Facets = new Dictionary<string, List<string>>();
            foreach (var facet in response.Facets)
            {
                var values = new List<string>();
                foreach (var value in facet.Value)
                {
                    values.Add(value.Value.ToString());
                }
                model.Facets.Add(facet.Key, values);
            }

            return model;
        }

        public async Task<List<string>> AutoCompleteAndSuggestAsync(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return new List<string>();
            }

            // Setup the type-ahead search parameters.
            var ap = new AutocompleteOptions()
            {
                Mode = AutocompleteMode.OneTermWithContext,
                Size = 1,
            };
            var autocompleteResult = await _searchclient.AutocompleteAsync(term, "sg", ap);

            // Setup the suggest search parameters.
            var sp = new SuggestOptions()
            {
                Size = 8,
            };

            // Only one suggester can be specified per index. The name of the suggester is set when the suggester is specified by other API calls.
            // The suggester for the hotel database is called "sg" and simply searches the hotel name.
            var suggestResult = await _searchclient.SuggestAsync<Hotel>(term, "sg", sp).ConfigureAwait(false);

            // Create an empty list.
            var results = new List<string>();

            if (autocompleteResult.Value.Results.Count > 0)
            {
                // Add the top result for type-ahead.
                results.Add(autocompleteResult.Value.Results[0].Text);
            }
            else
            {
                // There were no type-ahead suggestions, so add an empty string.
                results.Add("");
            }

            for (int n = 0; n < suggestResult.Value.Results.Count; n++)
            {
                // Now add the suggestions.
                results.Add(suggestResult.Value.Results[n].Text);
            }

            return results;
        }
    }
}
