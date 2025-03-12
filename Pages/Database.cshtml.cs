using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using DataDictionary.Models;
using DataDictionary.Services;

namespace DataDictionary.Pages
{
    public class DatabaseModel : PageModel
    {
        private readonly DataDictionaryService _dataDictionaryService;
        private readonly ILogger<DatabaseModel> _logger;

        public DatabaseModel(DataDictionaryService dataDictionaryService, ILogger<DatabaseModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        public List<ServerModel> Servers { get; set; } = new List<ServerModel>();
        public List<Models.DatabaseModel> DatabaseModelList { get; set; } = new List<Models.DatabaseModel>();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get servers
                var servers = await _dataDictionaryService.GetServersAsync();
                if (servers != null)
                {
                    Servers = servers.ToList();
                }

                // Get databases
                var databases = await _dataDictionaryService.GetDatabasesAsync();
                if (databases != null)
                {
                    DatabaseModelList = databases.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database information");
                ErrorMessage = $"Error retrieving database information: {ex.Message}";
            }
        }
    }
} 