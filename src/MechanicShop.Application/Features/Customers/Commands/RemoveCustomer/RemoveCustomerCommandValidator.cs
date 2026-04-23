using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;

public sealed class RemoveCustomerCommandValidator : AbstractValidator<RemoveCustomerCommand>
{
    public RemoveCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerId)
        .NotEmpty()
        .WithMessage("CustomerId is required");
    }
}