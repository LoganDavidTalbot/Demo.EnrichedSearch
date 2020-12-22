using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Demo.EnrichedSearch.Shared.Models;
using Microsoft.Spatial;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public class SearchIndexService : ISearchIndexService
    {
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _indexClient;

        public SearchIndexService(SearchClient searchClient, SearchIndexClient indexClient)
        {
            _searchClient = searchClient;
            _indexClient = indexClient;
        }

        public async Task CreateIndexAsync(string indexName)
        {
            Azure.Search.Documents.Indexes.FieldBuilder fieldBuilder = new Azure.Search.Documents.Indexes.FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(Hotel));

            var definition = new SearchIndex(indexName, searchFields);

            var suggester = new SearchSuggester("sg", new[] { "HotelName", "Category", "Address/City", "Address/StateProvince" });
            definition.Suggesters.Add(suggester);

            var index = await _indexClient.CreateOrUpdateIndexAsync(definition);
        }

        public async Task<SearchIndex> GetIndexAsync(string indexName)
        {
            var index = await _indexClient.GetIndexAsync(indexName);

            return index.Value;
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            var index = await _indexClient.DeleteIndexAsync(indexName);

            return index.Status == 200;
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

        // Upload documents in a single Upload request.
        public async Task UploadDocumentsAsync()
        {
            IndexDocumentsBatch<Hotel> batch = IndexDocumentsBatch.Create(
                IndexDocumentsAction.Upload(
                    new Hotel()
                    {
                        HotelId = "1",
                        HotelName = "Secret Point Motel",
                        Description = "The hotel is ideally located on the main commercial artery of the city in the heart of New York. A few minutes away is Time's Square and the historic centre of the city, as well as other places of interest that make New York one of America's most attractive and cosmopolitan cities.",
                        DescriptionFr = "L'hôtel est idéalement situé sur la principale artère commerciale de la ville en plein cœur de New York. A quelques minutes se trouve la place du temps et le centre historique de la ville, ainsi que d'autres lieux d'intérêt qui font de New York l'une des villes les plus attractives et cosmopolites de l'Amérique.",
                        Category = "Boutique",
                        Tags = new[] { "pool", "air conditioning", "concierge" },
                        ParkingIncluded = false,
                        LastRenovationDate = new DateTimeOffset(1970, 1, 18, 0, 0, 0, TimeSpan.Zero),
                        Rating = 3.6,
                        Location = GeographyPoint.Create(22, 22),
                        Rooms = new Room[] 
                        { 
                            new Room() { Description = "Room Des", BaseRate = 100, DescriptionFr = "Le Room", BedOptions = "Any", Type = "Single", SmokingAllowed = false, SleepsCount = 3, Tags = new string[] { "single", "front", "near pool" } } 
                        },
                        cheapest = 100,
                        expensive = 300,
                        Address = new Address()
                        {
                            StreetAddress = "677 5th Ave",
                            City = "New York",
                            StateProvince = "NY",
                            PostalCode = "10022",
                            Country = "USA"
                        }
                        
                    }),
                IndexDocumentsAction.Upload(
                    new Hotel()
                    {
                        HotelId = "2",
                        HotelName = "Twin Dome Motel",
                        Description = "The hotel is situated in a  nineteenth century plaza, which has been expanded and renovated to the highest architectural standards to create a modern, functional and first-class hotel in which art and unique historical elements coexist with the most modern comforts.",
                        DescriptionFr = "L'hôtel est situé dans une place du XIXe siècle, qui a été agrandie et rénovée aux plus hautes normes architecturales pour créer un hôtel moderne, fonctionnel et de première classe dans lequel l'art et les éléments historiques uniques coexistent avec le confort le plus moderne.",
                        Category = "Boutique",
                        Tags = new[] { "pool", "free wifi", "concierge" },
                        ParkingIncluded = false,
                        LastRenovationDate = new DateTimeOffset(1979, 2, 18, 0, 0, 0, TimeSpan.Zero),
                        Rating = 3.60,
                        Address = new Address()
                        {
                            StreetAddress = "140 University Town Center Dr",
                            City = "Sarasota",
                            StateProvince = "FL",
                            PostalCode = "34243",
                            Country = "USA"
                        },
                        Location = GeographyPoint.Create(22, 22),
                        Rooms = new Room[]
                        {
                            new Room() { Description = "Room Des", BaseRate = 100, DescriptionFr = "Le Room", BedOptions = "Any", Type = "Single", SmokingAllowed = false, SleepsCount = 3, Tags = new string[] { "single", "front", "near pool" } }
                        },
                        cheapest = 100,
                        expensive = 300,
                    }),
                IndexDocumentsAction.Upload(
                    new Hotel()
                    {
                        HotelId = "3",
                        HotelName = "Triple Landscape Hotel",
                        Description = "The Hotel stands out for its gastronomic excellence under the management of William Dough, who advises on and oversees all of the Hotel’s restaurant services.",
                        DescriptionFr = "L'hôtel est situé dans une place du XIXe siècle, qui a été agrandie et rénovée aux plus hautes normes architecturales pour créer un hôtel moderne, fonctionnel et de première classe dans lequel l'art et les éléments historiques uniques coexistent avec le confort le plus moderne.",
                        Category = "Resort and Spa",
                        Tags = new[] { "air conditioning", "bar", "continental breakfast" },
                        ParkingIncluded = true,
                        LastRenovationDate = new DateTimeOffset(2015, 9, 20, 0, 0, 0, TimeSpan.Zero),
                        Rating = 4.80,
                        Address = new Address()
                        {
                            StreetAddress = "3393 Peachtree Rd",
                            City = "Atlanta",
                            StateProvince = "GA",
                            PostalCode = "30326",
                            Country = "USA"
                        },
                        Location = GeographyPoint.Create(22, 22),
                        Rooms = new Room[]
                        {
                            new Room() { Description = "Room Des", BaseRate = 100, DescriptionFr = "Le Room", BedOptions = "Any", Type = "Single", SmokingAllowed = false, SleepsCount = 3, Tags = new string[] { "single", "front", "near pool" } }
                        },
                        cheapest = 100,
                        expensive = 300,
                    }),
                IndexDocumentsAction.Upload(
                    new Hotel()
                    {
                        HotelId = "4",
                        HotelName = "Sublime Cliff Hotel",
                        Description = "Sublime Cliff Hotel is located in the heart of the historic center of Sublime in an extremely vibrant and lively area within short walking distance to the sites and landmarks of the city and is surrounded by the extraordinary beauty of churches, buildings, shops and monuments. Sublime Cliff is part of a lovingly restored 1800 palace.",
                        DescriptionFr = "Le sublime Cliff Hotel est situé au coeur du centre historique de sublime dans un quartier extrêmement animé et vivant, à courte distance de marche des sites et monuments de la ville et est entouré par l'extraordinaire beauté des églises, des bâtiments, des commerces et Monuments. Sublime Cliff fait partie d'un Palace 1800 restauré avec amour.",
                        Category = "Boutique",
                        Tags = new[] { "concierge", "view", "24-hour front desk service" },
                        ParkingIncluded = true,
                        LastRenovationDate = new DateTimeOffset(1960, 2, 06, 0, 0, 0, TimeSpan.Zero),
                        Rating = 4.60,
                        Address = new Address()
                        {
                            StreetAddress = "7400 San Pedro Ave",
                            City = "San Antonio",
                            StateProvince = "TX",
                            PostalCode = "78216",
                            Country = "USA"
                        },
                        Location = GeographyPoint.Create(22, 22),
                        Rooms = new Room[]
                        {
                            new Room() { Description = "Room Des", BaseRate = 100, DescriptionFr = "Le Room", BedOptions = "Any", Type = "Single", SmokingAllowed = false, SleepsCount = 3, Tags = new string[] { "single", "front", "near pool" } }
                        },
                        cheapest = 100,
                        expensive = 300,
                    })
                );

            IndexDocumentsResult result = await _searchClient.IndexDocumentsAsync(batch);
        }
    }
}
