using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.RemoveCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RemoveCustomerCommandHandlerTest(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Customer_Not_Found_Should_Fail()
    {
        var command = new RemoveCustomerCommand(Guid.NewGuid());

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Equal("ApplicationErrors.Customer.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Customer_Has_Associated_WorkOrder_Should_Fail()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id).Value;

        await _context.Customers.AddAsync(customer);
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new RemoveCustomerCommand(customer.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code.StartsWith("Customer"));
    }

    [Fact]
    public async Task Handle_When_Customer_Info_Valid_Should_Success()
    {
         // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new RemoveCustomerCommand(customer.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var deletedCustomer = await _context.Customers.FindAsync(customer.Id);
        Assert.Null(deletedCustomer);
    }
}