using Microsoft.EntityFrameworkCore;
using MpOnline.ResumeAnalyser.API.Models;
using MpOnline.ResumeAnalyser.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. In-Memory Database for testing and evaluation
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("MpOnlineResumeDb"));

// 2. CORS configuration for frontend web app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<GeminiAiService>();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontendApp");
app.UseAuthorization();

// Health check endpoints for monitoring and cloud readiness
app.MapHealthChecks("/health");
app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy", service = "MpOnline.ResumeAnalyser.API", timestamp = DateTime.UtcNow }));

app.MapControllers();

// Database Seeder
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any(u => u.Email == "admin@mponline.gov.in"))
    {
        db.Users.Add(new User
        {
            FullName = "MPOnline Portal Evaluator",
            Email = "admin@mponline.gov.in",
            PasswordHash = "admin123",
            Role = "administrator",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

app.Run();
