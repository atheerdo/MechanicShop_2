using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class InvoicesControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    [Fact]
    public async Task IssueInvoice_WithValidData_ReturnCreatedInvoice()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
                           .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
                           .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
                           .WithLabor(TestUsers.Labor01.Id)
                           .WithState(WorkOrderState.Completed)
                           .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", (object?)null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<InvoiceDto>();
            Assert.NotNull(result);
            Assert.Equal(workOrder.Id, result.WorkOrderId);
        }
        finally
        {
            await _context.WorkOrders
                .Where(w => w.Id == workOrder.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task IssueInvoice_WithInValidData_ReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var nonExistingInvoiceId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{nonExistingInvoiceId}", (object?)null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoice_ById_ReturnsInvoice()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
                           .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
                           .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
                           .WithLabor(TestUsers.Labor01.Id)
                           .WithState(WorkOrderState.Completed)
                           .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        InvoiceDto? issuedInvoice = null;

        try
        {
            var issueResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", (object?)null);
            issuedInvoice = await issueResponse.Content.ReadFromJsonAsync<InvoiceDto>();

            var getResponse = await _client.GetAsync($"/api/v1.0/invoices/{issuedInvoice!.InvoiceId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var fetchedInvoice = await getResponse.Content.ReadFromJsonAsync<InvoiceDto>();
            Assert.NotNull(fetchedInvoice);
            Assert.Equal(issuedInvoice!.InvoiceId, fetchedInvoice.InvoiceId);
        }
        finally
        {
            await _context.Invoices
                .Where(i => i.Id == issuedInvoice!.InvoiceId)
                .ExecuteDeleteAsync();

            await _context.WorkOrders
                .Where(w => w.Id == workOrder.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetInvoice_pdf_ReturnsInvoice()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
                           .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
                           .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
                           .WithLabor(TestUsers.Labor01.Id)
                           .WithState(WorkOrderState.Completed)
                           .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        InvoiceDto? issuedInvoice = null;

        try
        {
            var issueResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", (object?)null);
            Assert.Equal(HttpStatusCode.Created, issueResponse.StatusCode);

            issuedInvoice = await issueResponse.Content.ReadFromJsonAsync<InvoiceDto>();
            Assert.NotNull(issuedInvoice);

            var pdfResponse = await _client.GetAsync($"/api/v1.0/invoices/{issuedInvoice!.InvoiceId}/pdf");
            Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
            Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType!.MediaType);

            var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(pdfBytes);
        }
        finally
        {
            if (issuedInvoice is not null)
            {
                await _context.Invoices
                    .Where(i => i.Id == issuedInvoice.InvoiceId)
                    .ExecuteDeleteAsync();
            }

            await _context.WorkOrders
                    .Where(w => w.Id == workOrder.Id)
                    .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetInvoicePdf_WithNonExistingInvoiceId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var pdfResponse = await _client.GetAsync($"/api/v1.0/invoices/{Guid.NewGuid()}/pdf");
        Assert.Equal(HttpStatusCode.NotFound, pdfResponse.StatusCode);
    }
}