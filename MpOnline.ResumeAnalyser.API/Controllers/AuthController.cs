using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MpOnline.ResumeAnalyser.API.Models;

namespace MpOnline.ResumeAnalyser.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            return BadRequest("Email already exists.");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "User registered successfully", UserId = user.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginUser)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginUser.Email && u.PasswordHash == loginUser.PasswordHash);

        if (user == null)
            return Unauthorized("Invalid credentials.");

        return Ok(new { Message = "Login successful", UserId = user.Id, Email = user.Email, FullName = user.FullName });
    }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
