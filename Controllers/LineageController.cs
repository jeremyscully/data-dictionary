using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DataDictionary.Services;
using DataDictionary.Models;

namespace DataDictionary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineageController : ControllerBase
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<LineageController> _logger;

        public LineageController(IDataDictionaryService dataDictionaryService, ILogger<LineageController> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        /// <summary>
        /// Get lineage data for a specific column
        /// </summary>
        /// <param name="id">Column ID</param>
        /// <returns>JSON object with source and target lineages</returns>
        [HttpGet("column/{id}")]
        public async Task<IActionResult> GetColumnLineage(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving lineage data for column ID: {ColumnId}", id);
                
                // Verify the column exists
                var column = await _dataDictionaryService.GetColumnAsync(id);
                if (column == null)
                {
                    _logger.LogWarning("Column with ID {ColumnId} not found", id);
                    return NotFound(new { error = $"Column with ID {id} not found" });
                }
                
                // Get column lineage data
                var sourceLineages = await _dataDictionaryService.GetColumnLineageAsync(id, true);
                var targetLineages = await _dataDictionaryService.GetColumnLineageAsync(id, false);

                // Return the data as JSON
                return Ok(new { 
                    sourceLineages = sourceLineages ?? new List<DataLineageModel>(),
                    targetLineages = targetLineages ?? new List<DataLineageModel>()
                });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error retrieving column lineage for column ID {ColumnId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving lineage data: " + ex.Message });
            }
        }
    }
} 