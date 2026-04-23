using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer.UpdateCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateCustomerCommandHandlerTest(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Customer_Not_Found_Should_Fail()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: "Doe John",
             PhoneNumber: "+905533888778",
             Email: "doe@gmail.com",
             Vehicles: new List<UpdateVehicleCommand>
             {
                 new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123")
             });

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Equal("ApplicationErrors.Customer.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Vehicle_Creation_Fails_Should_Fail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new UpdateCustomerCommand(
             CustomerId: customer.Id,
             Name: customer.Name!,
             PhoneNumber: customer.PhoneNumber!,
             Email: customer.Email!,
             Vehicles: new List<UpdateVehicleCommand>
             {
                 new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: string.Empty,
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123")
             });

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code.StartsWith("Vehicle"));
    }

    [Fact]
    public async Task Handle_When_Fail_To_Update_Customer_Should_Fail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new UpdateCustomerCommand(
             CustomerId: customer.Id,
             Name: string.Empty,
             PhoneNumber: customer.PhoneNumber!,
             Email: customer.Email!,
             Vehicles: customer.Vehicles.Select(v =>
                new UpdateVehicleCommand(
                    v.Id,
                    v.Make,
                    v.Model,
                    v.Year,
                    v.LicensePlate)).ToList());
        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Equal("Customer_Name_Required", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Vehicle_Upsert_Fails_Should_Return_Error()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new UpdateCustomerCommand(
             CustomerId: customer.Id,
             Name: customer.Name!,
             PhoneNumber: customer.PhoneNumber!,
             Email: customer.Email!,
             Vehicles: new List<UpdateVehicleCommand>
             {
                new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123"),
                new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Honda",
                        Model: "Civic",
                        Year: 2021,
                        LicensePlate: " " )
             });

        var result = await _mediator.Send(command);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code.StartsWith("Vehicle"));
    }

    [Fact]
    public async Task Handle_When_Command_Is_Valid_Should_Return_Updated()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new UpdateCustomerCommand(
             CustomerId: customer.Id,
             Name: customer.Name!,
             PhoneNumber: customer.PhoneNumber!,
             Email: "ali@gmail.com", // update email
             Vehicles: new List<UpdateVehicleCommand>
             {
                new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123"),
                new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Honda",
                        Model: "Civic",
                        Year: 2021,
                        LicensePlate: "ABC-567")
             });

        var result = await _mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.IsType<Updated>(result.Value);

        var updatedCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.NotNull(updatedCustomer);
        Assert.Equal("ali@gmail.com", updatedCustomer.Email);
    }
}