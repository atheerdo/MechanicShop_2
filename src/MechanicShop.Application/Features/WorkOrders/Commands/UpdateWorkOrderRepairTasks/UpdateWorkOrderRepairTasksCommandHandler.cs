using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandHandler(
            ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
            IAppDbContext context,
            HybridCache cache,
            IWorkOrderPolicy workOrderValidator)
            : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
    private readonly ILogger<UpdateWorkOrderRepairTasksCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderValidator = workOrderValidator;

    public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand command, CancellationToken ct)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.RepairTasks)
            .FirstOrDefaultAsync(w => w.Id == command.WorkOrderId);

        if (workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        if (command.RepairTaskIds.Length == 0)
        {
            _logger.LogError("Empty RepairTaskIds list submitted.");

            return RepairTaskErrors.AtLeastOneRepairTaskIsRequired;
        }

        var requestedTasks = await _context.RepairTasks
                    .Where(t => command.RepairTaskIds.Contains(t.Id))
                    .ToListAsync(ct);

        if (requestedTasks.Count != command.RepairTaskIds.Length)
        {
            var missingIds = command.RepairTaskIds.Except(requestedTasks.Select(t => t.Id)).ToArray();

            _logger.LogError("One or more RepairTasks not found. {ids}", string.Join(", ", missingIds));

            return ApplicationErrors.RepairTaskNotFound;
        }

        var clearExistingResult = workOrder.ClearRepairTasks();

        if (clearExistingResult.IsError)
        {
            return clearExistingResult.Errors;
        }

        foreach (var task in requestedTasks)
        {
            var addRepairTaskResult = workOrder.AddRepairTask(task);

            if (addRepairTaskResult.IsError)
            {
                return addRepairTaskResult.Errors;
            }
        }

        var totalDuration = TimeSpan.FromMinutes(requestedTasks.Sum(t => (int)t.EstimatedDurationInMins));

        var newEndAt = workOrder.StartAtUtc + totalDuration;

        if (_workOrderValidator.IsOutsideOperatingHours(workOrder.StartAtUtc, totalDuration))
        {
            return Error.Conflict("WorkOrder_Outside_OperatingHours", "WorkOrder timing exceeds business hours.");
        }

        var spotCheckResult = await _workOrderValidator.CheckSpotAvailabilityAsync(
            workOrder.Spot,
            workOrder.StartAtUtc,
            newEndAt,
            excludeWorkOrderId: workOrder.Id,
            ct: ct);

        if (spotCheckResult.IsError)
        {
            return spotCheckResult.Errors;
        }

        if (await _workOrderValidator.IsLaborOccupied(workOrder.LaborId, workOrder.Id, workOrder.StartAtUtc, newEndAt))
        {
            return ApplicationErrors.LaborOccupied;
        }

        workOrder.UpdateTiming(workOrder.StartAtUtc, newEndAt);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(ct);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _cache.RemoveByTagAsync("work-order", ct);

        return Result.Updated;
    }
}

