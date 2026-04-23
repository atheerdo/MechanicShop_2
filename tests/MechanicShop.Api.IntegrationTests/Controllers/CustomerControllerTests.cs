using System.Net;
using System.Text;
using System.Text.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CustomerControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetCustomers_WithValidData_ShouldReturnAllCustomers()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resultJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<CustomerDto>>(resultJson, _jsonOptions);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        response.Dispose();
    }

    [Fact]
    public async Task GetCustomers_WithAuthorizationAsLabor_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        using var response = await _client.GetAsync("/api/v1.0/customers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WithValidData_ShouldReturnSuccess()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        try
        {
            using var response = await _client.GetAsync($"/api/v1.0/customers/{customer.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resultJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CustomerDto>(resultJson, _jsonOptions);
            Assert.NotNull(result);
            Assert.Equal(customer.Id, result.CustomerId);
            Assert.Equal(customer.Name, result.Name);
        }
        finally
        {
            await _context.Customers
                .Where(c => c.Id == customer.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetCustomerById_WithInvalidCustomerId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync($"/api/v1.0/customers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnSuccess()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = CustomerFactory.CreateCustomer().Value;

        var request = new CreateCustomerRequest
        {
            Name = customer.Name!,
            PhoneNumber = customer.PhoneNumber!,
            Email = customer.Email!,
            Vehicles = customer.Vehicles.Select(v => new CreateVehicleRequest
            {
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate
            }).ToList()
        };

        CustomerDto? dto = null;

        try
        {
            using var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var resultJson = await response.Content.ReadAsStringAsync();
            dto = JsonSerializer.Deserialize<CustomerDto>(resultJson, _jsonOptions);
            Assert.NotNull(dto);
            Assert.Equal(customer.Name, dto.Name);
            Assert.Equal(customer.Email, dto.Email);
        }
        finally
        {
            if (dto is not null)
            {
                await _context.Customers
                    .Where(c => c.Id == dto.CustomerId)
                    .ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task CreateCustomer_WithInValidData_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = CustomerFactory.CreateCustomer().Value;

        var request = new CreateCustomerRequest
        {
            Name = customer.Name!,
            PhoneNumber = customer.PhoneNumber!,
            Email = customer.Email!,
            Vehicles = []
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var response = await _client.PostAsJsonAsync("/api/v1.0/customers", requestContent);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldReturnSuccess()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var existingCustomer = await _context.Customers
            .Include(c => c.Vehicles)
            .FirstAsync(c => c.Id == customer.Id);

        Console.WriteLine($"Existing Customer: {existingCustomer.Name}, Vehicles Count: {existingCustomer.Vehicles.Count()}");

        var request = new UpdateCustomerRequest
        {
            Name = "Updated Name",
            PhoneNumber = "+90553388996",
            Email = "sh@example.com",
            Vehicles = customer.Vehicles.Select(v => new UpdateVehicleRequest
            {
                VehicleId = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate
            }).ToList()
        };

        try
        {
            using var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{customer.Id}", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resultJson = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<CustomerDto>(resultJson, _jsonOptions);
            Assert.NotNull(dto);
            Assert.Equal(request.Name, dto.Name);

            var updated = await _context.Customers
                .AsNoTracking()
                .FirstAsync(c => c.Id == customer.Id);

            Assert.Equal(request.Name, updated.Name);
        }
        finally
        {
            await _context.Customers
                .Where(c => c.Id == customer.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidData_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var customer = CustomerFactory.CreateCustomer().Value;
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var request = new UpdateCustomerRequest
        {
            Name = " ", // invalid
            PhoneNumber = customer.PhoneNumber!,
            Email = customer.Email!,
            Vehicles = customer.Vehicles.Select(v => new UpdateVehicleRequest
            {
                VehicleId = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate
            }).ToList()
        };

        try
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            using var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{customer.Id}", requestContent);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally
        {
            await _context.Customers
                .Where(c => c.Id == customer.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidCustomerId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var customer = CustomerFactory.CreateCustomer().Value;

        var request = new UpdateCustomerRequest
        {
            Name = "Updated Name",
            PhoneNumber = customer.PhoneNumber!,
            Email = customer.Email!,
            Vehicles = customer.Vehicles.Select(v => new UpdateVehicleRequest
            {
                VehicleId = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate
            }).ToList()
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{customer.Id}", requestContent);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_WithValidData_ShouldReturnSuccess()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var customer = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        using var response = await _client.DeleteAsync($"/api/v1.0/customers/{customer.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var exists = await _context.Customers
                .AsNoTracking()
                .AnyAsync(c => c.Id == customer.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteCustomer_WithNotFoundCustomerId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var customerId = Guid.NewGuid();

        using var response = await _client.DeleteAsync($"/api/v1.0/customers/{customerId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_AsLabor_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var customerId = Guid.NewGuid();

        using var response = await _client.DeleteAsync($"/api/v1.0/customers/{customerId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}