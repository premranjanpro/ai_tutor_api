using System;

namespace FamilyAI.Contracts.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthUserInfo User
);

public record AuthUserInfo(
    Guid UserId,
    string Name,
    string Role
);
