using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserInfo;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetUserByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();

    private readonly UserManager<AppUser> _userManager = factory.UserManager;

    [Fact]
    public async Task Handle_When_User_Not_Found_Should_Return_NotFound_Error()
    {
        var userId = "12345"; // guaranteed not to exist

        var query = new GetUserByIdQuery(userId);

        var result = await _mediator.Send(query);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "User.NotFound");
    }

    [Fact]
    public async Task Handle_When_User_Exists_Should_Success()
    {
        var user = TestUsers.Labor01;

        var createResult = await _userManager.CreateAsync(user, "Password123!");
        Assert.True(createResult.Succeeded);

        await _userManager.AddToRoleAsync(user, "Labor");

        var query = new GetUserByIdQuery(user.Id);

        var result = await _mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Contains("Labor", result.Value.Roles);
    }
}