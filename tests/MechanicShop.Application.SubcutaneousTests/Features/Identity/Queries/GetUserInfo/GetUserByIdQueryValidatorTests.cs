using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdQueryValidatorTests
{
    private readonly GetUserByIdQueryValidator _validator;

    public GetUserByIdQueryValidatorTests()
    {
        _validator = new GetUserByIdQueryValidator();
    }

    [Fact]
    public void Validate_When_UserId_Is_Empty_Should_Return_Error()
    {
        var user = new GetUserByIdQuery(string.Empty);

        var result = _validator.Validate(user);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "UserId_Is_Required");
    }

    [Fact]
    public void Validate_When_UserId_Is_Valid_Should_Success()
    {
        var user = new GetUserByIdQuery("12345");

        var result = _validator.Validate(user);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}