using System;
using System.Threading.Tasks;
using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Pages.Lineage
{
    public class DeleteModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IDataDictionaryService dataDictionaryService, ILogger<DeleteModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        [BindProperty]
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
                _logger.LogError(ex, "Error retrieving data lineage relationship with ID {LineageId} for deletion", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the data lineage relationship.";
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var lineageId = LineageRelationship.LineageId;
                
                // Check if the lineage relationship exists
                var existingLineage = await _dataDictionaryService.GetDataLineageRelationshipAsync(lineageId);
                if (existingLineage == null)
                {
                    return NotFound();
                }

                // Delete the lineage relationship
                await _dataDictionaryService.DeleteDataLineageAsync(lineageId);

                TempData["SuccessMessage"] = "Data lineage relationship deleted successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting data lineage relationship with ID {LineageId}", LineageRelationship.LineageId);
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                
                // Reload the lineage relationship to display it again
                LineageRelationship = await _dataDictionaryService.GetDataLineageRelationshipAsync(LineageRelationship.LineageId);
                
                return Page();
            }
        }
    }
} 