// Author: CB
// Joke: API endpoints are like pizza slices — there's always room for one more 

using Microsoft.AspNetCore.Mvc;
using EcfrAnalyzerByCB;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class EcfrController : ControllerBase
{
    private readonly EcfrService _ecfrService;

    public EcfrController(EcfrService ecfrService)
    {
        _ecfrService = ecfrService;
    }

    // GET /api/ecfr/49
    [HttpGet("{titleNumber}")]
    public async Task<IActionResult> DownloadEcfrTitle(string titleNumber)
    {
        Console.WriteLine($"Controller triggered with Title {titleNumber}");

        var message = await _ecfrService.FetchAndSaveEcfrTitleAsync(titleNumber);
        return Ok(message);
    }

    [HttpGet("{titleNumber}/metrics/corrections-per-part")]
    public IActionResult GetCorrectionMetrics(string titleNumber)
    {
        Console.WriteLine($" Correction metric endpoint hit for title {titleNumber}");

        var result = _ecfrService.CountCorrectionsPerPart(titleNumber);
        return Ok(result);
    }
    [HttpGet("{titleNumber}/metrics/average-delay")]
    public IActionResult GetAverageDelayPerPart(string titleNumber)
    {
        var result = _ecfrService.GetAverageCorrectionDelayPerPart(titleNumber);
        return Ok(result);
    }

    [HttpGet("{titleNumber}/metrics/checksums")]
    public IActionResult GetChecksums(string titleNumber)
    {
        var result = _ecfrService.GetChecksumPerPart(titleNumber);
        return Ok(result);
    }


}
