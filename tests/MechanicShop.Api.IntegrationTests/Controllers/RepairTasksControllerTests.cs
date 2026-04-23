using System.Net;
using System.Net.Http.Json;
using ICSharpCode.SharpZipLib.Core;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Contracts.Requests.RepairTasks;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RepairTasksControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    [Fact]
    public async Task GetRepairTasks_WithValidAuthorization_ReturnsNonEmptyList()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/repair-tasks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<RepairTaskDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetRepairTasks_WithoutAuthorization_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/repair-tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRepairTaskById_WithValidId_ReturnRepairTask()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.GetAsync($"/api/v1.0/repair-tasks/{repairTask.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<RepairTaskDto>();
            Assert.NotNull(result);
            Assert.Equal(repairTask.Id, result.RepairTaskId);
        }
        finally
        {
            await _context.RepairTasks
            .Where(r => r.Id == repairTask.Id)
            .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetRepairTaskById_WithInValidId_ReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync($"/api/v1.0/repair-tasks/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_WithValidData_ReturnsCreatedRepairTask()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        try
        {
            var request = new CreateRepairTaskRequest
        {
            Name = repairTask.Name,
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes?)repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => new CreateRepairTaskPartRequest
            {
                Name = p.Name!,
                Cost = p.Cost,
                Quantity = p.Quantity,
            }).ToList()
        };

            var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<RepairTaskDto>();
            Assert.NotNull(result);
            Assert.Equal(repairTask.Name, result.Name);
        }
        finally
        {
            await _context.RepairTasks
            .Where(r => r.Name == repairTask.Name)
            .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateRepairTask_WithInValidData_ReturnsBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var request = new CreateRepairTaskRequest
        {
            Name = repairTask.Name,
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes?)repairTask.EstimatedDurationInMins,
            Parts = [] // Invalid: No parts provided
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_WithoutAuthorization_ReturnsUnauthorized()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var request = new CreateRepairTaskRequest
        {
            Name = repairTask.Name,
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes?)repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => new CreateRepairTaskPartRequest
            {
                Name = p.Name!,
                Cost = p.Cost,
                Quantity = p.Quantity,
            }).ToList()
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_WithWrongRole_ReturnsForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var request = new CreateRepairTaskRequest
        {
            Name = repairTask.Name,
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes?)repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => new CreateRepairTaskPartRequest
            {
                Name = p.Name!,
                Cost = p.Cost,
                Quantity = p.Quantity,
            }).ToList()
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_WithValidData_ReturnsUpdatedRepairTask()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        try
        {
            var request = new UpdateRepairTaskRequest
        {
            Name = "Updated Repair Task Name",
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes)repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => new UpdateRepairTaskPartRequest
            {
                Name = p.Name!,
                Cost = p.Cost,
                Quantity = p.Quantity,
            }).ToList()
        };

            var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{repairTask.Id}", request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var updatedRepairTask = await _context.RepairTasks.AsNoTracking().FirstAsync(rt => rt.Id == repairTask.Id);
            Assert.NotNull(updatedRepairTask);
            Assert.Equal("Updated Repair Task Name", updatedRepairTask.Name);
        }
        finally
        {
            await _context.RepairTasks
            .Where(r => r.Name == "Updated Repair Task Name" || r.Id == repairTask.Id)
            .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateRepairTask_WithInValidData_ReturnsBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        try
        {
            var request = new UpdateRepairTaskRequest
        {
            Name = "Updated Repair Task Name",
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes)repairTask.EstimatedDurationInMins,
            Parts = [] // Invalid: No parts provided
        };

            var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{repairTask.Id}", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally
        {
            await _context.RepairTasks
            .Where(r => r.Id == repairTask.Id)
            .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateRepairTask_WithoutAuthorization_ReturnsUnauthorized()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var request = new UpdateRepairTaskRequest
        {
            Name = "Updated Repair Task Name",
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes)repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => new UpdateRepairTaskPartRequest
            {
                Name = p.Name!,
                Cost = p.Cost,
                Quantity = p.Quantity,
            }).ToList()
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{repairTask.Id}", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_WithLaborRole_ReturnsForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var request = new UpdateRepairTaskRequest
        {
            Name = "Updated Repair Task Name",
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes)repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => new UpdateRepairTaskPartRequest
            {
                Name = p.Name!,
                Cost = p.Cost,
                Quantity = p.Quantity,
            }).ToList()
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{repairTask.Id}", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_WithNonExistentRepairTaskId_ReturnsNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var request = new UpdateRepairTaskRequest
        {
            Name = "Updated Repair Task Name",
            LaborCost = repairTask.LaborCost,
            EstimatedDurationInMins = (Contracts.Common.RepairDurationInMinutes)repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => new UpdateRepairTaskPartRequest
            {
                Name = p.Name!,
                Cost = p.Cost,
                Quantity = p.Quantity,
            }).ToList()
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{repairTask.Id}", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_WithValidRepairTaskId_ReturnsNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{repairTask.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var deletedRepairTask = await _context.RepairTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == repairTask.Id);

        Assert.Null(deletedRepairTask);
    }

    [Fact]
    public async Task DeleteRepairTask_WithNonExistentRepairTaskId_ReturnsNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{nonExistentId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_WithWithLaborRole_ReturnsForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var repairTaskId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{repairTaskId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_WithoutAuthorization_ReturnsUnauthorized()
    {
        var repairTaskId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{repairTaskId}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}



