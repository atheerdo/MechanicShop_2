using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
    public RelocateWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
        .NotEmpty()
        .WithMessage("WorkOrderId is required");

        RuleFor(x => x.NewStartAt)
        .NotEmpty()
        .WithMessage("New Start time must be in future.");

        RuleFor(x => x.NewSpot)
        .IsInEnum()
        .WithErrorCode("WorkOrder_SpotInvalid");
    }
}