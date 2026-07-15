using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using FamilyAI.Contracts.Common;
using FamilyAI.Contracts.Family;
using FamilyAI.Domain.Entities;
using FamilyAI.Domain.Enums;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Infrastructure.Services;

public class FamilyService : IFamilyService
{
    private readonly AppDbContext _context;

    public FamilyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<FamilyResponse>> GetFamilyByUserIdAsync(Guid userId)
    {
        var adminMember = await _context.FamilyMembers
            .Include(m => m.Family)
            .ThenInclude(f => f!.Members)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (adminMember == null || adminMember.Family == null)
        {
            return ApiResponse<FamilyResponse>.FailureResponse("Family not found for this user.");
        }

        var family = adminMember.Family;
        var membersList = family.Members.Select(MapMemberToResponse).ToList();

        var response = new FamilyResponse(
            family.Id,
            family.FamilyName,
            family.PrimaryLanguage,
            family.SecondaryLanguage,
            family.Country,
            family.TimeZone,
            membersList
        );

        return ApiResponse<FamilyResponse>.SuccessResponse(response, "Family retrieved successfully.");
    }

    public async Task<ApiResponse<FamilyMemberResponse>> AddFamilyMemberAsync(Guid userId, AddFamilyMemberRequest request)
    {
        var adminMember = await _context.FamilyMembers
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (adminMember == null || adminMember.Family == null)
        {
            return ApiResponse<FamilyMemberResponse>.FailureResponse("Family context not found.");
        }

        if (!Enum.TryParse<MemberType>(request.MemberType, true, out var parsedMemberType))
        {
            return ApiResponse<FamilyMemberResponse>.FailureResponse($"Invalid member type: {request.MemberType}");
        }

        var member = new FamilyMember
        {
            FamilyId = adminMember.Family.Id,
            FullName = request.FullName,
            Nickname = request.Nickname,
            Relation = request.Relation,
            MemberType = parsedMemberType,
            DateOfBirth = request.DateOfBirth,
            PreferredLanguage = request.PreferredLanguage,
            ClassName = request.ClassName,
            SchoolBoard = request.SchoolBoard,
            InterestsJson = JsonSerializer.Serialize(request.Interests ?? new List<string>()),
            LearningGoalsJson = JsonSerializer.Serialize(request.LearningGoals ?? new List<string>())
        };

        await _context.FamilyMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        return ApiResponse<FamilyMemberResponse>.SuccessResponse(MapMemberToResponse(member), "Family member added successfully.");
    }

    private static FamilyMemberResponse MapMemberToResponse(FamilyMember member)
    {
        var interests = new List<string>();
        try
        {
            interests = JsonSerializer.Deserialize<List<string>>(member.InterestsJson) ?? new List<string>();
        }
        catch { }

        var learningGoals = new List<string>();
        try
        {
            learningGoals = JsonSerializer.Deserialize<List<string>>(member.LearningGoalsJson) ?? new List<string>();
        }
        catch { }

        return new FamilyMemberResponse(
            member.Id,
            member.FamilyId,
            member.FullName,
            member.Nickname,
            member.Relation,
            member.MemberType.ToString(),
            member.DateOfBirth,
            member.Age,
            member.PreferredLanguage,
            member.ClassName,
            member.SchoolBoard,
            interests,
            learningGoals
        );
    }

    public async Task<ApiResponse<bool>> UpdateCustomInstructionsAsync(Guid userId, string instructions)
    {
        var adminMember = await _context.FamilyMembers
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (adminMember == null || adminMember.Family == null)
        {
            return ApiResponse<bool>.FailureResponse("Family context not found.");
        }

        adminMember.Family.ParentCustomInstructions = instructions;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "AI training guidelines updated successfully.");
    }
}
