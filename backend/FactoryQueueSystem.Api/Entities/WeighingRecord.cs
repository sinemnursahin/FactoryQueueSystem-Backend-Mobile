namespace FactoryQueueSystem.Api.Entities;

public class WeighingRecord
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public decimal? LoadedWeight { get; set; }
    public decimal? EmptyWeight { get; set; }
    public decimal? NetAmount { get; set; }
    public DateTime? LoadedWeighDate { get; set; }
    public DateTime? EmptyWeighDate { get; set; }

    public Shipment Shipment { get; set; } = null!;
}
