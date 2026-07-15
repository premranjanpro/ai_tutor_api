using System;

namespace FamilyAI.Contracts.Conversation;

public record ConversationSessionResponse(
    Guid Id,
    Guid FamilyId,
    Guid MemberId,
    string ConversationType,
    string AiProvider,
    string AiModel,
    DateTime StartedAt,
    string Status
);
