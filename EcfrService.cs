// Author: CB
// Joke: loading....please wait...

using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using EcfrAnalyzerByCB;

/// <summary>
/// This code is designed to Downloads a specific Title from the eCFR API and stores it as a local JSON file.
/// It uses HttpClient to make the API request and handles errors gracefully.
/// All logs help in debugging and understanding the flow of the application.
/// </summary>

namespace EcfrAnalyzerByCB
{
    public class EcfrService
    {
        private readonly HttpClient _httpClient;

        // Constructor with dependency injection for HttpClient
        public EcfrService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// </param>
        /// <param name="titleNumber">The numeric title to fetch (e.g., "14" or "49")</param>
        /// <returns>Success or error message</returns>
        public async Task<string> FetchAndSaveEcfrTitleAsync(string titleNumber)
        {
            string apiUrl = $"https://www.ecfr.gov/api/admin/v1/corrections/title/{titleNumber}";

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                // log Check if the response is successful
                Console.WriteLine($"Requested: {apiUrl}");

                response.EnsureSuccessStatusCode();



                var content = await response.Content.ReadAsStringAsync();

                // Force file to save in current project root
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
                Directory.CreateDirectory(folder);
                // log if the directory exists, to be deleted later
                Console.WriteLine($"Folder created: {folder}");

                var path = Path.Combine(folder, $"corrections-title-{titleNumber}.json");
                var fullPath = Path.GetFullPath(path);

                //log testing my file path
                Console.WriteLine($"Writing file to: {fullPath}");

                await File.WriteAllTextAsync(fullPath, content);

                return $" Data for Title {titleNumber} saved to:\n{fullPath}";
            }
            catch (HttpRequestException httpEx)
            {
                return $"!Failed to fetch from API: {httpEx.Message}";
            }
            catch (IOException ioEx)
            {
                return $"!Failed to save file: {ioEx.Message}";
            }
            catch (System.Exception ex)
            {
                return $"!Sorry Unexpected error: {ex.Message}";
            }
        }
        public Dictionary<string, int> CountCorrectionsPerPart(string titleNumber)
        {
            // Build the path to the saved JSON file
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", $"corrections-title-{titleNumber}.json");

            // If the file doesn't exist, return an empty result and log the issue
            if (!File.Exists(path))
            {
                Console.WriteLine($"!!No File not found at {path}");
                return new Dictionary<string, int>();
            }

            // Read the raw JSON string from the file

            var json = File.ReadAllText(path);

            // Deserialize it into the root structure defined by EcfrCorrectionResponse
            var root = JsonSerializer.Deserialize<EcfrCorrectionResponse>(json);

            // Flatten all CFR references across corrections, group by 'part', and count them
            var grouped = root?.ecfr_corrections?
                .SelectMany(c => c.cfr_references ?? new List<CfrReference>())        // Flatten all references
                .GroupBy(r => r.hierarchy?.part ?? "Unknown")                         // Group by 'part'
                .ToDictionary(g => g.Key, g => g.Count())                             // Count occurrences per part
                ?? new Dictionary<string, int>();                                     // Default empty dictionary

            return grouped;
        }

        public Dictionary<string, double> GetAverageCorrectionDelayPerPart(string titleNumber)
        {
            // Build the path to the saved JSON file
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", $"corrections-title-{titleNumber}.json");

            //check if the file exists, and it does not, return an empty result
            if (!File.Exists(path))
            {
                Console.WriteLine($"!! File not found at {path}");
                return new Dictionary<string, double>();
            }
            // Read the raw JSON and deserialize it (??)
            var json = File.ReadAllText(path);
            var root = JsonSerializer.Deserialize<EcfrCorrectionResponse>(json);

            if (root?.ecfr_corrections == null)
                return new Dictionary<string, double>();

            //process the corrections where both dates are valid
            var partGroups = root.ecfr_corrections
                .Where(c => DateTime.TryParse(c.error_occurred, out _) && DateTime.TryParse(c.error_corrected, out _))
                .SelectMany(c => (c.cfr_references ?? new List<CfrReference>())
                    .Select(r =>
                    {
                        var occurred = DateTime.Parse(c.error_occurred!);
                        var corrected = DateTime.Parse(c.error_corrected!);
                        // Calculate the delay in days between error occurred and corrected ( this will be spicy)
                        var delay = (corrected - occurred).TotalDays;
                        return new { Part = r.hierarchy?.part ?? "Unknown", Delay = delay };
                    }))
                // Group by 'part' and calculate the average delay
                .GroupBy(x => x.Part)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Average(x => x.Delay), 2) // Round for easy readability
                );

            return partGroups;
        }

        //definitely had to look up how to get the checksum. see https://stackoverflow.com/questions/1358510/how-to-compare-2-files-fast-using-net
        public Dictionary<string, string> GetChecksumPerPart(string titleNumber)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", $"corrections-title-{titleNumber}.json");

            if (!File.Exists(path))
            {
                Console.WriteLine($"!! File not found at {path}");
                return new Dictionary<string, string>();
            }

            var json = File.ReadAllText(path);
            var root = JsonSerializer.Deserialize<EcfrCorrectionResponse>(json);

            if (root?.ecfr_corrections == null)
                return new Dictionary<string, string>();

            var checksums = root.ecfr_corrections
                .Where(c => c.cfr_references != null)
                .SelectMany(c => c.cfr_references!.Select(r =>
                {
                    var part = r.hierarchy?.part ?? "Unknown";
                    var data = $"{r.hierarchy?.section}|{c.corrective_action}|{c.error_occurred}|{c.error_corrected}";
                    return new { Part = part, Data = data };
                }))
                .GroupBy(x => x.Part)
                .ToDictionary(
                    g => g.Key,
                    g => ComputeSha256Checksum(string.Join("|", g.Select(x => x.Data)))
                );

            return checksums;
        }

        // Helper function to generate SHA-256 hash
        private static string ComputeSha256Checksum(string rawData)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(rawData);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}