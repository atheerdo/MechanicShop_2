using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;

public sealed class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
    public GetRepairTaskByIdQueryValidator()
    {
        RuleFor(rt => rt.RepairTaskId)
        .NotEmpty()
        .WithErrorCode("RepairTaskId_Is_Required")
        .WithMessage("Repair task id is required.");
    }
}