using System.Net;
using System.Net.Http.Json;
using System.Security;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;
using MechanicShop.Tests.Common.Security;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IdentityControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    [Fact]
    public async Task GenerateToken_WithValidCredentials_ReturnsOk()
    {
        var user = TestUsers.Labor01;

        var request = new GenerateTokenQuery(
            Email: user.Email!,
            Password: "1234$5678");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact]
    public async Task GenerateToken_WithInValidCredentials_ReturnsBadRequest()
    {
        var user = TestUsers.Labor01;

        var request = new GenerateTokenQuery(
            Email: user.Email!,
            Password: " ");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewAccessToken()
    {
        var user = TestUsers.Labor01;

        var generateTokenRequest = new GenerateTokenQuery(
            Email: user.Email!,
            Password: "1234$5678");

        var generateTokenResponse = await _client.PostAsJsonAsync("/identity/token/generate", generateTokenRequest);
        Assert.Equal(HttpStatusCode.OK, generateTokenResponse.StatusCode);

        var tokenResult = await generateTokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokenResult);

        var refreshToken = tokenResult!.RefreshToken;
        Assert.NotNull(refreshToken);

        var refreshTokenRequest = new RefreshTokenQuery(
            RefreshToken: refreshToken,
            ExpiredAccessToken: tokenResult.AccessToken!);

        var refreshTokenResponse = await _client.PostAsJsonAsync("/identity/token/refresh-token", refreshTokenRequest);
        Assert.Equal(HttpStatusCode.OK, refreshTokenResponse.StatusCode);

        var refreshedTokenResult = await refreshTokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(refreshedTokenResult);
        Assert.False(string.IsNullOrWhiteSpace(refreshedTokenResult.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshedTokenResult.RefreshToken));
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRequest_ReturnsBadRequest()
    {
        var user = TestUsers.Labor01;

        var generateTokenRequest = new GenerateTokenQuery(
            Email: user.Email!,
            Password: "1234$5678");

        var generateTokenResponse = await _client.PostAsJsonAsync("/identity/token/generate", generateTokenRequest);
        Assert.Equal(HttpStatusCode.OK, generateTokenResponse.StatusCode);

        var tokenResult = await generateTokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokenResult);

        var refreshToken = tokenResult!.RefreshToken;
        Assert.NotNull(refreshToken);

        var refreshTokenRequest = new RefreshTokenQuery(
            RefreshToken: refreshToken,
            ExpiredAccessToken: " ");

        var refreshTokenResponse = await _client.PostAsJsonAsync("/identity/token/refresh-token", refreshTokenRequest);
        Assert.Equal(HttpStatusCode.BadRequest, refreshTokenResponse.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserInfo_WithValidData_ReturnsUserInfo()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/identity/current-user/claims");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AppUserDto>();
        Assert.NotNull(result);
        Assert.Equal(TestUsers.Manager.Id, result.UserId);
        Assert.Equal(TestUsers.Manager.Email, result.Email);
        Assert.Equal("Manager", result.Roles[0]);
    }

    [Fact]
    public async Task GetCurrentUserInfo_WithoutAuthorization_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/identity/current-user/claims");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}