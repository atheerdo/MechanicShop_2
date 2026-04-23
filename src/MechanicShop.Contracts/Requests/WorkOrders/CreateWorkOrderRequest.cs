using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public class CreateWorkOrderRequest
{
    [Required(ErrorMessage = "Spot is required.")]
    [Range(0, 3, ErrorMessage = "Spot must be [0, 1, 2 0r 3].")]
    public Spot Spot { get; set; }

    [Required(ErrorMessage = "VehicleId is required.")]
    public Guid VehicleId { get; set; }

    [MinLength(1, ErrorMessage = "At least one repair task must be selected.")]
    public List<Guid> RepairTaskIds { get; set; } = [];

    [Required(ErrorMessage = "Labor is required.")]
    public Guid LaborId { get; set; }

    [Required(ErrorMessage = "StartAtUtc is required.")]
    public DateTimeOffset StartAtUtc { get; set; }
}