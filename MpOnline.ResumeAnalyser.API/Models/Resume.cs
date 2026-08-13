namespace MpOnline.ResumeAnalyser.API.Models;

public class Resume
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string FileName { get; set; }
    public required string ExtractedText { get; set; }

    // Storing structured AI JSON analysis payload
    public string? AiAnalysisJson { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
