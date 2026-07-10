namespace FactoryQueueSystem.Api.DTOs.Admin;

public record AdminVehicleResponse(
    Guid Id,
    string PlateNumber,
    Guid UserId,
    string DriverName,
    DateTime CreatedAt,
    Guid? CurrentShipmentId,
    int? CurrentStatus,
    string? CurrentStatusName,
    bool IsDeleted,
    DateTime? DeletedAt);

public record AdminVehicleRequest(
    string PlateNumber,
    Guid UserId);
