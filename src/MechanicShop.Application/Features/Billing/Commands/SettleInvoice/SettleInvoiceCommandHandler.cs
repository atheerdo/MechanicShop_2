using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

public class SettleInvoiceCommandHandler(
        ILogger<SettleInvoiceCommandHandler> logger,
        IAppDbContext context,
        HybridCache cache,
        TimeProvider datetime) : IRequestHandler<SettleInvoiceCommand, Result<Success>>
{
    private readonly ILogger<SettleInvoiceCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _datetime = datetime;

    public async Task<Result<Success>> Handle(SettleInvoiceCommand command, CancellationToken ct)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == command.InvoiceId, ct);

        if (invoice is null)
        {
            _logger.LogError("Invoice with id:{InvoiceId} not found.", command.InvoiceId);

            return ApplicationErrors.InvoiceNotFound;
        }

        var payInvoiceResult = invoice.MarkAsPaid(_datetime);

        if (payInvoiceResult.IsError)
        {
            _logger.LogError(
                   "Invoice payment failed for id: {InvoiceId}. Errors: {Errors}", invoice.Id, payInvoiceResult.Errors);

            return payInvoiceResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("invoice", ct);

        _logger.LogInformation("Invoice with id: {InvoiceId} paid successfully.", invoice.Id);

        return Result.Success;
    }
}