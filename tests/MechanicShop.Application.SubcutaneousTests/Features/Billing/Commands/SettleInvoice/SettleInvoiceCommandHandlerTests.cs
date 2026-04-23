using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettleInvoiceCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_Invoice_Not_Found_Should_Return_InvoiceNotFound()
    {
        // Arrange
        var command = new SettleInvoiceCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.Invoice.NotFound");
    }

    [Fact]
    public async Task Handle_When_Invoice_Failed_To_Paid_Should_Fail()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.UpdateState(WorkOrderState.InProgress);
        workOrder.UpdateState(WorkOrderState.Completed);

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new IssueInvoiceCommand(workOrder.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsSuccess);

        var invoiceId = result.Value.InvoiceId;

        var settleCommand = new SettleInvoiceCommand(invoiceId);
        var firstSettleResult = await _mediator.Send(settleCommand);

        Assert.True(firstSettleResult.IsSuccess);

        var secondSettleResult = await _mediator.Send(settleCommand);

        // Assert
        Assert.True(secondSettleResult.IsError);
        Assert.Equal("Invoice.Locked", secondSettleResult.TopError.Code);
    }

    [Fact]
    public async Task Handle_When_Invoice_Settle_Is_Valid_Should_Success()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.UpdateState(WorkOrderState.InProgress);
        workOrder.UpdateState(WorkOrderState.Completed);

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new IssueInvoiceCommand(workOrder.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsSuccess);

        var invoiceId = result.Value.InvoiceId;

        var settleCommand = new SettleInvoiceCommand(invoiceId);
        var settleResult = await _mediator.Send(settleCommand);

        Assert.True(settleResult.IsSuccess);

        var invoice = await _context.Invoices.FindAsync(invoiceId);
        Assert.Equal(InvoiceStatus.Paid, invoice!.Status);
    }
}