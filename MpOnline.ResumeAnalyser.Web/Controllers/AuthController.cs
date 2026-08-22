using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MpOnline.ResumeAnalyser.Web.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MpOnline.ResumeAnalyser.Web.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _httpClientFactory.CreateClient("ResumeApi");
        var payload = new { Email = model.Email, PasswordHash = model.Password };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim(ClaimTypes.Email, model.Email)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Cannot connect to Auth API: {ex.Message}";
            return View(model);
        }

        ViewBag.Error = "Invalid email or password.";
        return View(model);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _httpClientFactory.CreateClient("ResumeApi");
        var payload = new
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = model.Password,
            Role = "job_seeker"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("api/auth/register", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Cannot connect to Auth API: {ex.Message}";
            return View(model);
        }

        ViewBag.Error = "Registration failed. Email might already exist.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
