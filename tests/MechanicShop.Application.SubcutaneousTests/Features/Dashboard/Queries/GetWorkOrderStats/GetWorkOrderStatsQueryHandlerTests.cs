using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStats;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderStatsQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_No_WorkOrders_For_Date_Should_Return_Zero_Stats()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = new GetWorkOrderStatsQuery(date);

        var result = await _mediator.Send(query);

        Assert.True(result.IsSuccess);

        var stats = result.Value;
        Assert.Equal(date, stats.Date);
        Assert.Equal(0, stats.Total);
        Assert.Equal(0, stats.TotalRevenue);
        Assert.Equal(0, stats.UniqueVehicles);
        Assert.Equal(0, stats.UniqueCustomers);
    }

    [Fact]
    public async Task Handle_When_WorkOrders_Have_No_Invoices_Should_Return_Zero_Revenue()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: date.ToDateTime(TimeOnly.MinValue).AddHours(2)).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var result = await _mediator.Send(new GetWorkOrderStatsQuery(date));

        Assert.True(result.IsSuccess);

        var stats = result.Value;
        Assert.Equal(1, stats.Total);
        Assert.Equal(0, stats.TotalRevenue);
        Assert.Equal(1, stats.Scheduled);
    }

    [Fact]
    public async Task Handle_When_WorkOrders_Have_Invoices_Should_Calculate_Revenue_And_Profit()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: date.ToDateTime(TimeOnly.MinValue).AddHours(3)).Value;

        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: workOrder.Id).Value;

        // Attach invoice to work order
        workOrder.Invoice = invoice;

        // Persist both entities
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync(default);

        // Act
        var result = await _mediator.Send(new GetWorkOrderStatsQuery(date));

        // Assert
        Assert.True(result.IsSuccess);

        var stats = result.Value;

        Assert.Equal(invoice.Total, stats.TotalRevenue);
    }

    [Fact]
    public async Task Handle_When_WorkOrders_Have_Mixed_States_Should_Count_Correctly()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var workOrder1 = WorkOrderFactory.CreateWorkOrder().Value;
        var workOrder2 = WorkOrderFactory.CreateWorkOrder().Value;
        workOrder2.UpdateState(WorkOrderState.InProgress);

        var workOrder3 = WorkOrderFactory.CreateWorkOrder().Value;
        workOrder3.UpdateState(WorkOrderState.Completed);

        await _context.WorkOrders.AddRangeAsync(workOrder1, workOrder2, workOrder3);
        await _context.SaveChangesAsync(default);

        // Act
        var result = await _mediator.Send(new GetWorkOrderStatsQuery(date));

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(3, stats.Total);
        Assert.Equal(1, stats.Scheduled);
        Assert.Equal(1, stats.InProgress);
        Assert.Equal(1, stats.Completed);
    }

    [Fact]
    public async Task Handle_Should_Calculate_Unique_Vehicles_And_Customers()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var customer1 = CustomerFactory.CreateCustomer().Value;
        var customer2 = CustomerFactory.CreateCustomer().Value;

        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        vehicle1.Customer = customer1;

        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        vehicle2.Customer = customer2;

        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle1.Id,
            startAt: date.ToDateTime(TimeOnly.MinValue).AddHours(2)).Value;

        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle2.Id,
            startAt: date.ToDateTime(TimeOnly.MinValue).AddHours(3)).Value;

        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.Vehicles.AddRangeAsync(vehicle1, vehicle2);
        await _context.WorkOrders.AddRangeAsync(workOrder1, workOrder2);
        await _context.SaveChangesAsync(default);

        // Act
        var result = await _mediator.Send(new GetWorkOrderStatsQuery(date));

        // Assert
        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(2, stats.UniqueVehicles);
        Assert.Equal(2, stats.UniqueCustomers);
    }

    [Fact]
    public async Task Handle_Should_Ignore_WorkOrders_Outside_Requested_Date()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var todayOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: date.ToDateTime(TimeOnly.MinValue).AddHours(1)).Value;

        var yesterdayOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: date.AddDays(-1).ToDateTime(TimeOnly.MinValue)).Value;

        await _context.WorkOrders.AddRangeAsync(todayOrder, yesterdayOrder);
        await _context.SaveChangesAsync(default);

        var stats = (await _mediator.Send(new GetWorkOrderStatsQuery(date))).Value;

        Assert.Equal(1, stats.Total);
    }
}