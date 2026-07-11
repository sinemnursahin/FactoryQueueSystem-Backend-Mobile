using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class AdminDashboardService(AppDbContext db, FactoryClock clock)
{
    private static readonly ShipmentStatus[] DashboardStatuses =
    [
        ShipmentStatus.OnTheWay,
        ShipmentStatus.InQueue,
        ShipmentStatus.CalledToScale,
        ShipmentStatus.AtScale,
        ShipmentStatus.Unloading,
        ShipmentStatus.Completed
    ];

    private static readonly ShipmentStatus[] ActiveStatuses =
    [
        ShipmentStatus.CalledToScale,
        ShipmentStatus.AtScale,
        ShipmentStatus.Unloading
    ];

    public async Task<AdminDashboardResponse> GetAsync()
    {
        var today = DateOnly.FromDateTime(clock.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var tomorrowStart = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var baseQuery = db.Shipments.IgnoreQueryFilters();

        var todaysVehicles = await baseQuery.CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart);
        var waitingQueue = await baseQuery.CountAsync(x => x.Status == ShipmentStatus.InQueue);
        var activeOperations = await baseQuery.CountAsync(x => ActiveStatuses.Contains(x.Status));
        var completedToday = await baseQuery.CountAsync(x =>
            x.Status == ShipmentStatus.Completed &&
            x.CompletedAt >= todayStart &&
            x.CompletedAt < tomorrowStart);

        var currentQueue = await baseQuery
            .Where(x => x.Status == ShipmentStatus.InQueue)
            .Include(x => x.Vehicle)
            .OrderBy(x => x.QueueNumber == null)
            .ThenBy(x => x.QueueNumber)
            .ThenBy(x => x.QueuedAt)
            .Select(x => new DashboardQueueItemResponse(
                x.Id,
                x.QueueNumber,
                x.Vehicle == null ? null : x.Vehicle.PlateNumber,
                x.Vehicle == null ? null : x.Vehicle.DriverName,
                x.QueuedAt,
                (int)x.Status,
                StatusText(x.Status),
                false))
            .ToListAsync();

        if (currentQueue.Count > 0)
        {
            currentQueue[0] = currentQueue[0] with { IsNext = true };
        }

        var groupedCounts = await baseQuery
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var statusCounts = DashboardStatuses
            .Select(status => new DashboardStatusCountResponse(status, (int)status, StatusText(status), groupedCounts.GetValueOrDefault(status)))
            .ToList();

        var recentActivities = await GetRecentActivitiesAsync();

        return new AdminDashboardResponse(
            todaysVehicles,
            waitingQueue,
            activeOperations,
            completedToday,
            currentQueue,
            statusCounts,
            recentActivities,
            clock.UtcNow);
    }

    private async Task<List<DashboardActivityResponse>> GetRecentActivitiesAsync()
    {
        var recentShipments = await db.Shipments.IgnoreQueryFilters()
            .Include(x => x.Vehicle)
            .Include(x => x.WeighingRecord)
            .OrderByDescending(x => x.CompletedAt ?? x.QueuedAt ?? x.CreatedAt)
            .Take(40)
            .ToListAsync();

        var shipmentActivities = recentShipments
            .SelectMany(x =>
            {
                var items = new List<DashboardActivityResponse>
                {
                    new("Araç atandı", x.Vehicle?.PlateNumber, x.Vehicle?.DriverName, x.CreatedAt, "text-bg-primary")
                };

                if (x.QueuedAt != null)
                {
                    items.Add(new DashboardActivityResponse("Araç sıraya girdi", x.Vehicle?.PlateNumber, x.Vehicle?.DriverName, x.QueuedAt.Value, "text-bg-info"));
                }

                if (x.Status == ShipmentStatus.CalledToScale)
                {
                    items.Add(new DashboardActivityResponse("Kantara çağrıldı", x.Vehicle?.PlateNumber, x.Vehicle?.DriverName, x.QueuedAt ?? x.CreatedAt, "text-bg-warning"));
                }

                if (x.WeighingRecord?.LoadedWeighDate != null)
                {
                    items.Add(new DashboardActivityResponse("Boşaltım başladı", x.Vehicle?.PlateNumber, x.Vehicle?.DriverName, x.WeighingRecord.LoadedWeighDate.Value, "text-bg-warning"));
                }

                if (x.CompletedAt != null)
                {
                    items.Add(new DashboardActivityResponse("Sevkiyat tamamlandı", x.Vehicle?.PlateNumber, x.Vehicle?.DriverName, x.CompletedAt.Value, "text-bg-success"));
                }

                return items;
            })
            .ToList();

        var driverActivities = await db.Users.IgnoreQueryFilters()
            .Where(x => x.Role == "Driver")
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new DashboardActivityResponse("Sürücü kaydoldu", null, $"{x.FirstName} {x.LastName}".Trim(), x.CreatedAt, "text-bg-secondary"))
            .ToListAsync();

        return shipmentActivities
            .Concat(driverActivities)
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .ToList();
    }

    private static string StatusText(ShipmentStatus status) => status switch
    {
        ShipmentStatus.OnTheWay => "Yolda",
        ShipmentStatus.InQueue => "Sırada",
        ShipmentStatus.CalledToScale => "Kantara Çağrıldı",
        ShipmentStatus.AtScale => "Kantarda",
        ShipmentStatus.Unloading => "Boşaltımda",
        ShipmentStatus.UnloadingComplete => "Boşaltım Tamamlandı",
        ShipmentStatus.Completed => "Tamamlandı",
        _ => status.ToString()
    };
}
