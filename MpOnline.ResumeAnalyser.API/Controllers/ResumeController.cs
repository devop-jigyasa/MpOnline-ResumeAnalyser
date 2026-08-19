using Microsoft.AspNetCore.Mvc;
using MpOnline.ResumeAnalyser.API.Models;
using MpOnline.ResumeAnalyser.API.Services;
using System.Text;
using UglyToad.PdfPig;

namespace MpOnline.ResumeAnalyser.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly GeminiAiService _aiService;
    private readonly ApplicationDbContext _db;

    public ResumeController(GeminiAiService aiService, ApplicationDbContext db)
    {
        _aiService = aiService;
        _db = db;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0) 
            return BadRequest("Invalid or empty file.");

        var extractedText = new StringBuilder();

        // 1. Read uploaded PDF into memory
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // 2. Extract text using PdfPig
            using (var document = PdfDocument.Open(memoryStream))
            {
                foreach (var page in document.GetPages())
                {
                    extractedText.Append(page.Text).Append(" ");
                }
            }
        }

        // 3. Send text to Gemini AI for ATS scoring
        var rawText = extractedText.ToString();
        var aiJsonResponse = await _aiService.AnalyzeResumeAsync(rawText);

        // 4. Save record into database
        var resumeRecord = new Resume
        {
            UserId = 1,
            FileName = file.FileName,
            ExtractedText = rawText,
            AiAnalysisJson = aiJsonResponse
        };

        _db.Resumes.Add(resumeRecord);
        await _db.SaveChangesAsync();

        // 5. Return JSON payload
        return Content(aiJsonResponse, "application/json");
    }
}
