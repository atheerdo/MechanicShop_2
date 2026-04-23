using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_RepairTask_Not_Found_Should_Fail()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Brake Inspection",
            90m,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 1)
            });

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "RepairTask.NotFound");
    }

    [Fact]
    public async Task Handle_When_Parts_Not_Valid_Should_Fail()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new UpdateRepairTaskCommand(
            repairTask.Id,
            "Brake Inspection",
            90m,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), " ", 50m, 1)
            });

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Part.Name.Required");
    }

    [Fact]
    public async Task Handle_When_Update_RepairTask_Should_Fail()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new UpdateRepairTaskCommand(
            repairTask.Id,
            "Brake Inspection",
            0,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 1)
            });

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "RepairTask.LaborCost.Invalid");
    }

    [Fact]
    public async Task Handle_When_UpsertParts_Should_Fail()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new UpdateRepairTaskCommand(
            repairTask.Id,
            "Brake Inspection",
            90,
            RepairDurationInMinutes.Min30,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 0)
            });

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Part.Quantity.Invalid");
    }

    [Fact]
    public async Task Handle_When_Data_Is_Valid_Should_Succeed()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new UpdateRepairTaskCommand(
            repairTask.Id,
            "Change Oil", // Update Name
            repairTask.LaborCost,
            repairTask.EstimatedDurationInMins,
            new List<UpdateRepairTaskPartCommand>
            {
                new UpdateRepairTaskPartCommand(Guid.NewGuid(), "Brake Pad", 50m, 1) // Update Parts
            });

        var result = await _mediator.Send(command);

        Assert.True(result.IsSuccess);

        var updatedRepairTask = await _context.RepairTasks.FirstOrDefaultAsync(rt => rt.Id == repairTask.Id);
        Assert.NotNull(updatedRepairTask);
        Assert.Equal("Change Oil", updatedRepairTask!.Name);
    }
}