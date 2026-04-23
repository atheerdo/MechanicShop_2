using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(i => i.WorkOrderId)
        .NotEmpty()
        .WithErrorCode("WorkOrderId_Is_Required")
        .WithMessage("WorkOrderId is required");
    }
}