using System.Net;
using System.Net.Http.Json;
using FamilyAI.Contracts.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FamilyAI.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Set environment variable to switch DbProvider to InMemory before application startup
        Environment.SetEnvironmentVariable("DbProvider", "InMemory");
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ShouldReturnSuccessResponse()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<HealthStatusResponse>>();
        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotNull(content.Data);
        Assert.Equal("Healthy", content.Data.Status);
        Assert.True(content.Data.DatabaseConnected);
        Assert.Equal("Service is operational", content.Message);
        Assert.Null(content.Errors);
    }
}
