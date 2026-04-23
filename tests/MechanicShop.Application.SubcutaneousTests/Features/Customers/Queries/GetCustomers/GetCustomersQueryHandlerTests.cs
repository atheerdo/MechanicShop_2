using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomersQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();

    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Retrieve_Customers_Should_Success()
    {
        var customer1 = CustomerFactory.CreateCustomer().Value;
        var customer2 = CustomerFactory.CreateCustomer().Value;

        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.SaveChangesAsync(default);

        var query = new GetCustomersQuery();

        var result = await _mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, c => c.CustomerId == customer1.Id);
        Assert.Contains(result.Value, c => c.CustomerId == customer2.Id);
    }
}