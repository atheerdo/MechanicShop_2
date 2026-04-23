using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskPartCommandValidatorTests
{
    private readonly UpdateRepairTaskPartCommandValidator _validator;

    public UpdateRepairTaskPartCommandValidatorTests()
    {
        _validator = new UpdateRepairTaskPartCommandValidator();
    }

    [Fact]
    public void Validate_When_Part_Name_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateRepairTaskPartCommand(
            Guid.NewGuid(),
            " ",
            100m,
            1);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Part name is required.");
    }

    [Fact]
    public void Validate_When_Cost_Is_Greater_Than_10000_Should_Return_Error()
    {
        var command = new UpdateRepairTaskPartCommand(
            Guid.NewGuid(),
            "Brake Pad",
            11000m,
            1);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cost");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Cost must be between 1 and 10,000.");
    }

    [Fact]
    public void Validate_When_Cost_Is_Less_Than_01_Should_Return_Error()
    {
        var command = new UpdateRepairTaskPartCommand(
            Guid.NewGuid(),
            "Brake Pad",
            0,
            1);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cost");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Cost must be between 1 and 10,000.");
    }

    [Fact]
    public void Validate_When_Quantity_Is_Greater_Than_10_Should_Return_Error()
    {
        var command = new UpdateRepairTaskPartCommand(
            Guid.NewGuid(),
            "Brake Pad",
            90m,
            11);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity must be between 1 and 10.");
    }

    [Fact]
    public void Validate_When_Quantity_Is_Less_Than_01_Should_Return_Error()
    {
        var command = new UpdateRepairTaskPartCommand(
            Guid.NewGuid(),
            "Brake Pad",
            90m,
            0);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity must be between 1 and 10.");
    }

    [Fact]
    public void Validate_When_Valid_Data_Should_Succeed()
    {
        var command = new UpdateRepairTaskPartCommand(
            Guid.NewGuid(),
            "Brake Pad",
            90m,
            2);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}