using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MechanicShop.Infrastructure.RealTime;

public class SignalRWorkOrderNotifier(IHubContext<WorkOrderHub> hubContext)
    : IWorkOrderNotifier
{
    private readonly IHubContext<WorkOrderHub> _hubContext = hubContext;

    public Task NotifyWorkOrdersChangedAsync(CancellationToken ct = default) =>
        _hubContext.Clients.All.SendAsync("WorkOrdersChanged", ct);
}
