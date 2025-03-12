using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Pages.Lineage
{
    public class CreateModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IDataDictionaryService dataDictionaryService, ILogger<CreateModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        [BindProperty]
        public DataLineageModel LineageRelationship { get; set; } = new DataLineageModel();

        public SelectList LineageTypes { get; set; }
        public SelectList Databases { get; set; }
        public SelectList SourceTables { get; set; }
        public SelectList SourceColumns { get; set; }

        public async Task<IActionResult> OnGetAsync(int? sourceColumnId = null)
        {
            try
            {
                // Load lineage types for dropdown
                var lineageTypes = await _dataDictionaryService.GetLineageTypesAsync();
                LineageTypes = new SelectList(lineageTypes, "LineageTypeId", "TypeName");

                // Load databases for dropdowns
                var databases = await _dataDictionaryService.GetDatabasesAsync();
                Databases = new SelectList(databases, "DatabaseId", "DatabaseName");

                // If sourceColumnId is provided, pre-select the source column and its parent table and database
                if (sourceColumnId.HasValue)
                {
                    var column = await _dataDictionaryService.GetColumnAsync(sourceColumnId.Value);
                    if (column != null)
                    {
                        // Set the source column ID
                        LineageRelationship.SourceColumnId = sourceColumnId.Value;
                        
                        // Set the source table ID and load its columns
                        LineageRelationship.SourceTableId = column.TableId;
                        var columns = await _dataDictionaryService.GetColumnsAsync(column.TableId);
                        SourceColumns = new SelectList(columns, "ColumnId", "ColumnName", sourceColumnId.Value);
                        
                        // Set the source database ID and load its tables
                        var table = await _dataDictionaryService.GetTableAsync(column.TableId);
                        if (table?.DatabaseObject?.Database != null)
                        {
                            LineageRelationship.SourceDatabaseId = table.DatabaseObject.DatabaseId;
                            var tables = await _dataDictionaryService.GetTablesAsync(table.DatabaseObject.DatabaseId);
                            SourceTables = new SelectList(tables, "TableId", "DatabaseObject.ObjectName", column.TableId);
                        }
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data for lineage creation form");
                TempData["ErrorMessage"] = "An error occurred while loading the form data.";
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload dropdowns if validation fails
                    var lineageTypes = await _dataDictionaryService.GetLineageTypesAsync();
                    LineageTypes = new SelectList(lineageTypes, "LineageTypeId", "TypeName");

                    var databases = await _dataDictionaryService.GetDatabasesAsync();
                    Databases = new SelectList(databases, "DatabaseId", "DatabaseName");

                    return Page();
                }

                // Validate that at least one source and one target is specified
                if ((LineageRelationship.SourceDatabaseId == null && 
                     LineageRelationship.SourceTableId == null && 
                     LineageRelationship.SourceColumnId == null) ||
                    (LineageRelationship.TargetDatabaseId == null && 
                     LineageRelationship.TargetTableId == null && 
                     LineageRelationship.TargetColumnId == null))
                {
                    ModelState.AddModelError(string.Empty, "At least one source and one target must be specified.");
                    
                    // Reload dropdowns
                    var lineageTypes = await _dataDictionaryService.GetLineageTypesAsync();
                    LineageTypes = new SelectList(lineageTypes, "LineageTypeId", "TypeName");

                    var databases = await _dataDictionaryService.GetDatabasesAsync();
                    Databases = new SelectList(databases, "DatabaseId", "DatabaseName");
                    
                    return Page();
                }

                // Validate that transformation description is provided for Aggregated and Computation types
                if ((LineageRelationship.LineageTypeId == 2 || LineageRelationship.LineageTypeId == 3) && 
                    string.IsNullOrWhiteSpace(LineageRelationship.TransformationDescription))
                {
                    ModelState.AddModelError("LineageRelationship.TransformationDescription", 
                        "Transformation description is required for Aggregated and Computation lineage types.");
                    
                    // Reload dropdowns
                    var lineageTypes = await _dataDictionaryService.GetLineageTypesAsync();
                    LineageTypes = new SelectList(lineageTypes, "LineageTypeId", "TypeName");

                    var databases = await _dataDictionaryService.GetDatabasesAsync();
                    Databases = new SelectList(databases, "DatabaseId", "DatabaseName");
                    
                    return Page();
                }

                // Add the lineage relationship
                await _dataDictionaryService.AddDataLineageAsync(
                    LineageRelationship.LineageTypeId,
                    LineageRelationship.SourceDatabaseId,
                    LineageRelationship.SourceTableId,
                    LineageRelationship.SourceColumnId,
                    LineageRelationship.TargetDatabaseId,
                    LineageRelationship.TargetTableId,
                    LineageRelationship.TargetColumnId,
                    LineageRelationship.TransformationDescription);

                TempData["SuccessMessage"] = "Data lineage relationship created successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating data lineage relationship");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                
                // Reload dropdowns
                var lineageTypes = await _dataDictionaryService.GetLineageTypesAsync();
                LineageTypes = new SelectList(lineageTypes, "LineageTypeId", "TypeName");

                var databases = await _dataDictionaryService.GetDatabasesAsync();
                Databases = new SelectList(databases, "DatabaseId", "DatabaseName");
                
                return Page();
            }
        }
    }
} 