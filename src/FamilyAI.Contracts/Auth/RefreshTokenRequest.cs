namespace FamilyAI.Contracts.Auth;

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
