using FactoryQueueSystem.Api.Entities;

namespace FactoryQueueSystem.Api.DTOs.Admin;

public record AdminDashboardResponse(
    int TodaysVehicles,
    int WaitingQueue,
    int ActiveOperations,
    int CompletedToday,
    List<DashboardQueueItemResponse> CurrentQueue,
    List<DashboardStatusCountResponse> StatusCounts,
    List<DashboardActivityResponse> RecentActivities,
    DateTime GeneratedAt);

public record DashboardQueueItemResponse(
    Guid ShipmentId,
    int? QueueNumber,
    string? PlateNumber,
    string? DriverName,
    DateTime? ArrivalTime,
    int Status,
    string StatusName,
    bool IsNext);

public record DashboardStatusCountResponse(
    ShipmentStatus Status,
    int StatusValue,
    string StatusName,
    int Count);

public record DashboardActivityResponse(
    string Action,
    string? PlateNumber,
    string? DriverName,
    DateTime Timestamp,
    string BadgeClass);
