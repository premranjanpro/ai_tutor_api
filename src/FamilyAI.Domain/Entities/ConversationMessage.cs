using System;
using FamilyAI.Domain.Enums;

namespace FamilyAI.Domain.Entities;

public class ConversationMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public SenderType SenderType { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public MessageType MessageType { get; set; } = MessageType.Text;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ConversationSession? Session { get; set; }
}
