using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class CustomerMapperTest
{
    [Fact]
    public void ToDto_WhenCustomerHasVehicle_ShouldMapAllPropertiesCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();

        var customerDto = customer.ToDto();

        Assert.NotNull(customerDto);
        Assert.Equal(customer.Id, customerDto.CustomerId);
        Assert.Equal(customer.Name, customerDto.Name);
        Assert.Equal(customer.PhoneNumber, customerDto.PhoneNumber);
        Assert.Equal(customer.Email, customerDto.Email);
        Assert.Single(customerDto.Vehicles);

        var vehicleDto = customerDto.Vehicles.First();

        Assert.NotNull(customerDto.Vehicles);
        Assert.Equal(vehicle.Id, vehicleDto.VehicleId);
        Assert.Equal(vehicle.Make, vehicleDto.Make);
        Assert.Equal(vehicle.Model, vehicleDto.Model);
        Assert.Equal(vehicle.Year, vehicleDto.Year);
        Assert.Equal(vehicle.LicensePlate, vehicleDto.LicensePlate);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        // Arrange
       var customer = CustomerFactory.CreateCustomer().Value;

       var customers = new List<Customer> { customer };

       // Act
       var dtos = customers.ToDtos();

       // Assert
       Assert.Single(dtos);
       var dto = dtos[0];
       Assert.NotNull(dto);

       Assert.Equal(customer.Id, dto.CustomerId);
       Assert.Equal(customer.Name, dto.Name);
       Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
       Assert.Equal(customer.Email, dto.Email);
       Assert.Equal(customer.Vehicles.Count(), dto.Vehicles.Count());
    }

    [Fact]
    public void ToDto_WhenVehicleIsFound_ShouldMapAllPropertiesCorrectly()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var vehicleDto = vehicle.ToDto();

        Assert.NotNull(vehicleDto);
        Assert.Equal(vehicle.Id, vehicleDto.VehicleId);
        Assert.Equal(vehicle.Make, vehicleDto.Make);
        Assert.Equal(vehicle.Model, vehicleDto.Model);
        Assert.Equal(vehicle.Year, vehicleDto.Year);
        Assert.Equal(vehicle.LicensePlate, vehicleDto.LicensePlate);
    }

    [Fact]
    public void ToDtos_ShouldMapListOfVehiclesCorrectly()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var vehicles = new List<Vehicle> { vehicle };

        // Act
        var dtos = vehicles.ToDtos();

        // Assert
        Assert.Single(dtos);
        var dto = dtos[0];

        Assert.NotNull(dto);
        Assert.Equal(vehicle.Id, dto.VehicleId);
        Assert.Equal(vehicle.Make, dto.Make);
        Assert.Equal(vehicle.Model, dto.Model);
        Assert.Equal(vehicle.Year, dto.Year);
        Assert.Equal(vehicle.LicensePlate, dto.LicensePlate);
    }
}