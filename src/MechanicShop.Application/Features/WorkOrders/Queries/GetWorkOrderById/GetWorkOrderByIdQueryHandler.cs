using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public class GetWorkOrderByIdQueryHandler(
        ILogger<GetWorkOrderByIdQueryHandler> logger,
        IAppDbContext context
        )
        : IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
    private readonly ILogger<GetWorkOrderByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery query, CancellationToken ct)
    {
        var workOrder = await _context.WorkOrders
                            .Include(a => a.RepairTasks)
                                .ThenInclude(a => a.Parts)
                            .Include(a => a.Labor)
                            .Include(a => a.Vehicle!)
                                .ThenInclude(v => v.Customer)
                            .Include(a => a.Invoice)
                        .FirstOrDefaultAsync(w => w.Id == query.WorkOrderId, ct);

        if (workOrder is null)
        {
            _logger.LogError("WorkOrder with id {WorkOrderId} was not found", query.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        return workOrder.ToDto();
    }
}