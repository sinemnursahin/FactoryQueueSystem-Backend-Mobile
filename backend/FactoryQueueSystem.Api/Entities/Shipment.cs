namespace FactoryQueueSystem.Api.Entities;

public class Shipment
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public ShipmentStatus Status { get; set; }
    public int? QueueNumber { get; set; }
    public DateOnly? QueueDate { get; set; }
    public string? RawMaterialName { get; set; }
    public string? SupplierName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? QueuedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
    public WeighingRecord? WeighingRecord { get; set; }
}
