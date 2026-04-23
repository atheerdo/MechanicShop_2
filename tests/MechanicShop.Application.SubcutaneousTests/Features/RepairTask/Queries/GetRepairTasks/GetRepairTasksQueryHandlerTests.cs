using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTasksQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Fetch_All_RepairTasks_Should_Succeed()
    {
        var repairTask01 = RepairTaskFactory.CreateRepairTask().Value;
        var repairTask02 = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddRangeAsync(repairTask01, repairTask02);
        await _context.SaveChangesAsync(default);

        var command = new GetRepairTasksQuery();

        var result = await _mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, r => r.RepairTaskId == repairTask01.Id);
        Assert.Contains(result.Value, r => r.RepairTaskId == repairTask02.Id);
    }
}