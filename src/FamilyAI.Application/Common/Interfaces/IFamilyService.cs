using System;
using System.Threading.Tasks;
using FamilyAI.Contracts.Common;
using FamilyAI.Contracts.Family;

namespace FamilyAI.Application.Common.Interfaces;

public interface IFamilyService
{
    Task<ApiResponse<FamilyResponse>> GetFamilyByUserIdAsync(Guid userId);
    Task<ApiResponse<FamilyMemberResponse>> AddFamilyMemberAsync(Guid userId, AddFamilyMemberRequest request);
    Task<ApiResponse<bool>> UpdateCustomInstructionsAsync(Guid userId, string instructions);
}
