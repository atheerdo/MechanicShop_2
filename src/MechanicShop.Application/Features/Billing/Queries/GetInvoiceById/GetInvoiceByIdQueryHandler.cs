using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(
                    ILogger<GetInvoiceByIdQueryHandler> logger,
                    IAppDbContext context)
                    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly ILogger<GetInvoiceByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await _context.Invoices.AsNoTracking()
                          .Include(i => i.LineItems)
                          .Include(i => i.WorkOrder!)
                              .ThenInclude(w => w.Vehicle!)
                                 .ThenInclude(v => v.Customer)
                          .FirstOrDefaultAsync(i => i.Id == query.InvoiceId);

        if (invoice is null)
        {
            _logger.LogError("No invoice with id:{InvoiceId}", query.InvoiceId);

            return ApplicationErrors.InvoiceNotFound;
        }

        return invoice.ToDto();
    }
}