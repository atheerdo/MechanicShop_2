using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Customer name is required.")
            .MaximumLength(100);

        RuleFor(c => c.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?\d{7,15}$").WithMessage("Phone number is not valid, must be 7-15 digits and should start with '+' if international.");

        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email is required.")
            .MaximumLength(100);

        RuleFor(c => c.Vehicles)
            .NotNull()
            .WithMessage("Vehicle list cannot be null.")
            .Must(v => v.Count > 0).WithMessage("At least one vehicle is required.");

        RuleForEach(c => c.Vehicles).SetValidator(new CreateVehicleCommandValidator());
    }
}