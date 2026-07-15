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
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.Id == session.MemberId, cancellationToken);

        if (member == null)
        {
            return ApiResponse<string>.FailureResponse("Family member associated with this session not found.");
        }

        // YouTube screen-time blocker logic:
        var cleanMsg = userMessage.ToLowerInvariant();
        if (cleanMsg.Contains("youtube") || cleanMsg.Contains("watch movie") || cleanMsg.Contains("play movie"))
        {
            var custom = member.Family?.ParentCustomInstructions ?? string.Empty;
            if (custom.ToLowerInvariant().Contains("1 hour") || custom.ToLowerInvariant().Contains("youtube"))
            {
                var response = "Beep Boop! Papa ke custom rule ke mutabik, abhi YouTube blocked hai. Aapko mobile milega study ke 1 hour baad. Pehle ABC alphabets ya 1+2 complete kijiye!";
                
                var uMsg = new ConversationMessage { SessionId = session.Id, SenderType = SenderType.User, MessageText = userMessage };
                var aMsg = new ConversationMessage { SessionId = session.Id, SenderType = SenderType.Ai, MessageText = response };
                await _context.ConversationMessages.AddRangeAsync(new[] { uMsg, aMsg }, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return ApiResponse<string>.SuccessResponse(response, "YouTube request intercepted by parent custom constraints.");
            }
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
        var custom = member.Family?.ParentCustomInstructions ?? string.Empty;
        var parentRules = string.IsNullOrWhiteSpace(custom)
            ? "No additional parent instructions."
            : $"CRITICAL PARENT CUSTOM GUIDELINES:\n{custom}";

        return template
            .Replace("{{member_name}}", string.IsNullOrWhiteSpace(member.Nickname) ? member.FullName : member.Nickname)
            .Replace("{{age}}", member.Age.ToString())
            .Replace("{{preferred_language}}", member.PreferredLanguage)
            .Replace("{{class_name}}", string.IsNullOrWhiteSpace(member.ClassName) ? "Not Specified" : member.ClassName)
            .Replace("{{interests}}", member.InterestsJson)
            .Replace("{{learning_goals}}", member.LearningGoalsJson)
            .Replace("{{current_datetime}}", DateTime.UtcNow.ToString("F"))
            .Replace("{{today_study_plan}}", "Review multiplication tables and basic addition.")
            .Replace("{{approved_memories}}", $"No memories saved yet.\n\n{parentRules}");
    }

    private static string BuildUserPrompt(IEnumerable<ConversationMessage> messages)
    {
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
