using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Tests.Common.Customers;

public static class CustomerFactory
{
    public static Result<Customer> CreateCustomer(
        Guid? id = null,
        string? name = null,
        string? phoneNumber = null,
        string? email = null,
        List<Vehicle>? vehicles = null)
    {
        return Customer.Create(
            id ?? Guid.NewGuid(),
            name ?? "Ali",
            phoneNumber ?? "+905533777556",
            email ?? "ali@example.com",
            vehicles ?? [VehicleFactory.CreateVehicle().Value, VehicleFactory.CreateVehicle().Value]);
    }
}