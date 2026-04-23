using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateCustomerCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Customer_Already_Exist_Should_Fail()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new CreateCustomerCommand(
            Name: customer.Name!,
            PhoneNumber: customer.PhoneNumber!,
            Email: customer.Email!,
            Vehicles: customer.Vehicles.Select(v => new CreateVehicleCommand(
                v.Make,
                v.Model,
                v.Year,
                v.LicensePlate)).ToList());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_When_Customer_Not_Exist_Should_Created_Successfully()
    {
        // Arrange

        var command = new CreateCustomerCommand(
            Name: "Doe John",
            PhoneNumber: "+00905533888778",
            Email: "doe@email.com",
            Vehicles: new List<CreateVehicleCommand>
    {
        new CreateVehicleCommand(
            Make: "German",
            Model: "BMW",
            Year: 2024,
            LicensePlate: "A543663")
    });

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var customer = await _context.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == result.Value.CustomerId);

        Assert.NotEmpty(customer!.Vehicles);
        Assert.Single(customer.Vehicles);
        Assert.Equal("doe@email.com", customer.Email);
    }
}