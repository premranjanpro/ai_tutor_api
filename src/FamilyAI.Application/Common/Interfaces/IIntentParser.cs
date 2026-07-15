using System.Threading;
using System.Threading.Tasks;

namespace FamilyAI.Application.Common.Interfaces;

public interface IIntentParser
{
    Task<ParsedIntent> ParseIntentAsync(string text, CancellationToken cancellationToken = default);
}

public class ParsedIntent
{
    public bool IsCommand { get; set; }
    public string CommandType { get; set; } = string.Empty; // launch_app, launch_deck
    public string Target { get; set; } = string.Empty; // facebook, instagram, ola, math, english, hindi
    public string CustomResponse { get; set; } = string.Empty;
}
