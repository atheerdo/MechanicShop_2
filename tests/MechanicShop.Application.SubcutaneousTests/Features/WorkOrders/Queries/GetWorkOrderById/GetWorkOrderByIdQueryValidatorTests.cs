using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

public class GetWorkOrderByIdQueryValidatorTests
{
    private readonly GetWorkOrderByIdQueryValidator _validator;

    public GetWorkOrderByIdQueryValidatorTests()
    {
        _validator = new GetWorkOrderByIdQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_WorkOrder_Is_Empty()
    {
        var query = new GetWorkOrderByIdQuery(WorkOrderId: Guid.Empty);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void Should_Pass_When_WorkOrderId_Is_Valid()
    {
        var query = new GetWorkOrderByIdQuery(WorkOrderId: Guid.NewGuid());

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }
}