namespace FactoryQueueSystem.Api.Services;

public class FactoryClock(IConfiguration configuration)
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly FactoryDateToday()
    {
        var timeZoneId = configuration["Factory:TimeZoneId"] ?? "Turkey Standard Time";
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(UtcNow, timeZone));
    }
}
