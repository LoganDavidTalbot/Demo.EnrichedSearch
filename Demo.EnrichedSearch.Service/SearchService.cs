using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Demo.EnrichedSearch.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task<SearchResponse> RunQueriesAsync(string searchQuery)
        {
            SearchOptions options;
            SearchResults<Hotel> response;
            options = new SearchOptions()
            {
                IncludeTotalCount = true
            };

            options.Select.Add("HotelId");
            options.Select.Add("HotelName");
            options.Select.Add("Description");
            options.Select.Add("Address/City");

            response = await _searchclient.SearchAsync<Hotel>(searchQuery, options);
            var hotels = new List<Hotel>();
            await foreach (var hotel in response.GetResultsAsync())
            {
                hotels.Add(hotel.Document);
            }

            return new SearchResponse()
            {
                Data = hotels,
                Count = response.TotalCount.Value
            };
        }
    }
}
