using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTask.Commands.RemoveRepairTask;

public class RemoveRepairTaskCommandValidatorTests
{
    private readonly RemoveRepairTaskCommandValidator _validator;

    public RemoveRepairTaskCommandValidatorTests()
    {
        _validator = new RemoveRepairTaskCommandValidator();
    }

    [Fact]
    public void Validate_When_RepairTaskId_Is_Valid_Should_Succeed()
    {
        var command = new RemoveRepairTaskCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_When_RepairTaskId_Is_Empty_Should_Return_Error()
    {
        var command = new RemoveRepairTaskCommand(Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Repair task Id is required.");
        Assert.Contains(result.Errors, e => e.PropertyName == "RepairTaskId");
    }
}