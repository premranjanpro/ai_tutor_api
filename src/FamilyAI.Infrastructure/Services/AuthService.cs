using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using FamilyAI.Application.Common.Interfaces;
using FamilyAI.Contracts.Auth;
using FamilyAI.Contracts.Common;
using FamilyAI.Domain.Entities;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(AppDbContext context, IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (existingUser)
        {
            return ApiResponse<AuthResponse>.FailureResponse("User with this email already exists.");
        }

        // Create User
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            MobileNumber = request.MobileNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "FamilyAdmin"
        };

        // Create Default Family
        var family = new Family
        {
            FamilyName = $"{request.FullName}'s Family",
            PrimaryLanguage = request.PreferredLanguage,
            Country = request.Country
        };

        // Add parent member profile to family
        var parentMember = new FamilyMember
        {
            FamilyId = family.Id,
            UserId = user.Id,
            FullName = request.FullName,
            Nickname = request.FullName.Split(' ').FirstOrDefault() ?? request.FullName,
            Relation = "Parent",
            MemberType = Domain.Enums.MemberType.Parent,
            PreferredLanguage = request.PreferredLanguage
        };

        family.Members.Add(parentMember);

        await _context.Users.AddAsync(user);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.FailureResponse("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.FailureResponse("Your account has been blocked by the Administrator.");
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            return ApiResponse<AuthResponse>.FailureResponse("Invalid or expired refresh token.");
        }

        var user = await _context.Users.FindAsync(storedToken.UserId);
        if (user == null)
        {
            return ApiResponse<AuthResponse>.FailureResponse("User not found.");
        }

        // Revoke current token
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var accessToken = _jwtGenerator.GenerateToken(user);
        var newRefreshTokenString = _jwtGenerator.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _context.RefreshTokens.AddAsync(newRefreshToken);
        await _context.SaveChangesAsync();

        var userInfo = new AuthUserInfo(user.Id, user.FullName, user.Role);
        var authResponse = new AuthResponse(accessToken, newRefreshTokenString, 3600, userInfo);

        return ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Token refreshed successfully.");
    }

    private async Task<ApiResponse<AuthResponse>> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _jwtGenerator.GenerateToken(user);
        var refreshTokenString = _jwtGenerator.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        var userInfo = new AuthUserInfo(user.Id, user.FullName, user.Role);
        var response = new AuthResponse(accessToken, refreshTokenString, 3600, userInfo);

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Authentication successful.");
    }
}
