using FluentValidation.TestHelper;

using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

public class IssueInvoiceCommandValidatorTests
{
    private readonly IssueInvoiceCommandValidator _validator;

    public IssueInvoiceCommandValidatorTests()
    {
        _validator = new IssueInvoiceCommandValidator();
    }

    [Fact]
    public void Validate_When_WorkOrder_Is_Empty_Should_Have_Error()
    {
        var command = new IssueInvoiceCommand(Guid.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.WorkOrderId)
              .WithErrorCode("WorkOrderId_Is_Required")
              .WithErrorMessage("WorkOrderId is required");
    }

    [Fact]
    public void Validate_When_WorkOrder_Exists_Should_Success()
    {
        // Arrange
        var command = new IssueInvoiceCommand(Guid.NewGuid());

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.WorkOrderId);
    }
}