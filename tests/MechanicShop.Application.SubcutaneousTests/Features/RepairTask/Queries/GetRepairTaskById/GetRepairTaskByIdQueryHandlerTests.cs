using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTaskByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_RepairTask_Does_Not_Found_Should_Fail()
    {
        var command = new GetRepairTaskByIdQuery(Guid.NewGuid());

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "RepairTask.NotFound");
    }

    [Fact]
    public async Task Handle_When_RepairTask_Is_Valid_Should_Succeed()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new GetRepairTaskByIdQuery(repairTask.Id);

        var result = await _mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(repairTask.Id, result.Value.RepairTaskId);
    }
}