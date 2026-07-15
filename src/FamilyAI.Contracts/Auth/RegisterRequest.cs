namespace FamilyAI.Contracts.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string MobileNumber,
    string Password,
    string Country,
    string PreferredLanguage
);
