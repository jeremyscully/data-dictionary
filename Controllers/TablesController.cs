using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DataDictionary.Services;
using DataDictionary.Models;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Controllers
{
    [Route("api/tables-data")]
    [ApiController]
    public class TablesController : ControllerBase
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<TablesController> _logger;

        public TablesController(IDataDictionaryService dataDictionaryService, ILogger<TablesController> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        /// <summary>
        /// Get tables for a specific database
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <returns>List of tables</returns>
        [HttpGet("database/{databaseId}")]
        public async Task<IActionResult> GetTables(int databaseId)
        {
            _logger.LogInformation("Getting tables for database ID: {DatabaseId}", databaseId);
            
            if (databaseId <= 0)
            {
                _logger.LogWarning("Invalid database ID: {DatabaseId}", databaseId);
                return BadRequest(new { error = "Invalid database ID" });
            }
            
            try
            {
                var tables = await _dataDictionaryService.GetTablesAsync(databaseId);
                _logger.LogInformation("Retrieved {Count} tables for database ID: {DatabaseId}", tables.Count(), databaseId);
                
                // Transform the data to a simpler format for the client
                var result = tables.Select(t => new {
                    tableId = t.TableId,
                    tableName = t.DatabaseObject?.ObjectName ?? $"Table_{t.TableId}",
                    schemaName = t.DatabaseObject?.SchemaName ?? "dbo",
                    fullName = $"{t.DatabaseObject?.SchemaName ?? "dbo"}.{t.DatabaseObject?.ObjectName ?? $"Table_{t.TableId}"}"
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error retrieving tables for database ID {DatabaseId}: {ErrorMessage}", databaseId, ex.Message);
                return StatusCode(500, new { error = "An error occurred while retrieving tables." });
            }
        }
    }

    [Route("api/columns-data")]
    [ApiController]
    public class ColumnsController : ControllerBase
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<ColumnsController> _logger;

        public ColumnsController(IDataDictionaryService dataDictionaryService, ILogger<ColumnsController> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        /// <summary>
        /// Get columns for a specific table
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns>List of columns</returns>
        [HttpGet("table/{tableId}")]
        public async Task<IActionResult> GetColumns(int tableId)
        {
            _logger.LogInformation("Getting columns for table ID: {TableId}", tableId);
            
            if (tableId <= 0)
            {
                _logger.LogWarning("Invalid table ID: {TableId}", tableId);
                return BadRequest(new { error = "Invalid table ID" });
            }
            
            try
            {
                var columns = await _dataDictionaryService.GetColumnsAsync(tableId);
                _logger.LogInformation("Retrieved {Count} columns for table ID: {TableId}", columns.Count(), tableId);
                
                // Transform the data to a simpler format for the client
                var result = columns.Select(c => new {
                    columnId = c.ColumnId,
                    columnName = c.ColumnName,
                    dataType = c.DataType?.TypeName ?? "unknown",
                    isNullable = c.IsNullable
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error retrieving columns for table ID {TableId}: {ErrorMessage}", tableId, ex.Message);
                return StatusCode(500, new { error = "An error occurred while retrieving columns." });
            }
        }
    }
} 