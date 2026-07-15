using FamilyAI.Contracts.Common;
using Xunit;

namespace FamilyAI.UnitTests;

public class HealthResponseTests
{
    [Fact]
    public void ApiResponse_SuccessResponse_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var healthStatus = new HealthStatusResponse("Healthy", true);

        // Act
        var response = ApiResponse<HealthStatusResponse>.SuccessResponse(healthStatus, "System is up");

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Healthy", response.Data.Status);
        Assert.True(response.Data.DatabaseConnected);
        Assert.Equal("System is up", response.Message);
        Assert.Null(response.Errors);
    }

    [Fact]
    public void ApiResponse_FailureResponse_ShouldSetPropertiesCorrectly()
    {
        // Act
        var response = ApiResponse<object>.FailureResponse("Connection error", "Failed operation");

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Failed operation", response.Message);
        Assert.NotNull(response.Errors);
        var error = Assert.Single(response.Errors);
        Assert.Equal("Connection error", error);
    }
}
