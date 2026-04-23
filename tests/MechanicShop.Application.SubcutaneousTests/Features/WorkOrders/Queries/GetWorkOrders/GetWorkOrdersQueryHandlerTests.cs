using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrdersQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_No_WorkOrders_Should_Return_Empty_List()
    {
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null);

        var result = await _mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items!);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_Should_Return_Paginated_Result()
    {
        for (int i = 0; i < 15; i++)
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;
            await _context.WorkOrders.AddAsync(wo);
        }

        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 2,
            PageSize: 10,
            SearchTerm: null);

        var result = await _mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(15, result.Value.TotalCount);
        Assert.Equal(5, result.Value.Items!.Count);
    }

    [Fact]
    public async Task Handle_When_Filter_By_State_Should_Return_Matching_WorkOrders()
    {
        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            State: WorkOrderState.InProgress);

        var result = await _mediator.Send(query);

        Assert.Single(result.Value.Items!);
        Assert.Equal(WorkOrderState.InProgress, result.Value.Items!.First().State);
    }

    [Fact]
    public async Task Handle_When_Filter_By_Spot_Should_Return_Matching_WorkOrders()
    {
        var woA = WorkOrderFactory.CreateWorkOrder(spot: Spot.A).Value;
        var woB = WorkOrderFactory.CreateWorkOrder(spot: Spot.B).Value;

        await _context.WorkOrders.AddRangeAsync(woA, woB);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            Spot: Spot.B);

        var result = await _mediator.Send(query);

        Assert.Single(result.Value.Items!);
        Assert.Equal(Spot.B, result.Value.Items!.First().Spot);
    }

    [Fact]
    public async Task Handle_When_Filter_By_VehicleId_Should_Return_Matching_WorkOrders()
    {
        var wo1 = WorkOrderFactory.CreateWorkOrder().Value;
        var wo2 = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.WorkOrders.AddRangeAsync(wo1, wo2);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            VehicleId: wo1.VehicleId);

        var result = await _mediator.Send(query);

        Assert.Single(result.Value.Items!);
        Assert.Equal(wo1.Id, result.Value.Items!.First().WorkOrderId);
    }

    [Fact]
    public async Task Handle_When_SearchTerm_Matches_Should_Return_Results()
    {
        var wo = WorkOrderFactory.CreateWorkOrder().Value;
        wo.AddRepairTask(RepairTaskFactory.CreateRepairTask(name: "Oil Change").Value);

        await _context.WorkOrders.AddAsync(wo);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "oil");

        var result = await _mediator.Send(query);

        Assert.Single(result.Value.Items!);
    }

    [Fact]
    public async Task Handle_When_Sort_Ascending_Should_Return_Correct_Order()
    {
        var older = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTime.UtcNow.AddHours(-2)).Value;

        var newer = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTime.UtcNow).Value;

        await _context.WorkOrders.AddRangeAsync(older, newer);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            SortColumn: "startAtUtc",
            SortDirection: "asc");

        var result = await _mediator.Send(query);

        Assert.Equal(older.Id, result.Value.Items!.First().WorkOrderId);
    }

    [Fact]
    public async Task Handle_When_Sort_Descending_Should_Return_Correct_Order()
    {
        var older = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTime.UtcNow.AddHours(-2)).Value;

        var newer = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTime.UtcNow).Value;

        await _context.WorkOrders.AddRangeAsync(older, newer);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            SortColumn: "startAtUtc",
            SortDirection: "desc");

        var result = await _mediator.Send(query);

        Assert.Equal(newer.Id, result.Value.Items!.First().WorkOrderId);
    }

    [Fact]
    public async Task Handle_When_Filter_By_StartDate_Range_Should_Work()
    {
        var inside = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTime.UtcNow.AddHours(1)).Value;

        var outside = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTime.UtcNow.AddDays(-1)).Value;

        await _context.WorkOrders.AddRangeAsync(inside, outside);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            StartDateFrom: DateTime.UtcNow,
            StartDateTo: DateTime.UtcNow.AddHours(2));

        var result = await _mediator.Send(query);

        Assert.Single(result.Value.Items!);
        Assert.Equal(inside.Id, result.Value.Items!.First().WorkOrderId);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        // Arrange
        var startAt = DateTime.UtcNow.AddHours(1);
        var endAt = startAt.AddHours(2);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: startAt,
            endAt: endAt).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: null,
            SortColumn: "createdAt",
            SortDirection: "asc",
            State: workOrder.State,
            VehicleId: workOrder.VehicleId,
            LaborId: workOrder.LaborId,
            StartDateFrom: startAt.AddMinutes(-5),
            StartDateTo: startAt.AddMinutes(5),
            EndDateFrom: endAt.AddMinutes(-5),
            EndDateTo: endAt.AddMinutes(5),
            Spot: Spot.B);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Single(result.Value.Items!);
        Assert.Equal(workOrder.Id, result.Value.Items!.First().WorkOrderId);
    }
}