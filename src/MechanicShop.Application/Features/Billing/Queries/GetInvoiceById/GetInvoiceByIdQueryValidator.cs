using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(i => i.InvoiceId)
        .NotEmpty()
        .WithErrorCode("InvoiceId_Is_Required")
        .WithMessage("InvoiceId is required.");
    }
}