namespace FactoryQueueSystem.Api.DTOs.Admin;

public record AdminUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string Role,
    DateTime CreatedAt,
    bool IsDeleted,
    DateTime? DeletedAt);

public record AdminUserUpdateRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string Role);
