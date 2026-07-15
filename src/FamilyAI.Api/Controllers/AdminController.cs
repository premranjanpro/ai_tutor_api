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
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<User>>>> GetUsers()
    {
        var users = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
        return Ok(ApiResponse<List<User>>.SuccessResponse(users, "Users retrieved successfully."));
    }

    [HttpPost("users/{id}/toggle-block")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleBlock(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("User not found."));
        }

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        string action = user.IsActive ? "unblocked" : "blocked";
        return Ok(ApiResponse<bool>.SuccessResponse(user.IsActive, $"User successfully {action}."));
    }
}
