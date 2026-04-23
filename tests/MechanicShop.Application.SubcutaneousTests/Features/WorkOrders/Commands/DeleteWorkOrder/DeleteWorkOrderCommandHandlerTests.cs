using Docker.DotNet.Models;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Contracts.Common;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DeleteWorkOrderCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
       var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

       await _context.WorkOrders.AddAsync(workOrder);
       await _context.SaveChangesAsync(default);

       var command = new DeleteWorkOrderCommand(workOrder.Id);

       var result = await _mediator.Send(command);

       Assert.True(result.IsSuccess);
       var deletedWorkOrder = await _context.WorkOrders.FindAsync(workOrder.Id);
       Assert.Null(deletedWorkOrder);
    }

    [Fact]
    public async Task Handle_With_Missing_WorkOrderId_Should_Fail()
    {
        var command = new DeleteWorkOrderCommand(Guid.NewGuid());

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Description.Contains("WorkOrderId is required"));
    }

    [Fact]
    public async Task Handle_When_WorkOrder_State_Is_Scheduled_Should_Success()
    {
         var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

         await _context.WorkOrders.AddAsync(workOrder);
         await _context.SaveChangesAsync(default);

         var command = new DeleteWorkOrderCommand(workOrder.Id);

         var result = await _mediator.Send(command);

         Assert.True(result.IsSuccess);
    }
}