using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryValidator : AbstractValidator<GetInvoicePdfQuery>
{
    public GetInvoicePdfQueryValidator()
    {
         RuleFor(i => i.InvoiceId)
        .NotEmpty()
        .WithErrorCode("InvoiceId_Is_Required")
        .WithMessage("InvoiceId is required.");
    }
}