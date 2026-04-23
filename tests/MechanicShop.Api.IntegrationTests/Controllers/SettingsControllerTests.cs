using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Contracts.Responses;
using MechanicShop.Tests.Common.Security;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettingsControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    [Fact]
    public async Task GetOperatingHours_WithValidManagerUser_ReturnsOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/settings/operating-hours");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<OperatingHoursResponse>();
        Assert.NotNull(result);
        Assert.Equal(new TimeOnly(0, 0, 0), result.OpeningTime);
        Assert.Equal(new TimeOnly(23, 59, 59), result.ClosingTime);
    }

    [Fact]
    public async Task GetOperatingHours_WithValidLaborUser_ReturnsOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/settings/operating-hours");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<OperatingHoursResponse>();
        Assert.NotNull(result);
        Assert.Equal(new TimeOnly(0, 0, 0), result.OpeningTime);
        Assert.Equal(new TimeOnly(23, 59, 59), result.ClosingTime);
    }
}