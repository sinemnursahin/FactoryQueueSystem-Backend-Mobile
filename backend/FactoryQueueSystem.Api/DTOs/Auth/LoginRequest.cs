namespace FactoryQueueSystem.Api.DTOs.Auth;

public record LoginRequest(string EmailOrPhone, string Password);
