using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Pages.Lineage
{
    public class IndexModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IDataDictionaryService dataDictionaryService, ILogger<IndexModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        public IEnumerable<DataLineageModel> LineageRelationships { get; set; } = new List<DataLineageModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                LineageRelationships = await _dataDictionaryService.GetDataLineageRelationshipsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data lineage relationships");
                TempData["ErrorMessage"] = "An error occurred while retrieving data lineage relationships.";
                return RedirectToPage("/Error");
            }
        }
    }
} 