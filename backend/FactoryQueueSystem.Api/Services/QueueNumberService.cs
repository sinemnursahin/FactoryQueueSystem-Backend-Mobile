using FactoryQueueSystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class QueueNumberService(AppDbContext db, FactoryClock clock)
{
    public async Task<(DateOnly QueueDate, int QueueNumber)> NextAsync()
    {
        var queueDate = clock.FactoryDateToday();
        var currentMax = await db.Shipments
            .Where(x => x.QueueDate == queueDate)
            .MaxAsync(x => (int?)x.QueueNumber) ?? 0;

        return (queueDate, currentMax + 1);
    }
}
