using System;

namespace FamilyAI.Domain.Entities;

public class AiPromptTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PromptCode { get; set; } = string.Empty; // e.g. child_conversation
    public string PromptName { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string Provider { get; set; } = "Gemini";
    public string Model { get; set; } = "gemini-1.5-flash";
    public int Version { get; set; } = 1;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 800;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
