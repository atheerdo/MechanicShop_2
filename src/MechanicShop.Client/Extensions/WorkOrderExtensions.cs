namespace MechanicShop.Client.Extensions;

using MechanicShop.Client.Models;

/// <summary>
/// Extension methods for WorkOrder models.
/// </summary>
public static class WorkOrderExtensions
{
    /// <summary>
    /// Adjusts the start and end times of a WorkOrder to local time.
    /// </summary>
    /// <param name="workOrder">The work order model to adjust.</param>
    public static void AdjustTimeToLocal(this WorkOrderModel workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        workOrder.StartAtUtc = workOrder.StartAtUtc.ToLocalTime();
        workOrder.EndAtUtc = workOrder.EndAtUtc.ToLocalTime();
    }

    /// <summary>
    /// Adjusts the start and end times of a WorkOrderListItem to local time.
    /// </summary>
    /// <param name="workOrder">The work order list item model to adjust.</param>
    public static void AdjustTimeToLocal(this WorkOrderListItemModel workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        workOrder.StartAtUtc = workOrder.StartAtUtc.ToLocalTime();
        workOrder.EndAtUtc = workOrder.EndAtUtc.ToLocalTime();
    }
}