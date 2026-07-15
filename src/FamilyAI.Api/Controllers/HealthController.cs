using FamilyAI.Contracts.Common;
using FamilyAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<HealthStatusResponse>>> GetHealth()
    {
        _logger.LogInformation("Performing health check");
        bool dbHealthy = false;

        try
        {
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                dbHealthy = true;
            }
            else
            {
                dbHealthy = await _context.Database.CanConnectAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed during health check");
        }

        var status = new HealthStatusResponse(
            Status: dbHealthy ? "Healthy" : "Degraded",
            DatabaseConnected: dbHealthy
        );

        if (dbHealthy)
        {
            return Ok(ApiResponse<HealthStatusResponse>.SuccessResponse(status, "Service is operational"));
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, 
            ApiResponse<HealthStatusResponse>.FailureResponse(
                new List<string> { "Database connection check failed" }, 
                "Service is degraded"
            ));
    }
}
