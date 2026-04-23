using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.RemoveRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RemoveRepairTaskCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_RepairTask_Not_Found_Should_Fail()
    {
        // Arrange
        var command = new RemoveRepairTaskCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "RepairTask.NotFound");
    }

    [Fact]
    public async Task Handle_When_RepairTask_Is_In_Use_Should_Fail()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        var repairTasks = workOrder.RepairTasks;

        await _context.WorkOrders.AddAsync(workOrder);
        foreach (var repairTask in repairTasks)
        {
            await _context.RepairTasks.AddAsync(repairTask);
        }

        await _context.SaveChangesAsync(default);

        var command = new RemoveRepairTaskCommand(repairTasks.First().Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "RepairTask.InUse");
    }

    [Fact]
    public async Task Handle_When_Happy_Path_Should_Success()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new RemoveRepairTaskCommand(repairTask.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var deletedTask = await _context.RepairTasks
        .FirstOrDefaultAsync(rt => rt.Id == repairTask.Id);

        Assert.Null(deletedTask);
    }
}