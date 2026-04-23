using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidatorTests
{
    private readonly GetCustomerByIdQueryValidator _validator;

    public GetCustomerByIdQueryValidatorTests()
    {
        _validator = new GetCustomerByIdQueryValidator();
    }

    [Fact]
    public void Validate_When_CustomerId_Is_Empty_Should_Return_Error()
    {
        var query = new GetCustomerByIdQuery(Guid.Empty);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_When_CustomerId_Is_Valid_Should_Succeed()
    {
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }
}