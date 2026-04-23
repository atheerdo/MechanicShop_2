using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Queries.GetLabors;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Queries.GetLabors;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetLaborsQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();

    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Query_Labors_Should_Success()
    {
        var labor1 = EmployeeFactory.CreateLabor().Value;
        var labor2 = EmployeeFactory.CreateLabor().Value;

        await _context.Employees.AddRangeAsync(labor1, labor2);
        await _context.SaveChangesAsync(default);

        var query = new GetLaborsQuery();

        var result = await _mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, e => e.LaborId == labor1.Id);
        Assert.Contains(result.Value, e => e.LaborId == labor2.Id);
    }
}