using System;
using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using FamilyAI.Contracts.Common;
using FamilyAI.Contracts.Family;
using Microsoft.AspNetCore.Mvc;

namespace FamilyAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FamilyController : ControllerBase
{
    private readonly IFamilyService _familyService;

    public FamilyController(IFamilyService familyService)
    {
        _familyService = familyService;
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<FamilyResponse>>> GetCurrentFamily()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<FamilyResponse>.FailureResponse("User ID context not found. Provide valid JWT or X-User-Id header."));
        }

        var response = await _familyService.GetFamilyByUserIdAsync(userId);
        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost("members")]
    public async Task<ActionResult<ApiResponse<FamilyMemberResponse>>> AddFamilyMember([FromBody] AddFamilyMemberRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<FamilyMemberResponse>.FailureResponse("User ID context not found. Provide valid JWT or X-User-Id header."));
        }

        var response = await _familyService.AddFamilyMemberAsync(userId, request);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(sub, out var userId))
        {
            return userId;
        }

        if (Request.Headers.TryGetValue("X-User-Id", out var headerId) && Guid.TryParse(headerId, out var headerGuid))
        {
            return headerGuid;
        }

        return Guid.Empty;
    }
}
