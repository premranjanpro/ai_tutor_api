using System.Threading;
using System.Threading.Tasks;

namespace FamilyAI.Application.Common.Interfaces;

public interface IAiProvider
{
    string ProviderName { get; }
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}
