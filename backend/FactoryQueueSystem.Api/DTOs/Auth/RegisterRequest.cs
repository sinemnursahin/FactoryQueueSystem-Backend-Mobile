namespace FactoryQueueSystem.Api.DTOs.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string Password);
