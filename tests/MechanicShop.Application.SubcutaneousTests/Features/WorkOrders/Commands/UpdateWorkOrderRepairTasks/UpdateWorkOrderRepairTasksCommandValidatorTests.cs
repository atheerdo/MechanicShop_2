using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
    public readonly UpdateWorkOrderRepairTasksCommandValidator _validator;

    public UpdateWorkOrderRepairTasksCommandValidatorTests()
    {
       _validator = new UpdateWorkOrderRepairTasksCommandValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenWorkOrderIdIsEmpty()
    {
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.Empty,
            RepairTaskIds: new[] { Guid.NewGuid() });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRepairTaskIdsIsEmpty()
    {
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTaskIds: Array.Empty<Guid>());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RepairTaskIds");
    }

    [Fact]
    public void Validate_When_ValidCommand_Should_Success()
    {
        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTaskIds: new[] { Guid.NewGuid() });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}