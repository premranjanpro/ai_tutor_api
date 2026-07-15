using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyAI.Contracts.Common;
using FamilyAI.Domain.Entities;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public MemoryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<FamilyMemory>>>> GetPendingMemories()
    {
        var memories = await _context.FamilyMemories
            .Where(m => m.Status == "Pending")
            .ToListAsync();

        // Auto-seed mock memories if empty for demo/testing convenience
        if (!memories.Any())
        {
            var family = await _context.Families.FirstOrDefaultAsync();
            var child = await _context.FamilyMembers
                .FirstOrDefaultAsync(m => m.MemberType == Domain.Enums.MemberType.Child);

            if (family != null && child != null)
            {
                var seeded = new List<FamilyMemory>
                {
                    new() {
                        FamilyId = family.Id,
                        MemberId = child.Id,
                        MemoryType = "learning_goal",
                        Content = $"{child.Nickname} wants to learn the multiplication table of 7.",
                        Importance = 0.85,
                        Sensitivity = "low",
                        RequiresParentApproval = true
                    },
                    new() {
                        FamilyId = family.Id,
                        MemberId = child.Id,
                        MemoryType = "interest",
                        Content = $"{child.Nickname} showed high interest in Carnotaurus and Stegosaurus dinosaurs.",
                        Importance = 0.75,
                        Sensitivity = "low",
                        RequiresParentApproval = true
                    },
                    new() {
                        FamilyId = family.Id,
                        MemberId = child.Id,
                        MemoryType = "learning_goal",
                        Content = $"{child.Nickname} has a mathematics class test scheduled for next Monday.",
                        Importance = 0.90,
                        Sensitivity = "medium",
                        RequiresParentApproval = true
                    }
                };

                await _context.FamilyMemories.AddRangeAsync(seeded);
                await _context.SaveChangesAsync();
                memories = seeded;
            }
        }

        return Ok(ApiResponse<List<FamilyMemory>>.SuccessResponse(memories, "Pending memories retrieved successfully."));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveMemory(Guid id)
    {
        var memory = await _context.FamilyMemories.FindAsync(id);
        if (memory == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Memory not found."));
        }

        memory.Status = "Approved";

        // Append to family member profile
        var member = await _context.FamilyMembers.FindAsync(memory.MemberId);
        if (member != null)
        {
            if (memory.MemoryType == "interest")
            {
                var interests = System.Text.Json.JsonSerializer.Deserialize<List<string>>(member.InterestsJson) ?? new List<string>();
                interests.Add(memory.Content);
                member.InterestsJson = System.Text.Json.JsonSerializer.Serialize(interests);
            }
            else if (memory.MemoryType == "learning_goal")
            {
                var goals = System.Text.Json.JsonSerializer.Deserialize<List<string>>(member.LearningGoalsJson) ?? new List<string>();
                goals.Add(memory.Content);
                member.LearningGoalsJson = System.Text.Json.JsonSerializer.Serialize(goals);
            }
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Memory approved and integrated into child's profile."));
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectMemory(Guid id)
    {
        var memory = await _context.FamilyMemories.FindAsync(id);
        if (memory == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Memory not found."));
        }

        memory.Status = "Rejected";
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Memory rejected successfully."));
    }
}
