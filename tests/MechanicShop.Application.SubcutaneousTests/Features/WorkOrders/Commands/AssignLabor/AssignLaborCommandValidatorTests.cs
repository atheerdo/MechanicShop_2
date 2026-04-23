using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.AssignLabor;

public class AssignLaborCommandValidatorTests
{
    private readonly AssignLaborCommandValidator _validator;

    public AssignLaborCommandValidatorTests()
    {
        _validator = new AssignLaborCommandValidator();
    }

    [Fact]
    public void Validate_Should_Have_Error_When_WorOrderId_Is_Empty()
    {
        var command = new AssignLaborCommand(
            WorkOrderId: Guid.Empty,
            LaborId: Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void Validate_Should_Have_Error_When_LaborId_Is_Empty()
    {
        var command = new AssignLaborCommand(
            WorkOrderId: Guid.NewGuid(),
            LaborId: Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LaborId");
    }

    [Fact]
    public void Validate_Should_Pass_When_Ids_Are_Valid()
    {
        var command = new AssignLaborCommand(
            WorkOrderId: Guid.NewGuid(),
            LaborId: Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}