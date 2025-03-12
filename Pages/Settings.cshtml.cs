using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DataDictionary.Pages
{
    public class SettingsModel : PageModel
    {
        public void OnGet()
        {
            // No specific server-side logic needed for theme settings
            // as themes are managed client-side with JavaScript and localStorage
        }
    }
} 