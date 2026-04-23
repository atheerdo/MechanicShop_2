using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Tests.Common.Auth;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.RefreshTokens;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RefreshTokenQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly UserManager<AppUser> _userManager = factory.UserManager;
    private readonly ITokenProvider _tokenProvider = factory.TokenProvider;

    [Fact]
    public async Task Handle_When_RefreshToken_Is_Valid_Should_Succeed()
    {
        // Arrange
        var user = new AppUser
        {
            UserName = Guid.NewGuid().ToString(),
            Email = $"{Guid.NewGuid()}@test.com"
        };

        var createResult = await _userManager.CreateAsync(user, "123456");
        Assert.True(createResult.Succeeded);

        await _userManager.AddToRoleAsync(user, "Manager");

        var generateTokenQuery = new GenerateTokenQuery(user.Email!, "123456");
        var tokenResponse = await _mediator.Send(generateTokenQuery);

        Assert.True(tokenResponse.IsSuccess);

        var refreshTokenQuery = new RefreshTokenQuery(
            tokenResponse.Value.RefreshToken!,
            tokenResponse.Value.AccessToken!);

        // Act
        var result = await _mediator.Send(refreshTokenQuery);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(
            tokenResponse.Value.AccessToken,
            result.Value.AccessToken);

        // Cleanup
        await _userManager.DeleteAsync(user);
    }

    [Fact]
    public async Task Handle_When_RefreshToken_Is_Invalid_Should_Fail()
    {
        // Arrange
        var refreshTokenQuery = new RefreshTokenQuery(
            "invalid_refresh_token",
            "invalid_access_token");

        // Act
        var result = await _mediator.Send(refreshTokenQuery);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e == ApplicationErrors.ExpiredAccessTokenInvalid);
    }
}