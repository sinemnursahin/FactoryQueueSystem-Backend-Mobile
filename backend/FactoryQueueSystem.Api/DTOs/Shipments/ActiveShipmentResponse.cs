namespace FactoryQueueSystem.Api.DTOs.Shipments;

public record ActiveShipmentResponse(
    Guid Id,
    Guid VehicleId,
    string PlateNumber,
    string DriverName,
    int Status,
    string StatusName,
    int? QueueNumber,
    DateOnly? QueueDate,
    string? RawMaterialName,
    string? SupplierName,
    DateTime CreatedAt,
    DateTime? QueuedAt,
    int TotalQueuedVehicles,
    int? VehiclesAhead);
