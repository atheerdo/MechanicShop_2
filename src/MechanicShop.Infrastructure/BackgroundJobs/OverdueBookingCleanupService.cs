using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.BackgroundJobs;

public class OverdueBookingCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<OverdueBookingCleanupService> logger,
        IOptions<AppSettings> options,
        TimeProvider dateTime) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OverdueBookingCleanupService> _logger = logger;
    private readonly TimeProvider _dateTime = dateTime;
    private readonly AppSettings _appSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while(!ct.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Start cancelling overdue WorkOrder on: {Now}", _dateTime.GetUtcNow());

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                var overdueAppointments = await context.WorkOrders.Where(w =>
                    w.State == WorkOrderState.Scheduled
                    && w.StartAtUtc.AddMinutes(_appSettings
                    .BookingCancellationThresholdMinutes) <= _dateTime.GetUtcNow().UtcDateTime)
                    .ToListAsync(ct);

                if(overdueAppointments.Count > 0)
                {
                    foreach(var workOrder in overdueAppointments)
                    {
                        workOrder.Cancel();
                    }

                    await context.SaveChangesAsync(ct);

                    _logger.LogInformation("Cancelled {Count} overdue WorkOrders.", overdueAppointments.Count);
                }
                else
                {
                    _logger.LogInformation("No overdue WorkOrders found to cancel.");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up overdue bookings.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_appSettings.OverdueBookingCleanupFrequencyMinutes), ct);
        }
    }
}