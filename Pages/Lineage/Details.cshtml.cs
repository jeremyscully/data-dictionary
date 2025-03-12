using System;
using System.Threading.Tasks;
using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Pages.Lineage
{
    public class DetailsModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IDataDictionaryService dataDictionaryService, ILogger<DetailsModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        public DataLineageModel LineageRelationship { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                LineageRelationship = await _dataDictionaryService.GetDataLineageRelationshipAsync(id);

                if (LineageRelationship == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data lineage relationship with ID {LineageId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the data lineage relationship.";
                return RedirectToPage("/Error");
            }
        }
    }
} 