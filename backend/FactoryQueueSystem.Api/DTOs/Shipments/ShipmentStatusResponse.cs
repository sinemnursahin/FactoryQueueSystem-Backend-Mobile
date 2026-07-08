namespace FactoryQueueSystem.Api.DTOs.Shipments;

public record ShipmentStatusResponse(
    Guid Id,
    int Status,
    string StatusName,
    int? QueueNumber,
    DateOnly? QueueDate,
    DateTime? QueuedAt,
    DateTime? CompletedAt,
    int TotalQueuedVehicles,
    int? VehiclesAhead);
