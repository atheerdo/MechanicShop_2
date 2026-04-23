using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

public class DeleteWorkOrderCommandValidatorTests
{
    private readonly DeleteWorkOrderCommandValidator _validator;

    public DeleteWorkOrderCommandValidatorTests()
    {
        _validator = new DeleteWorkOrderCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_WorkOrderId_Is_Empty()
    {
        var command = new DeleteWorkOrderCommand(Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkOrderId");
    }
}