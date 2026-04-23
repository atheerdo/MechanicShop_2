using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Domain.RepairTasks.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandValidatorTests
{
    private readonly UpdateRepairTaskCommandValidator _validator;

    public UpdateRepairTaskCommandValidatorTests()
    {
        _validator = new UpdateRepairTaskCommandValidator();
    }

    [Fact]
    public void Validate_When_RepairTaskId_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.Empty,
            Name: "Brake Inspection",
            LaborCost: 90m,
            EstimatedDurationInMinutes: RepairDurationInMinutes.Min30,
            Parts: new List<UpdateRepairTaskPartCommand>
            {
                 new UpdateRepairTaskPartCommand(
                            PartId: Guid.NewGuid(),
                            Name: "Brake Pad",
                            Cost: 50m,
                            Quantity: 1)
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RepairTaskId");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Repair task ID is required.");
    }

    [Fact]
    public void Validate_When_Name_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: " ",
            LaborCost: 90m,
            EstimatedDurationInMinutes: RepairDurationInMinutes.Min30,
            Parts: new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(
                        PartId: Guid.NewGuid(),
                        Name: "Brake Pad",
                        Cost: 50m,
                        Quantity: 1)
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Task name is required.");
    }

    [Fact]
    public void Validate_When_Name_Length_Is_Grater_Than_100_Character_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            new string('x', 101), // grater than 100
            90m,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(
                        Guid.NewGuid(),
                        "Brake Pad",
                        50m,
                        1)
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Task name must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_When_LaborCost_Is_Greater_Than_10000_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            11000,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 1)
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LaborCost");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Labor cost must be between 1 and 10,000.");
    }

    [Fact]
    public void Validate_When_LaborCost_Is_Less_Than_1_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            0,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 1)
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LaborCost");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Labor cost must be between 1 and 10,000.");
    }

    [Fact]
    public void Validate_When_EstimatedDurationInMins_Is_Invalid_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            90m,
            (RepairDurationInMinutes)999,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 1)
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EstimatedDurationInMins");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Invalid duration selected.");
    }

    [Fact]
    public void Validate_When_Parts_Are_Null_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            90m,
            (RepairDurationInMinutes)999,
            null!);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Parts");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Parts must not be null.");
    }

    [Fact]
    public void Validate_When_Parts_Count_Are_Zero_Should_Return_Error()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            90m,
            (RepairDurationInMinutes)999,
            []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Parts");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one part is required.");
    }

    [Fact]
    public void Validate_When_Parts_Are_Valid_Should_Succeed()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            90m,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 1)
            });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_When_Part_Is_Invalid_Should_Return_Error()
    {
        // Arrange: create a command with one invalid part (empty name)
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            90m,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(
                    Guid.NewGuid(),
                    " ", // invalid
                    50m,
                    1)
            });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Part name is required.");
    }
}