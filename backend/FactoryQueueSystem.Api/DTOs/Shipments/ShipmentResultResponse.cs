namespace FactoryQueueSystem.Api.DTOs.Shipments;

public record ShipmentResultResponse(
    Guid ShipmentId,
    decimal LoadedWeight,
    decimal EmptyWeight,
    decimal NetAmount,
    DateTime LoadedWeighDate,
    DateTime EmptyWeighDate,
    DateTime CompletedAt);
