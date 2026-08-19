using Microsoft.AspNetCore.Mvc;
using MpOnline.ResumeAnalyser.API.Services;

namespace MpOnline.ResumeAnalyser.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewController : ControllerBase
{
    private readonly GeminiAiService _aiService;

    public InterviewController(GeminiAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("generate-questions")]
    public async Task<IActionResult> GenerateQuestions([FromBody] string resumeText)
    {
        if (string.IsNullOrEmpty(resumeText))
            return BadRequest("Resume text is required.");

        var prompt = $"Based on this candidate's resume, generate 3 Technical interview questions, 2 Project-specific questions, and 2 Behavioral questions with answer key guidelines.\n\nResume: {resumeText}";

        var result = await _aiService.GenerateContentAsync(prompt);
        return Ok(new { Questions = result });
    }
}
