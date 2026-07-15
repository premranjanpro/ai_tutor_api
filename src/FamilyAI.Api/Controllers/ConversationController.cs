using System;
using System.Threading;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using FamilyAI.Contracts.Common;
using FamilyAI.Contracts.Conversation;
using FamilyAI.Domain.Entities;
using FamilyAI.Domain.Enums;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAiOrchestrator _aiOrchestrator;
    private readonly IIntentParser _intentParser;

    public ConversationController(AppDbContext context, IAiOrchestrator aiOrchestrator, IIntentParser intentParser)
    {
        _context = context;
        _aiOrchestrator = aiOrchestrator;
        _intentParser = intentParser;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ConversationSessionResponse>>> StartConversation([FromBody] StartConversationRequest request, CancellationToken cancellationToken)
    {
        var member = await _context.FamilyMembers.FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);
        if (member == null)
        {
            return BadRequest(ApiResponse<ConversationSessionResponse>.FailureResponse("Family member not found."));
        }

        // Get AI Provider details from settings
        var provider = "Gemini";
        var model = "gemini-1.5-flash";

        var session = new ConversationSession
        {
            FamilyId = member.FamilyId,
            MemberId = member.Id,
            ConversationType = request.ConversationType,
            AiProvider = provider,
            AiModel = model,
            StartedAt = DateTime.UtcNow,
            Status = SessionStatus.Active
        };

        await _context.ConversationSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ConversationSessionResponse(
            session.Id,
            session.FamilyId,
            session.MemberId,
            session.ConversationType,
            session.AiProvider,
            session.AiModel,
            session.StartedAt,
            session.Status.ToString()
        );

        return Ok(ApiResponse<ConversationSessionResponse>.SuccessResponse(response, "Conversation session started successfully."));
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<ApiResponse<string>>> SendMessage(Guid id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        // First run the intent parser
        var parsed = await _intentParser.ParseIntentAsync(request.MessageText, cancellationToken);
        if (parsed.IsCommand)
        {
            var responseText = $"COMMAND:{parsed.CommandType}:{parsed.Target}|{parsed.CustomResponse}";

            // Save messages to database to preserve history log
            var userMsg = new ConversationMessage
            {
                SessionId = id,
                SenderType = SenderType.User,
                MessageText = request.MessageText,
                MessageType = MessageType.Text
            };
            var aiMsg = new ConversationMessage
            {
                SessionId = id,
                SenderType = SenderType.Ai,
                MessageText = responseText,
                MessageType = MessageType.Text
            };

            await _context.ConversationMessages.AddRangeAsync(new[] { userMsg, aiMsg }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<string>.SuccessResponse(responseText, "Command intent parsed successfully."));
        }

        var response = await _aiOrchestrator.SendMessageAsync(id, request.MessageText, cancellationToken);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}
