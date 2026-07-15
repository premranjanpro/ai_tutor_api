using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using FamilyAI.Contracts.Common;
using FamilyAI.Domain.Entities;
using FamilyAI.Domain.Enums;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Infrastructure.AI;

public class AiOrchestrator : IAiOrchestrator
{
    private readonly AppDbContext _context;
    private readonly IAiProvider _aiProvider;

    public AiOrchestrator(AppDbContext context, IAiProvider aiProvider)
    {
        _context = context;
        _aiProvider = aiProvider;
    }

    public async Task<ApiResponse<string>> SendMessageAsync(Guid sessionId, string userMessage, CancellationToken cancellationToken)
    {
        var session = await _context.ConversationSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null)
        {
            return ApiResponse<string>.FailureResponse("Conversation session not found.");
        }

        var member = await _context.FamilyMembers
            .FirstOrDefaultAsync(m => m.Id == session.MemberId, cancellationToken);

        if (member == null)
        {
            return ApiResponse<string>.FailureResponse("Family member associated with this session not found.");
        }

        // Get system prompt template
        var promptTemplate = await _context.AiPromptTemplates
            .FirstOrDefaultAsync(p => p.PromptCode == "child_conversation" && p.IsActive, cancellationToken);

        if (promptTemplate == null)
        {
            return ApiResponse<string>.FailureResponse("System prompt template not found.");
        }

        // Save User Message
        var userMsgEntity = new ConversationMessage
        {
            SessionId = session.Id,
            SenderType = SenderType.User,
            MessageText = userMessage,
            MessageType = MessageType.Text
        };
        await _context.ConversationMessages.AddAsync(userMsgEntity, cancellationToken);

        // Build prompt context
        var systemPrompt = BuildSystemPrompt(promptTemplate.SystemPrompt, member);
        var userPrompt = BuildUserPrompt(session.Messages.Concat(new[] { userMsgEntity }));

        // Execute AI Completion
        var aiResponseText = await _aiProvider.CompleteAsync(systemPrompt, userPrompt, cancellationToken);

        // Save AI Message
        var aiMsgEntity = new ConversationMessage
        {
            SessionId = session.Id,
            SenderType = SenderType.Ai,
            MessageText = aiResponseText,
            MessageType = MessageType.Text
        };
        await _context.ConversationMessages.AddAsync(aiMsgEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.SuccessResponse(aiResponseText, "AI response generated successfully.");
    }

    private static string BuildSystemPrompt(string template, FamilyMember member)
    {
        return template
            .Replace("{{member_name}}", string.IsNullOrWhiteSpace(member.Nickname) ? member.FullName : member.Nickname)
            .Replace("{{age}}", member.Age.ToString())
            .Replace("{{preferred_language}}", member.PreferredLanguage)
            .Replace("{{class_name}}", string.IsNullOrWhiteSpace(member.ClassName) ? "Not Specified" : member.ClassName)
            .Replace("{{interests}}", member.InterestsJson)
            .Replace("{{learning_goals}}", member.LearningGoalsJson)
            .Replace("{{current_datetime}}", DateTime.UtcNow.ToString("F"))
            .Replace("{{today_study_plan}}", "Review multiplication tables and basic addition.")
            .Replace("{{approved_memories}}", "No memories saved yet.");
    }

    private static string BuildUserPrompt(IEnumerable<ConversationMessage> messages)
    {
        // Take last 6 messages to keep context short and relevant
        var list = messages.OrderByDescending(m => m.CreatedAt).Take(6).Reverse().ToList();
        
        var prompt = "Recent conversation history:\n";
        foreach (var msg in list)
        {
            var senderName = msg.SenderType == SenderType.User ? "User" : "AI";
            prompt += $"[{senderName}]: {msg.MessageText}\n";
        }
        
        prompt += "\nPlease respond to the User's latest message based on the history above and your core system prompt.";
        return prompt;
    }
}
