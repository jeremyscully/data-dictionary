using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataDictionary.Pages
{
    public class TableDetailsModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<TableDetailsModel> _logger;

        public TableDetailsModel(IDataDictionaryService dataDictionaryService, ILogger<TableDetailsModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int TableId { get; set; }

        public TableDefinitionModel Table { get; set; }
        public List<ColumnDefinitionModel> Columns { get; set; } = new List<ColumnDefinitionModel>();
        public List<TableRelationshipModel> ParentRelationships { get; set; } = new List<TableRelationshipModel>();
        public List<TableRelationshipModel> ChildRelationships { get; set; } = new List<TableRelationshipModel>();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                if (TableId > 0)
                {
                    // Get table details
                    Table = await _dataDictionaryService.GetTableAsync(TableId);
                    if (Table != null)
                    {
                        // Get columns for the table
                        var columns = await _dataDictionaryService.GetColumnsAsync(TableId);
                        if (columns != null)
                        {
                            Columns = columns.ToList();
                        }

                        // Get parent relationships (where this table is a child)
                        var parentRelationships = await _dataDictionaryService.GetTableRelationshipsAsync(TableId, false);
                        if (parentRelationships != null)
                        {
                            ParentRelationships = parentRelationships.ToList();
                        }

                        // Get child relationships (where this table is a parent)
                        var childRelationships = await _dataDictionaryService.GetTableRelationshipsAsync(TableId, true);
                        if (childRelationships != null)
                        {
                            ChildRelationships = childRelationships.ToList();
                        }
                    }
                    else
                    {
                        ErrorMessage = $"Table with ID {TableId} not found.";
                    }
                }
                else
                {
                    ErrorMessage = "No table ID specified.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for table {TableId}", TableId);
                ErrorMessage = $"Error retrieving table details: {ex.Message}";
            }
        }
    }
} 