using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_When_Name_Is_Empty_Should_Return_Error()
    {
        var command = new CreateCustomerCommand(
            Name: string.Empty,
            PhoneNumber: "+00905533888778",
            Email: "email@gmail.com",
            Vehicles: new List<CreateVehicleCommand>
            {
                new CreateVehicleCommand(
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
    public void Validate_When_PhoneNumber_Is_Empty_Should_Return_Error()
    {
        var command = new CreateCustomerCommand(
            Name: "Doe John",
            PhoneNumber: string.Empty,
            Email: "email@gmail.com",
            Vehicles: new List<CreateVehicleCommand>
            {
                new CreateVehicleCommand(
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
        var command = new CreateCustomerCommand(
            Name: "Doe John",
            PhoneNumber: "783627",
            Email: "email@gmail.com",
            Vehicles: new List<CreateVehicleCommand>
            {
                new CreateVehicleCommand(
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
    public void Validate_When_Email_Is_Empty_Should_Return_Error()
    {
        var command = new CreateCustomerCommand(
            Name: "Doe John",
            PhoneNumber: "+00905533888778",
            Email: string.Empty,
            Vehicles: new List<CreateVehicleCommand>
            {
                new CreateVehicleCommand(
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
    public void Validate_When_Email_Is_Invalid_Should_Return_Error()
    {
        var command = new CreateCustomerCommand(
            Name: "Doe John",
            PhoneNumber: "+00905533888778",
            Email: "gmail.com",
            Vehicles: new List<CreateVehicleCommand>
            {
                new CreateVehicleCommand(
                        Make: "Toyota",
                        Model: "Corolla",
                        Year: 2020,
                        LicensePlate: "ABC-123")
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_When_Vehicles_Is_Empty_Should_Return_Error()
    {
        var command = new CreateCustomerCommand(
            Name: "Doe John",
            PhoneNumber: "+00905533888778",
            Email: "email@gmail.com",
            Vehicles: []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Vehicles");
    }
}