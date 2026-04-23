using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Constants;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public class IssueInvoiceCommandHandler(
                    ILogger<IssueInvoiceCommandHandler> logger,
                    IAppDbContext context,
                    HybridCache cache,
                    TimeProvider datetime)
                    : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    private readonly ILogger<IssueInvoiceCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _datetime = datetime;

    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand command, CancellationToken ct)
    {
        var workOrder = await _context.WorkOrders
                .Include(w => w.Vehicle!)
                   .ThenInclude(v => v.Customer)
                .Include(w => w.RepairTasks)
                   .ThenInclude(rt => rt.Parts)
                .FirstOrDefaultAsync(w => w.Id == command.WorkOrderId, ct);

        if (workOrder is null)
        {
            _logger.LogError("Work Order with id: '{WorkOrderId}' does not exist", command.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        if (workOrder.State != WorkOrderState.Completed)
        {
            _logger.LogWarning("Invoice issuance rejected. WorkOrder {WorkOrderId} is not in completed.", command.WorkOrderId);

            return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
        }

        var invoiceId = Guid.NewGuid();

        var lineItems = new List<InvoiceLineItem>();

        var lineNumber = 1;

        foreach (var (task, taskIndex) in workOrder.RepairTasks.Select((t, i) => (t, i + 1)))
        {
            var partsSummary = task.Parts.Any()
               ? string.Join(Environment.NewLine, task.Parts.Select(p => $"    • {p.Name} x{p.Quantity} @ {p.Cost:C}"))
               : "    • No parts";

            var taskDescription =
                $"{taskIndex}: {task.Name}{Environment.NewLine}" +
                $"Labor: {task.LaborCost:C}{Environment.NewLine}" +
                $"Parts: {Environment.NewLine}{partsSummary}";

            var totalPartsCost = task.Parts.Sum(p => p.Quantity * p.Cost);
            var totalTaskCost = task.LaborCost + totalPartsCost;

            var lineItemResult = InvoiceLineItem.Create(
                invoiceId: invoiceId,
                lineNumber: lineNumber++,
                description: taskDescription,
                quantity: 1,
                unitPrice: totalTaskCost);

            if (lineItemResult.IsError)
            {
                return lineItemResult.Errors;
            }

            lineItems.Add(lineItemResult.Value);
        }

        var subTotal = lineItems.Sum(x => x.LineTotal);

        var taxAmount = subTotal * MechanicShopConstants.TaxRate;

        var discountAmount = workOrder.Discount ?? 0m;

        var createInvoiceResult = Invoice.Create(
            id: invoiceId,
            workOrderId: workOrder.Id,
            items: lineItems,
            discountAmount: discountAmount,
            taxAmount: taxAmount,
            datetime: _datetime);

        if (createInvoiceResult.IsError)
        {
            _logger.LogWarning("Invoice creation failed for WorkOrderId: {WorkOrderId}. Errors: {@Errors}", workOrder.Id, createInvoiceResult.Errors);

            return createInvoiceResult.Errors;
        }

        var invoice = createInvoiceResult.Value;

        await _context.Invoices.AddAsync(invoice);

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("invoice", ct);

        _logger.LogInformation("Invoice {InvoiceId} issued for WorkOrder {WorkOrderId}.", invoice.Id, workOrder.Id);

        return invoice.ToDto();
    }
}