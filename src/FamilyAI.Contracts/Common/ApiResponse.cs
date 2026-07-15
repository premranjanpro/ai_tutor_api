namespace FamilyAI.Contracts.Common;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string>? Errors { get; init; }

    public static ApiResponse<T> SuccessResponse(T? data, string? message = null) =>
        new() { Success = true, Data = data, Message = message, Errors = null };

    public static ApiResponse<T> FailureResponse(List<string> errors, string? message = null) =>
        new() { Success = false, Data = default, Message = message, Errors = errors };

    public static ApiResponse<T> FailureResponse(string error, string? message = null) =>
        new() { Success = false, Data = default, Message = message, Errors = new List<string> { error } };
}
