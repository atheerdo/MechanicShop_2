using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GenerateTokens;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GenerateTokenQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly UserManager<AppUser> _userManager = factory.UserManager;

    [Fact]
    public async Task Handle_When_User_Not_Found_Should_Fail_And_Return_Not_Found()
    {
        // Arrange
        var user = TestUsers.Labor01;

        var query = new GenerateTokenQuery(user.Email!, "12345$");

        var result = await _mediator.Send(query);

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Manager_Should_Succeed()
    {
        // Arrange
        var user = TestUsers.Manager;

        await _userManager.CreateAsync(user, "123456");

        await _userManager.AddToRoleAsync(user, "Manager");

        var query = new GenerateTokenQuery(user.Email!, "123456");

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
    }
}