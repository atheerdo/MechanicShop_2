using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandHandler(
                            ILogger<UpdateRepairTaskCommandHandler> logger,
                            IAppDbContext context,
                            HybridCache cache)
                            : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
                        {
    private readonly ILogger<UpdateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand command, CancellationToken ct)
    {
        var repairTask = await _context.RepairTasks
                .Include(rt => rt.Parts)
                .FirstOrDefaultAsync(rt => rt.Id == command.RepairTaskId, ct);

        if (repairTask is null)
        {
            _logger.LogError("Repair task with id: '{RepairTaskId}' does not exist.", command.RepairTaskId);

            return ApplicationErrors.RepairTaskNotFound;
        }

        var validateParts = new List<Part>();

        foreach (var p in command.Parts)
        {
            var partId = p.PartId ?? Guid.NewGuid();

            var partResult = Part.Create(partId, p.Name, p.Cost, p.Quantity);

            if (partResult.IsError)
            {
                return partResult.Errors;
            }

            validateParts.Add(partResult.Value);
        }

        var updateRepairTaskResult = repairTask.Update(
                                        command.Name,
                                        command.LaborCost,
                                        command.EstimatedDurationInMinutes!.Value);

        var upsertPartsResult = repairTask.UpsertParts(validateParts);

        if (upsertPartsResult.IsError)
        {
            return upsertPartsResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("repair_task");

        return Result.Updated;
    }
}