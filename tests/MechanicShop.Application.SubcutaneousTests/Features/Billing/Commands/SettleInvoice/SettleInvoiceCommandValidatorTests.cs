using System.Threading.Tasks;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

public class SettleInvoiceCommandValidatorTests
{
    private readonly SettleInvoiceCommandValidator _validator;

    public SettleInvoiceCommandValidatorTests()
    {
        _validator = new SettleInvoiceCommandValidator();
    }

    [Fact]
    public void Validate_When_InvoiceId_Is_Empty_Should_Have_Error()
    {
        var command = new SettleInvoiceCommand(Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InvoiceId");
    }

    [Fact]
    public void Validate_When_InvoiceId_Is_Valid_Should_Succeed()
    {
        var command = new SettleInvoiceCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}