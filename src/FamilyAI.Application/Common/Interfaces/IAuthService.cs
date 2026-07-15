using System.Threading.Tasks;
using FamilyAI.Contracts.Auth;
using FamilyAI.Contracts.Common;

namespace FamilyAI.Application.Common.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
}
