using System;
using FamilyAI.Domain.Entities;

namespace FamilyAI.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
