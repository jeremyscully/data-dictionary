using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataDictionary.Services;
using DataDictionary.Models;

namespace DataDictionary.Pages.Tables
{
    public class DetailsModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;

        public DetailsModel(IDataDictionaryService dataDictionaryService)
        {
            _dataDictionaryService = dataDictionaryService;
        }

        public TableDefinitionModel Table { get; set; }
        public IEnumerable<ColumnDefinitionModel> Columns { get; set; }
        public IEnumerable<TableRelationshipModel> ParentRelationships { get; set; }
        public IEnumerable<TableRelationshipModel> ChildRelationships { get; set; }
        public IEnumerable<DataLineageModel> SourceLineages { get; set; } = new List<DataLineageModel>();
        public IEnumerable<DataLineageModel> TargetLineages { get; set; } = new List<DataLineageModel>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // Get table details
                Table = await _dataDictionaryService.GetTableAsync(id);

                if (Table == null)
                {
                    return NotFound();
                }

                // Get columns
                Columns = await _dataDictionaryService.GetColumnsAsync(id);

                // Get relationships
                ParentRelationships = await _dataDictionaryService.GetParentRelationshipsAsync(id);
                ChildRelationships = await _dataDictionaryService.GetChildRelationshipsAsync(id);
                
                // Get lineage relationships
                SourceLineages = await _dataDictionaryService.GetTableLineageAsync(id, true);
                TargetLineages = await _dataDictionaryService.GetTableLineageAsync(id, false);

                return Page();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error retrieving table details: {ex.Message}");
                return RedirectToPage("/Error");
            }
        }
    }
} 