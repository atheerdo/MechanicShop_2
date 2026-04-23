using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Tests.Common.Security;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DashboardControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    [Fact]
    public async Task GetTodayStats_WithValidManagerUser_ReturnsTodayWorkOrderStats()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = await _client.GetAsync($"/api/v1.0/dashboard/stats?date={date}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTodayStats_WithValidLaborUser_ReturnsTodayWorkOrderStats()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = await _client.GetAsync($"/api/v1.0/dashboard/stats?date={date}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTodayStats_WithoutAuth_ReturnsUnauthorized()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = await _client.GetAsync($"/api/v1.0/dashboard/stats?date={date}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}