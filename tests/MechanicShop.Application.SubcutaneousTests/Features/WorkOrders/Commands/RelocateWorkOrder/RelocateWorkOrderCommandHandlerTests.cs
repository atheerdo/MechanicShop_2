using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.Identity.Client;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RelocateWorkOrderCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_WithValidData_Should_Succeed()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        var newStartAt = workOrder.StartAtUtc.AddHours(2);
        var newSpot = workOrder.Spot == Spot.A ? Spot.B :
                      workOrder.Spot == Spot.B ? Spot.C :
                      workOrder.Spot == Spot.D ? Spot.A : Spot.D;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new RelocateWorkOrderCommand(
            WorkOrderId: workOrder.Id,
            NewStartAt: newStartAt,
            NewSpot: newSpot);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Updated>(result.Value);

        var updatedWorkOrder = await _context.WorkOrders.FindAsync(workOrder.Id);
        Assert.NotNull(updatedWorkOrder);
        Assert.Equal(newStartAt, updatedWorkOrder!.StartAtUtc);
        Assert.Equal(newSpot, updatedWorkOrder.Spot);
   }

    [Fact]
    public async Task Handle_WithInvalidWorkOrderId_Should_Fail()
    {
        // Arrange
        var command = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: DateTimeOffset.UtcNow.AddHours(1),
            NewSpot: Spot.A);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder_NotFound");
    }

    [Fact]
    public async Task Handle_WithInvalidSpot_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new RelocateWorkOrderCommand(
            WorkOrderId: workOrder.Id,
            NewStartAt: workOrder.StartAtUtc.AddHours(1),
            NewSpot: (Spot)999);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code.Contains("Spot"));
    }

    [Fact]
    public async Task Handle_When_Labor_Is_Occupied_Should_Fail()
    {
        // Arrange
        var workOrder1 = WorkOrderFactory.CreateWorkOrder().Value;

        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            startAt: workOrder1.StartAtUtc.AddMinutes(30),
            laborId: workOrder1.LaborId).Value;

        await _context.WorkOrders.AddRangeAsync(workOrder1, workOrder2);
        await _context.SaveChangesAsync(default);

        var command = new RelocateWorkOrderCommand(
            WorkOrderId: workOrder2.Id,
            NewStartAt: workOrder1.StartAtUtc.AddMinutes(15),
            NewSpot: workOrder2.Spot);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Labor_Occupied");
    }

    [Fact]
    public async Task Handle_When_VehicleAlreadyScheduled_Should_Fail()
    {
        // Arrange
        var workOrder1 = WorkOrderFactory.CreateWorkOrder().Value;

        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: workOrder1.VehicleId,
            startAt: workOrder1.StartAtUtc.AddMinutes(30)).Value;

        await _context.WorkOrders.AddRangeAsync(workOrder1, workOrder2);
        await _context.SaveChangesAsync(default);

        var command = new RelocateWorkOrderCommand(
            WorkOrderId: workOrder2.Id,
            NewStartAt: workOrder1.StartAtUtc.AddMinutes(15),
            NewSpot: workOrder2.Spot);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Overlapping");
    }

    [Fact]
    public async Task Handle_When_Relocate_Makes_StartAt_Equals_EndAt_Should_Fail()
   {
    // Arrange
    var startAt = new DateTimeOffset(2025, 12, 25, 12, 0, 0, TimeSpan.Zero);
    var endAt = startAt.AddHours(1); // valid duration

    var workOrder = WorkOrderFactory.CreateWorkOrder(
        startAt: startAt,
        endAt: endAt).Value;

    await _context.WorkOrders.AddAsync(workOrder);
    await _context.SaveChangesAsync(default);

    var command = new RelocateWorkOrderCommand(
        WorkOrderId: workOrder.Id,
        NewStartAt: endAt,
        NewSpot: workOrder.Spot);

    // Act
    var result = await _mediator.Send(command);

    // Assert
    Assert.True(result.IsError);
    Assert.Single(result.Errors);
    Assert.Equal("InvalidTiming", result.Errors.First().Code);
   }

    [Fact]
    public async Task Handle_When_Update_Spot_And_WorkOrder_Not_Scheduled_Should_Fail()
    {
        // Arrange
        var workOrder1 = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.WorkOrders.AddAsync(workOrder1);
        await _context.SaveChangesAsync(default);
    }
}