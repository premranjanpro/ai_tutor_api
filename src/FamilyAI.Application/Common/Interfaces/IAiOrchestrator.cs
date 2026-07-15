using System;
using System.Threading;
using System.Threading.Tasks;
using FamilyAI.Contracts.Common;

namespace FamilyAI.Application.Common.Interfaces;

public interface IAiOrchestrator
{
    Task<ApiResponse<string>> SendMessageAsync(Guid sessionId, string userMessage, CancellationToken cancellationToken);
}
