using FluentValidation;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;


public sealed class GetWorkOrderStatsQueryValidator : AbstractValidator<GetWorkOrderStatsQuery>
{
    public GetWorkOrderStatsQueryValidator()
    {
        RuleFor(w => w.Date)
        .NotEmpty()
        .WithErrorCode("Date_Is_Required")
        .WithMessage("Date is required.");
    }
}