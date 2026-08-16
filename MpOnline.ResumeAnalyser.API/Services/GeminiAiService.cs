using Google.GenAI;

namespace MpOnline.ResumeAnalyser.API.Services;

public class GeminiAiService
{
    private readonly Client _client;

    public GeminiAiService(IConfiguration config)
    {
        var apiKey = config["Gemini:ApiKey"] ?? string.Empty;
        _client = new Client(apiKey: apiKey);
    }

    public async Task<string> AnalyzeResumeAsync(string resumeText)
    {
        var prompt = $@"Analyze this resume and provide an ATS score. 
You MUST return ONLY valid JSON matching this exact structure, with no markdown formatting:
{{
  ""candidate_name"": ""Extracted Name"",
  ""target_role"": ""Inferred Role"",
  ""overall_ats_score"": 85,
  ""score_breakdown"": {{
    ""formatting_and_parsability"": 90,
    ""keyword_match_and_density"": 85,
    ""section_structure"": 90,
    ""impact_and_metrics"": 80,
    ""experience_and_education"": 85
  }},
  ""ats_parsability_analysis"": {{
    ""contact_info_detected"": true,
    ""education_detected"": true,
    ""experience_detected"": true,
    ""skills_section_detected"": true
  }},
  ""improvement_suggestions"": [
    ""Replace passive verbs with strong action words like 'Engineered' or 'Spearheaded'."",
    ""Add explicit metrics to your projects to show scale.""
  ],
  ""unsubstantiated_skills"": [
    ""You listed 'React' in skills, but it is missing from your project experience.""
  ],
  ""ai_decision_rationale"": ""The candidate scored 85 because they possess strong technical keywords, but lost points due to a lack of quantifiable metrics in recent projects.""
}}

Resume Text:
{resumeText}";

        var response = await _client.Models.GenerateContentAsync("gemini-3.6-flash", prompt);
        return response.Text ?? string.Empty;
    }

    public async Task<string> GenerateContentAsync(string prompt)
    {
        var response = await _client.Models.GenerateContentAsync("gemini-3.6-flash", prompt);
        return response.Text ?? string.Empty;
    }
}
