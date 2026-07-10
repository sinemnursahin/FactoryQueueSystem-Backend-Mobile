namespace FactoryQueueSystem.Api.DTOs.Profile;

public record ProfileVehicleResponse(
    Guid Id,
    string PlateNumber);

public record ProfileVehicleRequest(
    string PlateNumber);

public record AssignShipmentVehicleRequest(
    string? PlateNumber);
