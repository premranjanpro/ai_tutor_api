using System.Threading;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;

namespace FamilyAI.Infrastructure.Services;

public class IntentParser : IIntentParser
{
    private readonly ICommandAiProvider _commandAi;

    public IntentParser(ICommandAiProvider commandAi)
    {
        _commandAi = commandAi;
    }

    public async Task<ParsedIntent> ParseIntentAsync(string text, CancellationToken cancellationToken = default)
    {
        var clean = text.ToLowerInvariant().Trim();

        // 1. Fast Rule-Based Intercepts (No API Quota cost)
        if (clean.Contains("open facebook") || clean.Contains("launch facebook"))
        {
            return new ParsedIntent
            {
                IsCommand = true,
                CommandType = "launch_app",
                Target = "facebook",
                CustomResponse = "Beep Boop! Opening Facebook for you."
            };
        }

        if (clean.Contains("open insta") || clean.Contains("instagram"))
        {
            return new ParsedIntent
            {
                IsCommand = true,
                CommandType = "launch_app",
                Target = "instagram",
                CustomResponse = "Beep Boop! Launching Instagram."
            };
        }

        if (clean.Contains("open ola") || clean.Contains("ola app"))
        {
            return new ParsedIntent
            {
                IsCommand = true,
                CommandType = "launch_app",
                Target = "ola",
                CustomResponse = "Beep Boop! Dispatching Ola app launcher."
            };
        }

        if (clean.Contains("open math") || clean.Contains("study math") || clean.Contains("1+2") || clean.Contains("addition"))
        {
            return new ParsedIntent
            {
                IsCommand = true,
                CommandType = "launch_deck",
                Target = "math",
                CustomResponse = "Beep Boop! Launching your Interactive Mathematics deck. What is 1 + 2?"
            };
        }

        if (clean.Contains("open english") || clean.Contains("study english") || clean.Contains("alphabets"))
        {
            return new ParsedIntent
            {
                IsCommand = true,
                CommandType = "launch_deck",
                Target = "english",
                CustomResponse = "Beep Boop! Loading English Alphabets card deck."
            };
        }

        if (clean.Contains("open hindi") || clean.Contains("study hindi"))
        {
            return new ParsedIntent
            {
                IsCommand = true,
                CommandType = "launch_deck",
                Target = "hindi",
                CustomResponse = "Beep Boop! Hindi card deck ready."
            };
        }

        // 2. AI-Assisted Intent Parsing fallback (using Command API Key)
        var systemPrompt = @"You are a Command Classifier.
Classify the user message into one of these actions:
- launch_app:facebook
- launch_app:instagram
- launch_app:ola
- launch_deck:math
- launch_deck:english
- launch_deck:hindi

If matched, reply exactly in this format: COMMAND:type:target|friendly response text
Example: COMMAND:launch_app:facebook|Opening Facebook
If no action matches, reply exactly: NONE
Do not return any other text or markdown.";

        var aiResponse = await _commandAi.CompleteAsync(systemPrompt, text, cancellationToken);
        if (!string.IsNullOrWhiteSpace(aiResponse) && aiResponse.StartsWith("COMMAND:"))
        {
            var parts = aiResponse.Split('|');
            if (parts.Length > 0)
            {
                var cmd = parts[0].Replace("COMMAND:", "");
                var cmdParts = cmd.Split(':');
                if (cmdParts.Length >= 2)
                {
                    return new ParsedIntent
                    {
                        IsCommand = true,
                        CommandType = cmdParts[0],
                        Target = cmdParts[1],
                        CustomResponse = parts.Length > 1 ? parts[1] : $"Executing {cmdParts[1]}"
                    };
                }
            }
        }

        return new ParsedIntent { IsCommand = false };
    }
}
