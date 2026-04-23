using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Auth;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Auth;

public class RefreshTokenTests
{
    [Fact]
    public void CreateRefreshToken_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        const string tokenValue = "token";
        var expiresOnUtc = DateTimeOffset.UtcNow.AddDays(7);

        var result = RefreshTokenFactory.CreateRefreshToken(id: id, token: tokenValue, userId: userId, expiresOnUtc: expiresOnUtc);

        Assert.True(result.IsSuccess);
        var token = result.Value;

        Assert.NotNull(token);
        Assert.Equal(tokenValue, token.Token);
        Assert.Equal(userId, token.UserId);
        Assert.False(string.IsNullOrWhiteSpace(token.UserId));
        Assert.True(token.ExpiresOnUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenIdEmpty()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(id: Guid.Empty);

        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRefreshToken_ShouldFail_WhenTokenInvalid(string? invalidToken)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(token: invalidToken);

        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.TokenRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRefreshToken_ShouldFail_WhenUserIdInvalid(string? invalidUserId)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(userId: invalidUserId);

        Assert.True(result.IsError);

        Assert.Equal(RefreshTokenErrors.UserIdRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenExpiresOnUtcIsInPast()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(expiresOnUtc: DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(result.IsError);

        Assert.Equal(RefreshTokenErrors.ExpiryInvalid.Code, result.TopError.Code);
    }
}