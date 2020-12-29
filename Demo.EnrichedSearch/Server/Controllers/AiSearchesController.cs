﻿using Demo.EnrichedSearch.Service;
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
        private readonly string _aiIndexName;

        public AiSearchesController(IAiSearchIndexService aiSearchIndexService, IConfiguration configuration)
        {
            _aiSearchIndexService = aiSearchIndexService;
            _aiIndexName = configuration["AiSearchIndexName"];
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
            var index = await _aiSearchIndexService.GetIndexStatisticsAsync();

            if (index == null)
            {
                return NotFound();
            }

            return Ok(new IndexStats()
            {
                IndexName = _aiIndexName,
                DocumentCount = index.DocumentCount,
                StorageSize = index.StorageSize
            });
        }
    }
}
