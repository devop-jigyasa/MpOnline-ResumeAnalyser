using Microsoft.AspNetCore.Mvc;
using MpOnline.ResumeAnalyser.API.Services;
using System.Text;
using UglyToad.PdfPig;

namespace MpOnline.ResumeAnalyser.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly GeminiAiService _aiService;

    public JobController(GeminiAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("match")]
    public async Task<IActionResult> MatchJob([FromForm] string jobDescription, IFormFile file)
    {
        if (string.IsNullOrEmpty(jobDescription) || file == null || file.Length == 0)
            return BadRequest("Job description and resume file are required.");

        var extractedText = new StringBuilder();
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using (var document = PdfDocument.Open(memoryStream))
            {
                foreach (var page in document.GetPages())
                {
                    extractedText.Append(page.Text).Append(" ");
                }
            }
        }

        var resumeText = extractedText.ToString();
        var prompt = $"Compare this resume to the following job description. Identify matching skills, missing skills, keyword gaps, and provide a job readiness score with actionable advice.\n\nJob: {jobDescription}\n\nResume: {resumeText}";

        var result = await _aiService.GenerateContentAsync(prompt);
        return Content(result, "text/plain");
    }
}
