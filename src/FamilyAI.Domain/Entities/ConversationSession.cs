using System;
using System.Collections.Generic;
using FamilyAI.Domain.Enums;

namespace FamilyAI.Domain.Entities;

public class ConversationSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Guid MemberId { get; set; }
    public string ConversationType { get; set; } = "General"; // General, Study, Interview
    public string AiProvider { get; set; } = "Gemini";
    public string AiModel { get; set; } = "gemini-1.5-flash";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public string Summary { get; set; } = string.Empty;
    public SessionStatus Status { get; set; } = SessionStatus.Active;

    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
