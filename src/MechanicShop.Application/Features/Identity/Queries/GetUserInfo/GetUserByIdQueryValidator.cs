using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(u => u.UserId)
        .NotEmpty()
        .WithErrorCode("UserId_Is_Required")
        .WithMessage("UserId is required");
    }
}