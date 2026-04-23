using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GenerateTokens;

public class GenerateTokenQueryValidatorsTests
{
    private readonly GenerateTokenQueryValidator _validator;

    public GenerateTokenQueryValidatorsTests()
    {
        _validator = new GenerateTokenQueryValidator();
    }

    [Fact]
    public void Validate_When_Email_And_Password_Are_Valid_Should_Success()
    {
        var query = new GenerateTokenQuery("doe@gmail.com", Password: "123456");

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_When_Email_Is_Null_Should_Return_Error()
    {
        var query = new GenerateTokenQuery(Email: null!, Password: "123456");

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode.StartsWith("Email"));
    }

    [Fact]
    public void Validate_When_Email_Is_Empty_Should_Return_Error()
    {
        var query = new GenerateTokenQuery(Email: string.Empty, Password: "123456");

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode.StartsWith("Email"));
    }

    [Fact]
    public void Validate_When_Password_Is_Empty_Should_Return_Error()
    {
        var query = new GenerateTokenQuery(Email: "doe@gmail.com", Password: string.Empty);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode.StartsWith("Password"));
    }

    [Fact]
    public void Validate_When_Password_Is_Null_Should_Return_Error()
    {
        var query = new GenerateTokenQuery(Email: "doe@gmail.com", Password: null!);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode.StartsWith("Password"));
    }
}