using FamilyAI.Application.Common.Interfaces;

namespace FamilyAI.Infrastructure.Services;

public class IntentParser : IIntentParser
{
    public ParsedIntent ParseIntent(string text)
    {
        var clean = text.ToLowerInvariant().Trim();

        // App Launchers
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

        // Deck Launchers
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

        return new ParsedIntent { IsCommand = false };
    }
}
