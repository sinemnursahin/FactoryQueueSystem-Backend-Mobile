namespace FactoryQueueSystem.Api.DTOs.Admin;

public record AdminShipmentResponse(
    Guid ShipmentId,
    Guid VehicleId,
    string PlateNumber,
    string DriverName,
    int Status,
    string StatusName,
    int? QueueNumber,
    DateOnly? QueueDate,
    string? RawMaterialName,
    string? SupplierName,
    DateTime? QueuedAt,
    decimal? LoadedWeight,
    decimal? EmptyWeight,
    decimal? NetAmount,
    DateTime? LoadedWeighDate,
    DateTime? EmptyWeighDate,
    DateTime? CompletedAt);
