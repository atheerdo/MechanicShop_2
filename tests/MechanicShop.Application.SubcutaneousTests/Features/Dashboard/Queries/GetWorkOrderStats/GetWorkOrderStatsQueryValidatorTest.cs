using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStats;

public class GetWorkOrderStatsQueryValidatorTest
{
    private readonly GetWorkOrderStatsQueryValidator _validator;

    public GetWorkOrderStatsQueryValidatorTest()
    {
        _validator = new GetWorkOrderStatsQueryValidator();
    }

    public class GetWorkOrderStatsQueryValidatorTests
   {
    private readonly GetWorkOrderStatsQueryValidator _validator = new();

    [Fact]
    public void Validate_When_Date_Is_Empty_Should_Return_Error()
    {
        // Arrange
        var query = new GetWorkOrderStatsQuery(default);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "Date_Is_Required");
    }

    [Fact]
    public void Validate_When_Date_Is_Valid_Should_Succeed()
    {
        // Arrange
        var query = new GetWorkOrderStatsQuery(DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
}