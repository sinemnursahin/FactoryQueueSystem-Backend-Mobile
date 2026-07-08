namespace FactoryQueueSystem.Api.Services;

public record ServiceResult<T>(bool Succeeded, T? Value, string? Error, int StatusCode)
{
    public static ServiceResult<T> Ok(T value) => new(true, value, null, StatusCodes.Status200OK);
    public static ServiceResult<T> BadRequest(string error) => new(false, default, error, StatusCodes.Status400BadRequest);
    public static ServiceResult<T> NotFound(string error = "Not found.") => new(false, default, error, StatusCodes.Status404NotFound);
    public static ServiceResult<T> Unauthorized(string error = "Unauthorized.") => new(false, default, error, StatusCodes.Status401Unauthorized);
}
