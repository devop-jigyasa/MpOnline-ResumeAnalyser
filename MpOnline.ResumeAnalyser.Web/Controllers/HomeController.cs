using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MpOnline.ResumeAnalyser.Web.Models;
using System.Diagnostics;

namespace MpOnline.ResumeAnalyser.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [AllowAnonymous]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile resumeFile)
    {
        if (resumeFile == null || resumeFile.Length == 0)
            return View("Index");

        var client = _httpClientFactory.CreateClient("ResumeApi");
        using var content = new MultipartFormDataContent();
        using var stream = resumeFile.OpenReadStream();
        content.Add(new StreamContent(stream), "file", resumeFile.FileName);

        try
        {
            var response = await client.PostAsync("api/resume/upload", content);
            if (response.IsSuccessStatusCode)
            {
                var rawResult = await response.Content.ReadAsStringAsync();
                var cleanedJson = CleanMarkdownBlocks(rawResult);
                return View("Result", new ResultViewModel { AiFeedback = cleanedJson });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return View("Result", new ResultViewModel
            {
                AiFeedback = $"API Service Error (Status {response.StatusCode}):\n{errorDetails}"
            });
        }
        catch (Exception ex)
        {
            return View("Result", new ResultViewModel
            {
                AiFeedback = $"Failed to reach Resume API: {ex.Message}\nPlease ensure the backend API is running."
            });
        }
    }

    [HttpGet]
    public IActionResult JobMatch() => View();

    [HttpPost]
    public async Task<IActionResult> AnalyzeJobMatch(IFormFile resumeFile, string jobDescription)
    {
        if (resumeFile == null || string.IsNullOrWhiteSpace(jobDescription))
            return RedirectToAction("JobMatch");

        var client = _httpClientFactory.CreateClient("ResumeApi");
        using var content = new MultipartFormDataContent();
        using var stream = resumeFile.OpenReadStream();
        content.Add(new StreamContent(stream), "file", resumeFile.FileName);
        content.Add(new StringContent(jobDescription), "jobDescription");

        try
        {
            var response = await client.PostAsync("api/job/match", content);
            if (response.IsSuccessStatusCode)
            {
                var rawResult = await response.Content.ReadAsStringAsync();
                return View("Result", new ResultViewModel { AiFeedback = rawResult });
            }

            return View("Result", new ResultViewModel { AiFeedback = $"Match evaluation failed with status: {response.StatusCode}" });
        }
        catch (Exception ex)
        {
            return View("Result", new ResultViewModel { AiFeedback = $"Cannot reach Job Match API: {ex.Message}" });
        }
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static string CleanMarkdownBlocks(string input)
    {
        var cleaned = input.Trim();
        if (cleaned.StartsWith("```json")) cleaned = cleaned[7..];
        else if (cleaned.StartsWith("```")) cleaned = cleaned[3..];
        if (cleaned.EndsWith("```")) cleaned = cleaned[..^3];
        return cleaned.Trim();
    }
}
