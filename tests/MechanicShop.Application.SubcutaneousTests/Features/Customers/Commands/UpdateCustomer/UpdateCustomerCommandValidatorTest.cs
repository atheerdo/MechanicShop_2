using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidatorTest
{
    private readonly UpdateCustomerCommandValidator _validator;

    public UpdateCustomerCommandValidatorTest()
    {
        _validator = new UpdateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_When_CustomerId_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.Empty,
             Name: "Doe John",
             PhoneNumber: "+00905533888778",
             Email: "email@gmail.com",
             Vehicles: new List<UpdateVehicleCommand>
             {
                 new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123")
             });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_When_Name_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: string.Empty,
             PhoneNumber: "+00905533888778",
             Email: "email@gmail.com",
             Vehicles: new List<UpdateVehicleCommand>
             {
                 new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123")
             });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_When_Email_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: "Doe John",
             PhoneNumber: "+00905533888778",
             Email: string.Empty,
             Vehicles: new List<UpdateVehicleCommand>
             {
                 new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123")
             });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_When_Email_Is_Invalid_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: "Doe John",
             PhoneNumber: "+00905533888778",
             Email: "gmail.com",
             Vehicles: new List<UpdateVehicleCommand>
             {
                 new UpdateVehicleCommand(
                        VehicleId: Guid.NewGuid(),
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123")
             });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_When_PhoneNumber_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: "Doe John",
             PhoneNumber: string.Empty,
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

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void Validate_When_PhoneNumber_Is_Not_Valid_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: "Doe John",
             PhoneNumber: "1233",
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

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void Validate_When_Vehicles_Is_Empty_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: "Doe John",
             PhoneNumber: "+00905533888778",
             Email: "doe@gmail.com",
             Vehicles: []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Vehicles");
    }

    [Fact]
    public void Validate_When_Vehicles_Is_Null_Should_Return_Error()
    {
        var command = new UpdateCustomerCommand(
             CustomerId: Guid.NewGuid(),
             Name: "Doe John",
             PhoneNumber: "+00905533888778",
             Email: "doe@gmail.com",
             Vehicles: null!);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Vehicles");
    }
}