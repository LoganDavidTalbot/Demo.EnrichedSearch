using Azure.Search.Documents.Indexes.Models;
using Demo.EnrichedSearch.Service;
using Demo.EnrichedSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AiSearchesController : ControllerBase
    {
        private readonly IAiSearchIndexService _aiSearchIndexService;
        private readonly IAiSearchService _aiSearchService;
        private readonly string _aiIndexName;
        private readonly string _aiIndexerName;

        public AiSearchesController(IAiSearchIndexService aiSearchIndexService, IAiSearchService aiSearchService, IConfiguration configuration)
        {
            _aiSearchIndexService = aiSearchIndexService;
            _aiSearchService = aiSearchService;
            _aiIndexName = configuration["AiSearchIndexName"];
            _aiIndexerName = configuration["AiSearchIndexerName"];
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateIndex()
        {
            var indexerStatus = await _aiSearchIndexService.CreateIndexAndIndexerAsync();

            return Ok(new { indexerStatus = indexerStatus });
        }

        [HttpGet()]
        public async Task<ActionResult> GetIndex()
        {
            SearchIndexStatistics index = null;
            string indexerStatus = null;
            try
            {
                index = await _aiSearchIndexService.GetIndexStatisticsAsync();
                indexerStatus = await _aiSearchIndexService.GetIndexerOverallStatusAsync();
            }
            catch (System.Exception e)
            {
                return BadRequest(new { errorMessage = e.Message });
            }            

            if (index == null)
            {
                return NotFound();
            }

            return Ok(new IndexStats()
            {
                IndexName = _aiIndexName,
                DocumentCount = index.DocumentCount,
                StorageSize = index.StorageSize,
                IndexerName = _aiIndexerName,
                IndexerStatus = indexerStatus
            });
        }

        [HttpPost("Search")]
        public async Task<ActionResult> Search([FromBody] SearchBody body)
        {
            var response = await _aiSearchService.RunQueriesAsync(body.Search, body.Page);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}
