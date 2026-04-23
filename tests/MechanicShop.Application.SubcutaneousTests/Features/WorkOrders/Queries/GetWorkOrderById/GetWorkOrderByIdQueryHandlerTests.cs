using Docker.DotNet.Models;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_WorkOrder_Not_Found_Should_Fail()
    {
        var command = new GetWorkOrderByIdQuery(Guid.NewGuid());

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Equal("ApplicationErrors.WorkOrder.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_Should_Success_When_Valid()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new GetWorkOrderByIdQuery(workOrder.Id);

        var result = await _mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
    }
}