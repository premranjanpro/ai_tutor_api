using System;

namespace FamilyAI.Contracts.Conversation;

public record StartConversationRequest(
    Guid MemberId,
    string ConversationType
);
