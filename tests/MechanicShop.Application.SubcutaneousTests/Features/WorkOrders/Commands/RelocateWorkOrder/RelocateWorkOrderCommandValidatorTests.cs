using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Domain.WorkOrders.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandValidatorTests
{
    private readonly RelocateWorkOrderCommandValidator _validator;

    public RelocateWorkOrderCommandValidatorTests()
    {
        _validator = new RelocateWorkOrderCommandValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_When_WorkOrderId_Is_Empty()
    {
        var command = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.Empty,
            NewStartAt: DateTimeOffset.UtcNow.AddDays(1),
            NewSpot: Spot.A);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void Validate_Should_Have_Error_When_NewStartAt_Is_In_The_Past()
    {
        var command = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: DateTimeOffset.UtcNow.AddDays(-1),
            NewSpot: Spot.A);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "NewStartAt");
    }

    [Fact]
    public void Validate_Should_Have_Error_When_NewSpot_Is_Invalid()
    {
        var command = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: DateTimeOffset.UtcNow.AddDays(1),
            NewSpot: (Spot)999);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "NewSpot");
    }

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: DateTimeOffset.UtcNow.AddDays(1),
            NewSpot: Spot.B);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}