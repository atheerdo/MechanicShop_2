using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(request => request.Spot)
        .IsInEnum()
        .WithErrorCode("Spot_Invalid")
        .WithMessage("Spot must be a valid Spot value. [A, B, C, D]");

        RuleFor(request => request.VehicleId)
        .NotEmpty()
        .WithMessage("Vehicle id is required.");

        RuleFor(request => request.StartAt)
        .GreaterThan(DateTimeOffset.UtcNow)
        .WithMessage("StartAt date must be in future.");

        RuleFor(request => request.RepairTaskIds)
        .NotEmpty()
        .WithMessage("At least one repair task must be selected.");

        RuleFor(request => request.LaborId)
        .Must(laborId => laborId is null || laborId != Guid.Empty)
        .WithMessage("If provided, laborId must not be empty.");
    }
}