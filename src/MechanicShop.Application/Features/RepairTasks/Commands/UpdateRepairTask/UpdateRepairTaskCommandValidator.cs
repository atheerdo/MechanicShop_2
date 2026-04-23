using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdateRepairTaskCommandValidator : AbstractValidator<UpdateRepairTaskCommand>
{
    public UpdateRepairTaskCommandValidator()
    {
        RuleFor(rt => rt.RepairTaskId)
            .NotEmpty().WithMessage("Repair task id is required.");

        RuleFor(rt => rt.Name)
            .NotEmpty().WithMessage("Name of repair task is required.")
            .MaximumLength(100).WithMessage("Task name must not exceed 100 characters.");

        RuleFor(rt => rt.LaborCost)
            .NotEmpty().WithMessage("Labor cost must be greater than 0")
            .GreaterThan(0);

        RuleFor(rt => rt.EstimatedDurationInMinutes)
            .IsInEnum()
            .WithMessage("Invalid duration selected.");

        RuleFor(rt => rt.Parts)
            .NotEmpty().WithMessage("Parts must not be null.")
            .Must(p => p.Count > 0).WithMessage("At least one part is required.");

        RuleForEach(rt => rt.Parts).SetValidator(new UpdateRepairTaskPartCommandValidator());
    }
}