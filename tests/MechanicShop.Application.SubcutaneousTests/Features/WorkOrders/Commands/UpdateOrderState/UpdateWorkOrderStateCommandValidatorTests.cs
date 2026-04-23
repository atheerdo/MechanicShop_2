using Docker.DotNet.Models;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Domain.WorkOrders.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateOrderState;

public class UpdateWorkOrderStateCommandValidatorTests
{
    public readonly UpdateWorkOrderStateCommandValidator _validator;

    public UpdateWorkOrderStateCommandValidatorTests()
    {
        _validator = new UpdateWorkOrderStateCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_State_Is_Invalid()
    {
        var command = new UpdateWorkOrderStateCommand(
            WorkOrderId: Guid.NewGuid(),
            State: (WorkOrderState)999);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "State");
        Assert.Contains(result.Errors, e => e.ErrorCode.Contains("Invalid"));
    }

    [Fact]
    public void Should_Success_When_State_Is_Valid()
    {
        var command = new UpdateWorkOrderStateCommand(
            WorkOrderId: Guid.NewGuid(),
            State: WorkOrderState.InProgress);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}