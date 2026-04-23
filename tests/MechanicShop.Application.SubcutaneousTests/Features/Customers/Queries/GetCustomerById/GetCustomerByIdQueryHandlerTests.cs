using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomerByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();

    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Customer_Not_Found_Should_Fail()
    {
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        var result = await _mediator.Send(query);

        Assert.True(result.IsError);
        Assert.Equal("Customer_NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Customer_Data_Is_Valid_Should_Success()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        var vehicle = customer.Vehicles.First();
        await _context.Vehicles.AddAsync(vehicle);
        await _context.SaveChangesAsync(default);

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var query = new GetCustomerByIdQuery(customer.Id);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(customer.Id, result.Value.CustomerId);
        Assert.Equal(customer.Name, result.Value.Name);
        Assert.Equal(customer.Email, result.Value.Email);
    }
}