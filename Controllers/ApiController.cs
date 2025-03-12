using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(IDataDictionaryService dataDictionaryService, ILogger<ApiController> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        // GET: api/tables?databaseId=1
        [HttpGet("tables")]
        public async Task<ActionResult<IEnumerable<object>>> GetTables(int databaseId)
        {
            try
            {
                var tables = await _dataDictionaryService.GetTablesAsync(databaseId);
                return Ok(tables.Select(t => new { tableId = t.TableId, tableName = t.TableName }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tables for database ID {DatabaseId}", databaseId);
                return StatusCode(500, "An error occurred while retrieving tables.");
            }
        }

        // GET: api/columns?tableId=1
        [HttpGet("columns")]
        public async Task<ActionResult<IEnumerable<object>>> GetColumns(int tableId)
        {
            try
            {
                var columns = await _dataDictionaryService.GetColumnsAsync(tableId);
                return Ok(columns.Select(c => new { columnId = c.ColumnId, columnName = c.ColumnName }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving columns for table ID {TableId}", tableId);
                return StatusCode(500, "An error occurred while retrieving columns.");
            }
        }
    }
} 