using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskCommandValidatorTests
{
    private readonly CreateRepairTaskCommandValidator _validator;

    public CreateRepairTaskCommandValidatorTests()
    {
        _validator = new CreateRepairTaskCommandValidator();
    }

    [Fact]
    public void Validate_When_Name_Is_Empty_Should_Return_Error()
    {
        var command = new CreateRepairTaskCommand(
            Name: string.Empty,
            LaborCost: 20m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: new List<CreateRepairTaskPartCommand>
                 { new CreateRepairTaskPartCommand("Brake buds", 100, 2) });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required.");
    }

    [Fact]
    public void Validate_When_Labor_Cost_Less_Or_Equal_Zero_Should_Return_Error()
    {
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 0,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: new List<CreateRepairTaskPartCommand>
                 { new CreateRepairTaskPartCommand("Brake buds", 100, 2) });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Labor cost must be greater than 0.");
    }

    [Fact]
    public void Validate_When_EstimatedDurationInMins_Is_Not_In_Enum_Should_Return_Error()
    {
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 30m,
            EstimatedDurationInMins: (RepairDurationInMinutes)999,
            Parts: new List<CreateRepairTaskPartCommand>
                 { new CreateRepairTaskPartCommand("Brake buds", 100, 2) });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Estimated duration is required.");
    }

    [Fact]
    public void Validate_When_EstimatedDurationInMins_Is_Null_Should_Return_Error()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 30m,
            EstimatedDurationInMins: null, // explicitly null
            Parts: new List<CreateRepairTaskPartCommand>
                { new CreateRepairTaskPartCommand("Brake buds", 100, 2) });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EstimatedDurationInMins");
    }

    [Fact]
    public void Validate_When_Parts_Is_Null_Should_Return_Error()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 30m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            Parts: null!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Parts list cannot be null.");
    }

    [Fact]
    public void Validate_When_Parts_Count_Zero_Or_Less_Should_Return_Error()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 30m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            Parts: []);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one part is required.");
    }

    [Fact]
    public void Validate_When_RepairTask_Data_Is_Valid_Should_Success()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 30m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            Parts: new List<CreateRepairTaskPartCommand>
                { new CreateRepairTaskPartCommand("Brake buds", 100, 2) });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}