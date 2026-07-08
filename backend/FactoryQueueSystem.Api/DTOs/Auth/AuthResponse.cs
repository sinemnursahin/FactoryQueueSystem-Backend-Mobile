namespace FactoryQueueSystem.Api.DTOs.Auth;

public record AuthResponse(
    string Token,
    Guid UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string Role);
