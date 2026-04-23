using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersQueryValidatorTests
{
    private readonly GetWorkOrdersQueryValidator _validator;
    public GetWorkOrdersQueryValidatorTests()
    {
        _validator = new GetWorkOrdersQueryValidator();
    }

    [Fact]
    public void Validate_Should_Pass_When_All_Properties_Are_Valid()
    {
        // Arrange
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 20,
            SearchTerm: "oil",
            SortColumn: "createdAt",
            SortDirection: "asc",
            State: WorkOrderState.InProgress,
            VehicleId: Guid.NewGuid(),
            LaborId: Guid.NewGuid(),
            StartDateFrom: DateTime.UtcNow.AddDays(-7),
            StartDateTo: DateTime.UtcNow,
            EndDateFrom: DateTime.UtcNow.AddDays(-5),
            EndDateTo: DateTime.UtcNow.AddDays(1),
            Spot: Spot.A);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Should_Fail_When_Page_Is_Zero()
    {
        var query = new GetWorkOrdersQuery(
            Page: 0,
            PageSize: 10,
            SearchTerm: null);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Page");
    }

    [Fact]
    public void Validate_Should_Fail_When_PageSize_Less_Or_Equal_Zero()
    {
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 0,
            SearchTerm: null);

        var result = _validator.Validate(query);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Validate_Should_Fail_When_PageSize_Exceeds_Max()
    {
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 1000,
            SearchTerm: null);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Validate_When_Invalid_State_Should_Fail()
    {
        var query = new GetWorkOrdersQuery(
            Page: 10,
            PageSize: 100,
            SearchTerm: null,
            State: (WorkOrderState)999);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "State");
    }

    [Fact]
    public void Validate_When_Invalid_Spot_Should_Fail()
    {
        var query = new GetWorkOrdersQuery(
            Page: 10,
            PageSize: 100,
            SearchTerm: null,
            Spot: (Spot)999);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Spot");
    }

    [Fact]
    public void Validate_When_StartDateFrom_Greater_Than_StartDateTo_Should_Fail()
    {
        var query = new GetWorkOrdersQuery(
            Page: 10,
            PageSize: 100,
            SearchTerm: null,
            StartDateFrom: DateTime.Now,
            StartDateTo: DateTime.Now.AddHours(1));

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDateFrom");
    }

    [Fact]
    public void Validate_When_EndDateFrom_Greater_Than_EndDateTo_Should_Fail()
    {
        var query = new GetWorkOrdersQuery(
            Page: 10,
            PageSize: 100,
            SearchTerm: null,
            EndDateFrom: DateTime.Now.AddHours(2),
            EndDateTo: DateTime.Now.AddMinutes(1));

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDateFrom");
    }

    [Fact]
    public void Validate_When_SortDirection_Is_Invalid_Should_Fail()
    {
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            SortDirection: "up");

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SortDirection");
    }

    [Fact]
    public void Validate_When_SortColumn_Is_Invalid_Should_Fail()
    {
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            SortColumn: "invalid_column");

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SortColumn");
    }
}