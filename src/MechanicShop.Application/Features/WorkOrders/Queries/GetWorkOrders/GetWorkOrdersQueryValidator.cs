using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryValidator
                : AbstractValidator<GetWorkOrdersQuery>
{
    private static readonly string[] AllowedSortColumns =
    {
        "createdAt",
        "startAt",
        "endAt",
        "state"
    };

    public GetWorkOrdersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.SortDirection)
            .Must(d => d is "asc" or "desc")
            .WithMessage("SortDirection must be 'asc' or 'desc'.");

        RuleFor(x => x.SortColumn)
            .Must(c => AllowedSortColumns.Contains(c))
            .WithMessage("Invalid sort column.");

        RuleFor(x => x.State)
            .IsInEnum()
            .When(x => x.State.HasValue);

        RuleFor(x => x.Spot)
            .IsInEnum()
            .When(x => x.Spot.HasValue);

        RuleFor(x => x)
            .Must(x =>
                !x.StartDateFrom.HasValue ||
                !x.StartDateTo.HasValue ||
                x.StartDateFrom <= x.StartDateTo)
            .WithMessage("StartDateFrom must be before StartDateTo.");

        RuleFor(x => x)
            .Must(x =>
                !x.EndDateFrom.HasValue ||
                !x.EndDateTo.HasValue ||
                x.EndDateFrom <= x.EndDateTo)
            .WithMessage("EndDateFrom must be before EndDateTo.");
    }
}
