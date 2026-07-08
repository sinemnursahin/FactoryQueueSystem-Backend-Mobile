namespace FactoryQueueSystem.Api.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
