using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.RemoveCustomer;

public class RemoveCustomerCommandValidatorTest
{
    private readonly RemoveCustomerCommandValidator _validator;

    public RemoveCustomerCommandValidatorTest()
    {
        _validator = new RemoveCustomerCommandValidator();
    }

    [Fact]
    public void Validate_When_CustomerId_Is_Empty_Should_Return_Error()
    {
        var command = new RemoveCustomerCommand(Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_When_CustomerId_Is_Valid_Should_Success()
    {
        var command = new RemoveCustomerCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}