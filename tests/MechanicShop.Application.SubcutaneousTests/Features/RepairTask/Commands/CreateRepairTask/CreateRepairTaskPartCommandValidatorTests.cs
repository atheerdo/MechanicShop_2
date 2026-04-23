using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskPartCommandValidatorTests
{
    private readonly CreateRepairTaskPartCommandValidator _validator;

    public CreateRepairTaskPartCommandValidatorTests()
    {
        _validator = new CreateRepairTaskPartCommandValidator();
    }

    [Fact]
    public void Validate_When_Name_Is_Empty_Should_Return_Error()
    {
        var command = new CreateRepairTaskPartCommand(Name: string.Empty, Cost: 100m, Quantity: 2);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Part name is required.");
    }

    [Fact]
    public void Validate_When_Cost_Is_Zero_Or_Less_Should_Return_Error()
    {
        var command = new CreateRepairTaskPartCommand(Name: "Brake Pad", Cost: 0, Quantity: 2);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Part cost must be greater than 0.");
    }

    [Fact]
    public void Validate_When_Quantity_Is_Zero_Or_Less_Should_Return_Error()
    {
        var command = new CreateRepairTaskPartCommand(Name: "Brake Pad", Cost: 100m, Quantity: 0);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity must be at least 1.");
    }

    [Fact]
    public void Validate_When_Data_Is_Valid_Should_Success()
    {
        var command = new CreateRepairTaskPartCommand(Name: "Brake Pad", Cost: 100m, Quantity: 2);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}