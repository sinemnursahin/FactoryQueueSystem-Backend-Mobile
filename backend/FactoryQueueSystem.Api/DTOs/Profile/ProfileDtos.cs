namespace FactoryQueueSystem.Api.DTOs.Profile;

public record ProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber);

public record ProfileUpdateRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber);
