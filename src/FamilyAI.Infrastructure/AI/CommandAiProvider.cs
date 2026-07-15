using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyAI.Infrastructure.AI;

public class CommandAiProvider : ICommandAiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<CommandAiProvider> _logger;

    public CommandAiProvider(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<CommandAiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        var apiKey = _config["CommandAi:ApiKey"] ?? Environment.GetEnvironmentVariable("COMMAND_AI__APIKEY");
        var model = _config["CommandAi:Model"] ?? "gemini-2.0-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Command AI API key is not configured. Falling back to local classifier.");
            return "UNKNOWN";
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
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\nUser Message: {userPrompt}" }
                        }
                    }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Command AI API error: {Status} - {Error}", response.StatusCode, err);
                return "UNKNOWN";
            }

            var resData = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(resData);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text?.Trim() ?? "UNKNOWN";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Command AI completion.");
            return "UNKNOWN";
        }
    }
}
