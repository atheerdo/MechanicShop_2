using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandValidator : AbstractValidator<CreateRepairTaskCommand>
{
    public CreateRepairTaskCommandValidator()
    {
        RuleFor(rt => rt.Name)
        .NotEmpty()
        .WithMessage("Name is required")
        .MaximumLength(100);

        RuleFor(rt => rt.LaborCost)
        .GreaterThan(0)
        .WithMessage("Labor cost must be greater than 0.");

        RuleFor(rt => rt.EstimatedDurationInMins)
        .NotNull().WithMessage("Estimated duration is required.")
        .IsInEnum();

        RuleFor(rt => rt.Parts)
        .NotNull().WithMessage("Parts list can not be null")
        .Must(p => p.Count > 0).WithMessage("At least one part is required.");

        RuleForEach(x => x.Parts).SetValidator(new CreateRepairTaskPartCommandValidator());
    }
}