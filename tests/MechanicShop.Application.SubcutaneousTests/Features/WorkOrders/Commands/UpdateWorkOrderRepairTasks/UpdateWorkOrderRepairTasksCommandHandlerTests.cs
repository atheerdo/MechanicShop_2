using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderRepairTasksCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_WorkOrder_DoesNotExist_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTaskIds: workOrder.RepairTasks.Select(rt => rt.Id).ToArray());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("ApplicationErrors.WorkOrder.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_RepairTaskIds_Is_Empty_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTaskIds: Array.Empty<Guid>());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("RepairTask.Required", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_RequestedTasks_DoNot_Exist_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTaskIds: new[] { Guid.NewGuid(), Guid.NewGuid() });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("ApplicationErrors.RepairTask.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_RequestedTasks_DoesNot_Equal_CommandTasks_Should_Fail()
    {
       // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTaskIds: new[] { repairTask.Id, Guid.NewGuid() });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("ApplicationErrors.RepairTask.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Clear_Repair_Tasks_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            repairTasks: new List<Domain.RepairTasks.RepairTask>()).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTaskIds: new[] { Guid.NewGuid() });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_When_AddRepairTask_Fails_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        // Force invalid add scenario (duplicate, invalid state, etc.)
        workOrder.AddRepairTask(repairTask);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTaskIds: new[] { repairTask.Id });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_When_WorkOrder_Is_Outside_Operating_Hours_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTimeOffset.UtcNow.AddHours(23)).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min180).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: workOrder.Id,
            RepairTaskIds: new[] { repairTask.Id });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("WorkOrder_Outside_OperatingHours", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Spot_Is_Not_Available_Should_Fail()
    {
    // Arrange
    var startAt = DateTimeOffset.UtcNow.AddHours(1);

    var workOrder1 = WorkOrderFactory.CreateWorkOrder(
        startAt: startAt,
        endAt: startAt.AddHours(1),
        spot: Spot.A).Value;

    var workOrder2 = WorkOrderFactory.CreateWorkOrder(
        startAt: startAt.AddMinutes(15),  // overlap
        endAt: startAt.AddHours(2),
        spot: Spot.A).Value;

    var repairTask = RepairTaskFactory.CreateRepairTask(
        repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

    await _context.WorkOrders.AddRangeAsync(workOrder1, workOrder2);
    await _context.RepairTasks.AddAsync(repairTask);
    await _context.SaveChangesAsync(default);

    var command = new UpdateWorkOrderRepairTasksCommand(
        WorkOrderId: workOrder1.Id,
        RepairTaskIds: new[] { repairTask.Id });

    // Act
    var result = await _mediator.Send(command);

    // Assert
    Assert.True(result.IsError);
    Assert.Equal("WorkOrder_SpotNotAvailable", result.TopError.Code);
}

    [Fact]
    public async Task Handle_When_Labor_Is_Occupied_Should_Fail()
    {
    // Arrange
    var startAt = DateTimeOffset.UtcNow.AddHours(1);

    var workOrder = WorkOrderFactory.CreateWorkOrder(
        startAt: startAt,
        endAt: startAt.AddHours(1)).Value;

    // overlapping work order with SAME labor
    var overlappingWorkOrder = WorkOrderFactory.CreateWorkOrder(
        startAt: startAt.AddMinutes(30),   // overlaps
        endAt: startAt.AddHours(2),
        laborId: workOrder.LaborId).Value;

    var repairTask = RepairTaskFactory.CreateRepairTask(
        repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

    await _context.WorkOrders.AddRangeAsync(workOrder, overlappingWorkOrder);
    await _context.RepairTasks.AddAsync(repairTask);
    await _context.SaveChangesAsync(default);

    var command = new UpdateWorkOrderRepairTasksCommand(
        WorkOrderId: workOrder.Id,
        RepairTaskIds: new[] { repairTask.Id });

    // Act
    var result = await _mediator.Send(command);

    // Assert
    Assert.True(result.IsError);
    Assert.Equal("ApplicationErrors.LaborOccupied", result.TopError.Code);
}

    [Fact]
    public async Task Handle_When_All_Conditions_Are_Met_Should_Succeed()
    {
    // Arrange
    var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
    var repairTask = RepairTaskFactory.CreateRepairTask(
        repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;

    await _context.WorkOrders.AddAsync(workOrder);
    await _context.RepairTasks.AddAsync(repairTask);
    await _context.SaveChangesAsync(default);

    var command = new UpdateWorkOrderRepairTasksCommand(
        WorkOrderId: workOrder.Id,
        RepairTaskIds: new[] { repairTask.Id });

    // Act
    var result = await _mediator.Send(command);

    // Assert
    Assert.True(result.IsSuccess);

    var updated = await _context.WorkOrders
        .Include(w => w.RepairTasks)
        .FirstAsync(w => w.Id == workOrder.Id);

    Assert.Single(updated.RepairTasks);
}
}