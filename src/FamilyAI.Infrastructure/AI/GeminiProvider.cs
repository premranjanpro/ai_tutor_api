using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyAI.Infrastructure.AI;

public class GeminiProvider : IAiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiProvider> _logger;

    public string ProviderName => "Gemini";

    public GeminiProvider(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<GeminiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        var apiKey = _config["AiSettings:Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("AI__GEMINI__APIKEY");
        var model = _config["AiSettings:Gemini:Model"] ?? "gemini-1.5-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Falling back to Mock Robotic AI response.");
            return GetMockRoboticResponse(systemPrompt, userPrompt);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userPrompt } }
                    }
                },
                systemInstruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                }
            };

            var response = await client.PostAsJsonAsync(url, requestBody, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API returned error: {StatusCode} - {Error}", response.StatusCode, errorText);
                return $"[Error] Failed to connect to Gemini API. Status: {response.StatusCode}. Mock response fallback: {GetMockRoboticResponse(systemPrompt, userPrompt)}";
            }

            var resultJson = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var textResponse = resultJson
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return textResponse ?? "I could not generate a response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Gemini API call.");
            return $"[Exception] {ex.Message}. Mock response fallback: {GetMockRoboticResponse(systemPrompt, userPrompt)}";
        }
    }

    private static string GetMockRoboticResponse(string systemPrompt, string userPrompt)
    {
        // Extract kid details from the system prompt to customize the robotic response
        string kidName = "Aarav";
        string kidAge = "8";

        if (systemPrompt.Contains("Name: "))
        {
            var lines = systemPrompt.Split('\n');
            var nameLine = lines.FirstOrDefault(l => l.StartsWith("Name: "));
            if (nameLine != null) kidName = nameLine.Replace("Name: ", "").Trim();

            var ageLine = lines.FirstOrDefault(l => l.StartsWith("Age: "));
            if (ageLine != null) kidAge = ageLine.Replace("Age: ", "").Trim();
        }

        // Simulating the robotic learning companion
        if (userPrompt.Contains("hello") || userPrompt.Contains("hi") || userPrompt.Contains("namaste"))
        {
            return $"[Robotic Companion Voice] Beep Boop! Hello {kidName}! I am Ghar Ka AI Teacher, your robotic learning companion. I remember you are {kidAge} years old. Today, let's learn something fun! What is your favorite subject?";
        }

        if (userPrompt.Contains("math") || userPrompt.Contains("mathematics") || userPrompt.Contains("numbers"))
        {
            return $"[Robotic Companion Voice] Accessing mathematics databases. Beep! {kidName}, let's practice addition. Since you are {kidAge} years old, I have generated a question: What is 5 + 3?";
        }

        return $"[Robotic Companion Voice] Processing input from {kidName} (Age: {kidAge}). Beep! I am an AI learning assistant. That sounds fascinating! Tell me more about it or ask me a question.";
    }
}
