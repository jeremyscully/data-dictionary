using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Pages.Lineage
{
    public class VisualizeModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<VisualizeModel> _logger;

        public VisualizeModel(IDataDictionaryService dataDictionaryService, ILogger<VisualizeModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        public SelectList Databases { get; set; }
        public SelectList Tables { get; set; }
        public int? DatabaseId { get; set; }
        public int? TableId { get; set; }
        public int Depth { get; set; } = 1;
        public DataDictionary.Models.DatabaseModel SelectedDatabase { get; set; }
        public TableDefinitionModel SelectedTable { get; set; }
        public string NodesJson { get; set; }
        public string EdgesJson { get; set; }

        public async Task<IActionResult> OnGetAsync(int? databaseId, int? tableId, int depth = 1)
        {
            try
            {
                DatabaseId = databaseId;
                TableId = tableId;
                Depth = depth;

                // Load databases for dropdown
                var databases = await _dataDictionaryService.GetDatabasesAsync();
                Databases = new SelectList(databases, "DatabaseId", "DatabaseName", databaseId);

                // Load tables for dropdown if database is selected
                if (databaseId.HasValue)
                {
                    var tables = await _dataDictionaryService.GetTablesAsync(databaseId.Value);
                    Tables = new SelectList(tables, "TableId", "TableName", tableId);
                    SelectedDatabase = databases.FirstOrDefault(d => d.DatabaseId == databaseId.Value);
                }

                // If table is selected, get the table details
                if (tableId.HasValue)
                {
                    SelectedTable = await _dataDictionaryService.GetTableAsync(tableId.Value);
                }

                // Generate visualization data
                if (tableId.HasValue || databaseId.HasValue)
                {
                    await GenerateVisualizationDataAsync();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data for lineage visualization");
                TempData["ErrorMessage"] = "An error occurred while loading the visualization data.";
                return RedirectToPage("/Error");
            }
        }

        private async Task GenerateVisualizationDataAsync()
        {
            var nodes = new List<object>();
            var edges = new List<object>();
            var processedNodes = new HashSet<string>();

            if (TableId.HasValue)
            {
                // Start with the selected table
                await BuildLineageGraphForTableAsync(SelectedTable, nodes, edges, processedNodes, Depth, 0);
            }
            else if (DatabaseId.HasValue)
            {
                // Start with the selected database
                await BuildLineageGraphForDatabaseAsync(SelectedDatabase, nodes, edges, processedNodes, Depth, 0);
            }

            NodesJson = JsonSerializer.Serialize(nodes);
            EdgesJson = JsonSerializer.Serialize(edges);
        }

        private async Task BuildLineageGraphForTableAsync(
            TableDefinitionModel table, 
            List<object> nodes, 
            List<object> edges, 
            HashSet<string> processedNodes, 
            int maxDepth, 
            int currentDepth)
        {
            if (table == null || currentDepth > maxDepth)
                return;

            // Create a unique ID for the node
            string nodeId = $"table_{table.TableId}";

            // Skip if already processed
            if (processedNodes.Contains(nodeId))
                return;

            // Add the table node
            nodes.Add(new
            {
                id = nodeId,
                label = table.TableName,
                title = $"Table: {table.TableName}",
                group = "table",
                url = $"/Tables/Details?id={table.TableId}",
                color = table.IsView ? "#ffcc99" : "#99ccff"  // Different colors for tables and views
            });

            processedNodes.Add(nodeId);

            // Get source lineage relationships (where this table is the target)
            var sourceLineages = await _dataDictionaryService.GetTableLineageAsync(table.TableId, false);

            foreach (var lineage in sourceLineages)
            {
                if (lineage.SourceTable != null)
                {
                    string sourceNodeId = $"table_{lineage.SourceTable.TableId}";
                    
                    // Add edge from source table to this table
                    edges.Add(new
                    {
                        from = sourceNodeId,
                        to = nodeId,
                        label = lineage.LineageType.TypeName,
                        title = string.IsNullOrEmpty(lineage.TransformationDescription) 
                            ? lineage.LineageType.TypeName 
                            : lineage.TransformationDescription,
                        arrows = "to",
                        color = GetLineageTypeColor(lineage.LineageTypeId)
                    });

                    // Recursively process the source table if not at max depth
                    if (currentDepth < maxDepth)
                    {
                        await BuildLineageGraphForTableAsync(
                            lineage.SourceTable, 
                            nodes, 
                            edges, 
                            processedNodes, 
                            maxDepth, 
                            currentDepth + 1);
                    }
                }
                else if (lineage.SourceDatabase != null)
                {
                    string sourceNodeId = $"database_{lineage.SourceDatabase.DatabaseId}";
                    
                    // Add the database node if not already processed
                    if (!processedNodes.Contains(sourceNodeId))
                    {
                        nodes.Add(new
                        {
                            id = sourceNodeId,
                            label = lineage.SourceDatabase.DatabaseName,
                            title = $"Database: {lineage.SourceDatabase.DatabaseName}",
                            group = "database",
                            url = $"/Databases/Details?id={lineage.SourceDatabase.DatabaseId}",
                            color = "#ccffcc"  // Green for databases
                        });
                        
                        processedNodes.Add(sourceNodeId);
                    }
                    
                    // Add edge from source database to this table
                    edges.Add(new
                    {
                        from = sourceNodeId,
                        to = nodeId,
                        label = lineage.LineageType.TypeName,
                        title = string.IsNullOrEmpty(lineage.TransformationDescription) 
                            ? lineage.LineageType.TypeName 
                            : lineage.TransformationDescription,
                        arrows = "to",
                        color = GetLineageTypeColor(lineage.LineageTypeId)
                    });
                }
            }

            // Get target lineage relationships (where this table is the source)
            var targetLineages = await _dataDictionaryService.GetTableLineageAsync(table.TableId, true);

            foreach (var lineage in targetLineages)
            {
                if (lineage.TargetTable != null)
                {
                    string targetNodeId = $"table_{lineage.TargetTable.TableId}";
                    
                    // Add edge from this table to target table
                    edges.Add(new
                    {
                        from = nodeId,
                        to = targetNodeId,
                        label = lineage.LineageType.TypeName,
                        title = string.IsNullOrEmpty(lineage.TransformationDescription) 
                            ? lineage.LineageType.TypeName 
                            : lineage.TransformationDescription,
                        arrows = "to",
                        color = GetLineageTypeColor(lineage.LineageTypeId)
                    });

                    // Recursively process the target table if not at max depth
                    if (currentDepth < maxDepth)
                    {
                        await BuildLineageGraphForTableAsync(
                            lineage.TargetTable, 
                            nodes, 
                            edges, 
                            processedNodes, 
                            maxDepth, 
                            currentDepth + 1);
                    }
                }
                else if (lineage.TargetDatabase != null)
                {
                    string targetNodeId = $"database_{lineage.TargetDatabase.DatabaseId}";
                    
                    // Add the database node if not already processed
                    if (!processedNodes.Contains(targetNodeId))
                    {
                        nodes.Add(new
                        {
                            id = targetNodeId,
                            label = lineage.TargetDatabase.DatabaseName,
                            title = $"Database: {lineage.TargetDatabase.DatabaseName}",
                            group = "database",
                            url = $"/Databases/Details?id={lineage.TargetDatabase.DatabaseId}",
                            color = "#ccffcc"  // Green for databases
                        });
                        
                        processedNodes.Add(targetNodeId);
                    }
                    
                    // Add edge from this table to target database
                    edges.Add(new
                    {
                        from = nodeId,
                        to = targetNodeId,
                        label = lineage.LineageType.TypeName,
                        title = string.IsNullOrEmpty(lineage.TransformationDescription) 
                            ? lineage.LineageType.TypeName 
                            : lineage.TransformationDescription,
                        arrows = "to",
                        color = GetLineageTypeColor(lineage.LineageTypeId)
                    });
                }
            }
        }

        private async Task BuildLineageGraphForDatabaseAsync(
            DataDictionary.Models.DatabaseModel database, 
            List<object> nodes, 
            List<object> edges, 
            HashSet<string> processedNodes, 
            int maxDepth, 
            int currentDepth)
        {
            if (database == null || currentDepth > maxDepth)
                return;

            // Create a unique ID for the node
            string nodeId = $"database_{database.DatabaseId}";

            // Skip if already processed
            if (processedNodes.Contains(nodeId))
                return;

            // Add the database node
            nodes.Add(new
            {
                id = nodeId,
                label = database.DatabaseName,
                title = $"Database: {database.DatabaseName}",
                group = "database",
                url = $"/Databases/Details?id={database.DatabaseId}",
                color = "#ccffcc"  // Green for databases
            });

            processedNodes.Add(nodeId);

            // Get source lineage relationships (where this database is the target)
            var sourceLineages = await _dataDictionaryService.GetDatabaseLineageAsync(database.DatabaseId, false);

            foreach (var lineage in sourceLineages)
            {
                if (lineage.SourceDatabase != null)
                {
                    string sourceNodeId = $"database_{lineage.SourceDatabase.DatabaseId}";
                    
                    // Add edge from source database to this database
                    edges.Add(new
                    {
                        from = sourceNodeId,
                        to = nodeId,
                        label = lineage.LineageType.TypeName,
                        title = string.IsNullOrEmpty(lineage.TransformationDescription) 
                            ? lineage.LineageType.TypeName 
                            : lineage.TransformationDescription,
                        arrows = "to",
                        color = GetLineageTypeColor(lineage.LineageTypeId)
                    });

                    // Recursively process the source database if not at max depth
                    if (currentDepth < maxDepth)
                    {
                        await BuildLineageGraphForDatabaseAsync(
                            lineage.SourceDatabase, 
                            nodes, 
                            edges, 
                            processedNodes, 
                            maxDepth, 
                            currentDepth + 1);
                    }
                }
            }

            // Get target lineage relationships (where this database is the source)
            var targetLineages = await _dataDictionaryService.GetDatabaseLineageAsync(database.DatabaseId, true);

            foreach (var lineage in targetLineages)
            {
                if (lineage.TargetDatabase != null)
                {
                    string targetNodeId = $"database_{lineage.TargetDatabase.DatabaseId}";
                    
                    // Add edge from this database to target database
                    edges.Add(new
                    {
                        from = nodeId,
                        to = targetNodeId,
                        label = lineage.LineageType.TypeName,
                        title = string.IsNullOrEmpty(lineage.TransformationDescription) 
                            ? lineage.LineageType.TypeName 
                            : lineage.TransformationDescription,
                        arrows = "to",
                        color = GetLineageTypeColor(lineage.LineageTypeId)
                    });

                    // Recursively process the target database if not at max depth
                    if (currentDepth < maxDepth)
                    {
                        await BuildLineageGraphForDatabaseAsync(
                            lineage.TargetDatabase, 
                            nodes, 
                            edges, 
                            processedNodes, 
                            maxDepth, 
                            currentDepth + 1);
                    }
                }
            }

            // If at the top level, also include tables in this database
            if (currentDepth == 0)
            {
                var tables = await _dataDictionaryService.GetTablesAsync(database.DatabaseId);
                
                foreach (var table in tables)
                {
                    string tableNodeId = $"table_{table.TableId}";
                    
                    // Add the table node
                    nodes.Add(new
                    {
                        id = tableNodeId,
                        label = table.TableName,
                        title = $"Table: {table.TableName}",
                        group = "table",
                        url = $"/Tables/Details?id={table.TableId}",
                        color = table.IsView ? "#ffcc99" : "#99ccff"  // Different colors for tables and views
                    });
                    
                    processedNodes.Add(tableNodeId);
                    
                    // Add edge from database to table (implicit relationship)
                    edges.Add(new
                    {
                        from = nodeId,
                        to = tableNodeId,
                        label = "Contains",
                        dashes = true,
                        color = "#999999"
                    });
                    
                    // Process lineage for this table
                    await BuildLineageGraphForTableAsync(
                        table, 
                        nodes, 
                        edges, 
                        processedNodes, 
                        maxDepth - 1, 
                        currentDepth + 1);
                }
            }
        }

        private string GetLineageTypeColor(int lineageTypeId)
        {
            // Different colors for different lineage types
            switch (lineageTypeId)
            {
                case 1: // Raw
                    return "#3366cc";
                case 2: // Aggregated
                    return "#dc3545";
                case 3: // Computation
                    return "#fd7e14";
                default:
                    return "#6c757d";
            }
        }
    }
} 