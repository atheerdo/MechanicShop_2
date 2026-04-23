using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateOrderState;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderStateCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_Should_Success_When_State_Is_Valid()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderStateCommand(
            WorkOrderId: workOrder.Id,
            State: WorkOrderState.InProgress);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Updated>(result.Value);

        var updatedWorkOrder = await _context.WorkOrders.FindAsync(workOrder.Id);
        Assert.Equal(WorkOrderState.InProgress, updatedWorkOrder!.State);
    }

    [Fact]
    public async Task Handle_When_WorkOrderStartAt_GreaterThan_UtcNow_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTimeOffset.UtcNow.AddHours(2)).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderStateCommand(
            WorkOrderId: workOrder.Id,
            State: WorkOrderState.InProgress);

         // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("WorkOrderErrors.StateTransitionNotAllowed", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_WorkOrder_DoesNotExist_Should_Fail()
    {
        // Arrange
        var command = new UpdateWorkOrderStateCommand(
            WorkOrderId: Guid.NewGuid(),
            State: WorkOrderState.InProgress);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("ApplicationErrors.WorkOrder.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_UpdateState_Should_Fail_To_Invalid_Transition()
    {
        // Arrange
         var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
         await _context.WorkOrders.AddAsync(workOrder);
         await _context.SaveChangesAsync(default);

         var command = new UpdateWorkOrderStateCommand(
            WorkOrderId: workOrder.Id,
            State: WorkOrderState.Completed);

        // Act
         var result = await _mediator.Send(command);

        // Assert
         Assert.True(result.IsError);
         Assert.Equal(WorkOrderState.Scheduled, workOrder.State);
         Assert.Equal("WorkOrderErrors.InvalidStateTransition", result.TopError.Code);
    }
}