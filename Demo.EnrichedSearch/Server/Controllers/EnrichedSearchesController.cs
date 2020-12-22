using Demo.EnrichedSearch.Service;
using Demo.EnrichedSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnrichedSearchesController : ControllerBase
    {
        private readonly ISearchIndexService _searchIndexService;
        private readonly ISearchService _searchService;
        private readonly string _hotelIndexName;

        public EnrichedSearchesController(ISearchIndexService searchIndexService, ISearchService searchService, IConfiguration configuration)
        {
            _searchIndexService = searchIndexService;
            _searchService = searchService;
            _hotelIndexName = configuration.GetSection("SearchService")["HotelIndexName"];
        }

        public IConfiguration Configuration { get; }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateIndex()
        {
            var index = await _searchIndexService.GetIndexStatisticsAsync(_hotelIndexName);

            if(index == null)
            {
                await _searchIndexService.CreateIndexAsync(_hotelIndexName);
                await _searchIndexService.UploadDocumentsAsync();
            }

            return Ok();
        }

        [HttpDelete()]
        public async Task<ActionResult> DeleteIndex()
        {
            var index = await _searchIndexService.GetIndexStatisticsAsync(_hotelIndexName);

            if (index != null)
            {
                await _searchIndexService.DeleteIndexAsync(_hotelIndexName);
                return Ok();
            }
            return NotFound();
        }

        [HttpGet()]
        public async Task<ActionResult> GetIndex()
        {
            var index = await _searchIndexService.GetIndexStatisticsAsync(_hotelIndexName);

            if (index == null)
            {
                return NotFound();
            }

            return Ok(new IndexStats()
            {
                IndexName = _hotelIndexName,
                DocumentCount = index.DocumentCount,
                StorageSize = index.StorageSize
            });
        }

        [HttpPost("Search")]
        public async Task<ActionResult> Search([FromBody] SearchBody body)
        {
            var response = await _searchService.RunQueriesAsync(body.Search);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}
