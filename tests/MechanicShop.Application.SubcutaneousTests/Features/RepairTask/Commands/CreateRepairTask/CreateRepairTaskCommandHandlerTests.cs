using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateRepairTaskCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_RepairTask_Is_Exists_Should_Fail()
    {
        // Arrange
        var existRepairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddAsync(existRepairTask);
        await _context.SaveChangesAsync(default);

        var command = new CreateRepairTaskCommand(
            Name: existRepairTask.Name,
            LaborCost: existRepairTask.LaborCost,
            EstimatedDurationInMins: existRepairTask.EstimatedDurationInMins,
            Parts: new List<CreateRepairTaskPartCommand>
                { new CreateRepairTaskPartCommand("Brake buds", 100, 2) });

        // Act
        var result = await _mediator.Send(command);

       // Assert
        Assert.True(result.IsError);
        Assert.Equal("RepairTaskPart.Duplicate", result.TopError.Code);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_When_Invalid_Parts_Should_Fail()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 100,
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            Parts: new List<CreateRepairTaskPartCommand>
                { new CreateRepairTaskPartCommand(" ", 100, 2) });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.NotEmpty(result.Errors);
        Assert.Equal("Part.Name.Required", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Invalid_Creation_RepairTask_Should_Fail()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 0,
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            Parts: new List<CreateRepairTaskPartCommand>
                { new CreateRepairTaskPartCommand("Brake Inspection", 100, 2) });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Code.StartsWith("RepairTask"));
    }

    [Fact]
    public async Task Handle_When_RepairTask_Data_Is_Valid_Should_Success()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            Name: "Brake Inspection",
            LaborCost: 100,
            EstimatedDurationInMins: RepairDurationInMinutes.Min45,
            Parts: new List<CreateRepairTaskPartCommand>
                { new CreateRepairTaskPartCommand("Brake buds", 100, 2) });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var savedTask = await _context.RepairTasks.FirstOrDefaultAsync(rt => rt.Name == "Brake Inspection");
        Assert.NotNull(savedTask);
    }
}