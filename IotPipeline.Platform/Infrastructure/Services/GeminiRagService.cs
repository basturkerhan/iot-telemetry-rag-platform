using IotPipeline.Platform.Common.Interfaces;
using System.Text.Json;

namespace IotPipeline.Platform.Infrastructure;

public class GeminiRagService(HttpClient httpClient, IConfiguration configuration)
    : IRagService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = configuration["GeminiSettings:ApiKey"]
        ?? throw new ArgumentException("Gemini API Key not found!");

    public async Task<string> AskQuestionWithContextAsync(
        string question,
        string context,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={_apiKey}";

        var prompt = $"""
        You are an IoT and Industrial Facility Analysis Assistant. Below is the real-time telemetry data (context) retrieved from the database:

        --- CONTEXT DATA ---
        {context}
        ----------------------

        User's Question: {question}

        Please provide a clear response in English by analyzing only the provided context data. If the given context is insufficient to answer the question, please state so.
        """;

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "Response could not be generated.";
    }
}
