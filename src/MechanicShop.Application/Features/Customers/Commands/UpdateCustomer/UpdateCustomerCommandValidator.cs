using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerId)
        .NotEmpty()
        .WithMessage("CustomerId is required");

        RuleFor(c => c.Name)
        .NotEmpty()
        .WithMessage("Name is required")
        .MaximumLength(100);

        RuleFor(c => c.Email)
        .NotEmpty()
        .EmailAddress()
        .WithMessage("Invalid email");

        RuleFor(x => x.PhoneNumber)
         .NotEmpty().WithMessage("Phone number is required.")
         .Matches(@"^\+?\d{7,15}$").WithMessage("Phone number must be 7–15 digits and may start with '+'.");

        RuleFor(c => c.Vehicles)
        .NotNull()
        .WithMessage("Vehicle list can not be null")
        .Must(v => v != null && v.Count > 0).WithMessage("At least one vehicle is required.");

        RuleForEach(c => c.Vehicles).SetValidator(new UpdateVehicleCommandValidator());
    }
}