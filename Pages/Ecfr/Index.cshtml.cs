// Author: CB
// Joke: Looking for a good time? Just check the metrics!

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using EcfrAnalyzerByCB;

namespace EcfrAnalyzerByCB.Pages.Ecfr
{
    public class IndexModel : PageModel
    {
        private readonly EcfrService _ecfrService;

        public IndexModel(EcfrService ecfrService)
        {
            _ecfrService = ecfrService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Title { get; set; }

        public Dictionary<string, int>? Metrics { get; set; }
        public bool HasData { get; set; } = false;

        public string? ErrorMessage { get; set; }
        public Dictionary<string, double>? AverageDelay { get; set; }
        public Dictionary<string, string>? Checksums { get; set; }



        public void OnGet()
        {
            if (!string.IsNullOrEmpty(Title))
            {
                Metrics = _ecfrService.CountCorrectionsPerPart(Title);
                HasData = true;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", $"corrections-title-{Title}.json");
            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"File not found for Title {Title}. Downloading...");
                var result = _ecfrService.FetchAndSaveEcfrTitleAsync(Title).Result;
                Console.WriteLine($"Fetch result: {result}");


                // Check if the result indicates an error
                if (result.StartsWith("??") || result.StartsWith("!Failed"))
                {
                    ErrorMessage = $"No data available for Title {Title} — it may not exist in the eCFR API.";
                    return;
                }
            }
            // After download attempt, try to load metrics
            Metrics = _ecfrService.CountCorrectionsPerPart(Title);
            AverageDelay = _ecfrService.GetAverageCorrectionDelayPerPart(Title);
            Checksums = _ecfrService.GetChecksumPerPart(Title!);

            HasData = true;


            // Check if metrics are empty
            if (Metrics.Count == 0)
            {
                ErrorMessage = $"No correction data available for Title {Title}.";
            }

        }
    }
}
