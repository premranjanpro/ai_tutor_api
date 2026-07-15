using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyAI.Infrastructure.AI;

public class FallbackAiProvider : IAiProvider, ICommandAiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<FallbackAiProvider> _logger;

    public string ProviderName => "FallbackOrchestrator";

    public FallbackAiProvider(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<FallbackAiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        // Load priority order from config
        var priorityOrder = _config.GetSection("AiFallbackSettings:PriorityOrder").Get<List<string>>() 
                            ?? new List<string> { "Gemini", "CommandAi", "ChatGPT", "OpenCode" };

        foreach (var providerName in priorityOrder)
        {
            var providerPath = $"AiFallbackSettings:Providers:{providerName}";
            var apiKey = _config[$"{providerPath}:ApiKey"];
            var model = _config[$"{providerPath}:Model"] ?? "gemini-2.0-flash";
            var urlTemplate = _config[$"{providerPath}:Url"] ?? "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            if (string.IsNullOrWhiteSpace(apiKey) || 
                apiKey.StartsWith("YOUR_") || 
                apiKey == "YOUR_GEMINI_API_KEY_HERE" || 
                apiKey == "YOUR_COMMAND_AI_API_KEY_HERE")
            {
                _logger.LogInformation("Skipping fallback provider {ProviderName} - API key is unconfigured or placeholder.", providerName);
                continue;
            }

            _logger.LogInformation("Attempting AI completion via provider: {ProviderName}...", providerName);

            try
            {
                var responseText = await ExecuteProviderRequestAsync(providerName, apiKey, model, urlTemplate, systemPrompt, userPrompt, cancellationToken);
                if (!string.IsNullOrWhiteSpace(responseText))
                {
                    _logger.LogInformation("Successfully completed AI request using provider: {ProviderName}", providerName);
                    return responseText;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Provider {ProviderName} failed. Attempting next fallback in priority order...", providerName);
            }
        }

        _logger.LogWarning("All configured fallback AI providers failed or were unconfigured. Using Mock local companion response.");
        return GetMockRoboticResponse(systemPrompt, userPrompt);
    }

    private async Task<string> ExecuteProviderRequestAsync(
        string providerName, 
        string apiKey, 
        string model, 
        string urlTemplate, 
        string systemPrompt, 
        string userPrompt, 
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        
        // Formulate target URL
        var targetUrl = urlTemplate.Replace("{model}", model).Replace("{apiKey}", apiKey);
        
        // Handle Gemini or CommandAi format
        if (providerName.Equals("Gemini", StringComparison.OrdinalIgnoreCase) || 
            providerName.Equals("CommandAi", StringComparison.OrdinalIgnoreCase))
        {
            var payload = new
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

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(targetUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var resData = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(resData);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()?.Trim() ?? string.Empty;
        }
        
        // Handle OpenAI format (ChatGPT) or OpenRouter format (OpenCode)
        if (providerName.Equals("ChatGPT", StringComparison.OrdinalIgnoreCase) || 
            providerName.Equals("OpenCode", StringComparison.OrdinalIgnoreCase))
        {
            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Setup Authorization Headers
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            if (providerName.Equals("OpenCode", StringComparison.OrdinalIgnoreCase))
            {
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000");
                client.DefaultRequestHeaders.Add("X-Title", "Family AI Companion");
            }

            var response = await client.PostAsync(targetUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var resData = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(resData);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()?.Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    private static string GetMockRoboticResponse(string systemPrompt, string userPrompt)
    {
        var cleanPrompt = userPrompt.ToLowerInvariant();

        if (cleanPrompt.Contains("math") || cleanPrompt.Contains("1+2"))
        {
            return "[Robotic AI]: Beep Boop! 1 + 2 equals 3. Very good! What shall we learn next?";
        }
        if (cleanPrompt.Contains("hi") || cleanPrompt.Contains("hello"))
        {
            return "[Robotic AI]: Hello Dost! I am your AI learning companion. Ask me a question about space, math, or fruits!";
        }
        if (cleanPrompt.Contains("space") || cleanPrompt.Contains("mars"))
        {
            return "[Robotic AI]: Beep! Mars is called the Red Planet because of iron oxide (rust) on its surface. Let's learn more!";
        }

        return "[Robotic AI]: Beep! That sounds interesting. Tell me more, or let's start a fun study deck together!";
    }
}
