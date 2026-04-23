using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Events;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.EventHandlers;

public class SendWorkOrderCompletedEmailHandlerTests
{
    private readonly INotificationService _notificationService;
    private readonly IAppDbContext _context;
    private readonly ILogger<SendWorkOrderCompletedEmailHandler> _logger;

    private readonly SendWorkOrderCompletedEmailHandler _handler;

    public SendWorkOrderCompletedEmailHandlerTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        _context = Substitute.For<IAppDbContext>();
        _logger = Substitute.For<ILogger<SendWorkOrderCompletedEmailHandler>>();

        _handler = new SendWorkOrderCompletedEmailHandler(
            _notificationService,
            _logger,
            _context);
    }

    [Fact]
    public async Task Handle_When_WorkOrder_Not_Found_Should_Log_Error_And_Return()
    {
        // Arrange
        var notification = new WorkOrderCompleted();

        _context.WorkOrders.Returns(new List<WorkOrder>().AsQueryable());

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("does not exist")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        await _notificationService.DidNotReceive()
            .SendEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        await _notificationService.DidNotReceive()
            .SendSmsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_When_WorkOrder_Exists_Should_Send_Sms()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: new List<Vehicle> { vehicle }).Value;
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id).Value;

        workOrder.Vehicle = vehicle;

        _context.WorkOrders.Returns(new List<WorkOrder> { workOrder }.AsQueryable());

        var notification = new WorkOrderCompleted();

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        await _notificationService.Received(1)
            .SendSmsAsync(customer.PhoneNumber!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_When_WorkOrder_Exists_Should_Not_Log_Error()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: new List<Vehicle> { vehicle }).Value;
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id).Value;

        workOrder.Vehicle = vehicle;

        _context.WorkOrders.Returns(new List<WorkOrder> { workOrder }.AsQueryable());

        var notification = new WorkOrderCompleted();

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _logger.DidNotReceiveWithAnyArgs()
            .Log(LogLevel.Error, default, default!, default!, default!);
    }
}
