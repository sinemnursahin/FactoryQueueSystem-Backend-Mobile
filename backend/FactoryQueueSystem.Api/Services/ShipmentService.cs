using System.Security.Claims;
using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Profile;
using FactoryQueueSystem.Api.DTOs.Shipments;
using FactoryQueueSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class ShipmentService(AppDbContext db, QueueNumberService queueNumberService, FactoryClock clock, ProfileVehicleService profileVehicleService)
{
    public async Task<ServiceResult<ActiveShipmentResponse>> GetActiveAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId == null)
        {
            return ServiceResult<ActiveShipmentResponse>.Unauthorized();
        }

        var shipment = await BaseDriverQuery(userId.Value)
            .Where(x => x.Status != ShipmentStatus.Completed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (shipment == null)
        {
            shipment = await BaseDriverQuery(userId.Value)
                .Where(x => x.Status == ShipmentStatus.Completed)
                .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (shipment == null)
        {
            shipment = await CreateDemoShipmentForDriverAsync(userId.Value);
        }

        return shipment == null
            ? ServiceResult<ActiveShipmentResponse>.NotFound("Aktif sevkiyat bulunamadı.")
            : ServiceResult<ActiveShipmentResponse>.Ok(await ToActiveResponseAsync(shipment));
    }

    public async Task<ServiceResult<ShipmentStatusResponse>> QueueAsync(Guid shipmentId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId == null)
        {
            return ServiceResult<ShipmentStatusResponse>.Unauthorized();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var shipment = await BaseDriverQuery(userId.Value).FirstOrDefaultAsync(x => x.Id == shipmentId);
        if (shipment == null)
        {
            return ServiceResult<ShipmentStatusResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (shipment.VehicleId == null)
        {
            return ServiceResult<ShipmentStatusResponse>.BadRequest("Sıraya girmek için önce araç plakası girilmelidir.");
        }

        if (shipment.Status != ShipmentStatus.OnTheWay || shipment.QueueNumber != null)
        {
            return ServiceResult<ShipmentStatusResponse>.BadRequest("Sevkiyat sadece yoldayken ve bir kez sıraya alınabilir.");
        }

        var next = await queueNumberService.NextAsync();
        shipment.Status = ShipmentStatus.InQueue;
        shipment.QueueDate = next.QueueDate;
        shipment.QueueNumber = next.QueueNumber;
        shipment.QueuedAt = clock.UtcNow;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return ServiceResult<ShipmentStatusResponse>.Ok(await ToStatusResponseAsync(shipment));
    }

    public async Task<ServiceResult<ActiveShipmentResponse>> AssignVehicleAsync(Guid shipmentId, ClaimsPrincipal user, AssignShipmentVehicleRequest request)
    {
        var userId = GetUserId(user);
        if (userId == null)
        {
            return ServiceResult<ActiveShipmentResponse>.Unauthorized();
        }

        var driver = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == "Driver");
        if (driver == null)
        {
            return ServiceResult<ActiveShipmentResponse>.Unauthorized();
        }

        var shipment = await BaseDriverQuery(userId.Value).FirstOrDefaultAsync(x => x.Id == shipmentId);
        if (shipment == null)
        {
            return ServiceResult<ActiveShipmentResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (shipment.Status != ShipmentStatus.OnTheWay)
        {
            return ServiceResult<ActiveShipmentResponse>.BadRequest("Araç sadece sıraya girmeden önce atanabilir.");
        }

        var vehicleResult = await profileVehicleService.GetOrCreateOwnedVehicleAsync(driver, request.PlateNumber);
        if (!vehicleResult.Succeeded || vehicleResult.Value == null)
        {
            return ServiceResult<ActiveShipmentResponse>.BadRequest(vehicleResult.Error ?? "Araç atanamadı.");
        }

        var vehicle = vehicleResult.Value;
        var hasOtherActiveShipment = await db.Shipments.AnyAsync(x =>
            x.Id != shipment.Id &&
            x.VehicleId == vehicle.Id &&
            x.Status != ShipmentStatus.Completed);
        if (hasOtherActiveShipment)
        {
            return ServiceResult<ActiveShipmentResponse>.BadRequest("This vehicle already has an active shipment.");
        }

        shipment.VehicleId = vehicle.Id;
        shipment.Vehicle = vehicle;
        await db.SaveChangesAsync();
        return ServiceResult<ActiveShipmentResponse>.Ok(await ToActiveResponseAsync(shipment));
    }

    public async Task<ServiceResult<ShipmentStatusResponse>> GetStatusAsync(Guid shipmentId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId == null)
        {
            return ServiceResult<ShipmentStatusResponse>.Unauthorized();
        }

        var shipment = await BaseDriverQuery(userId.Value).FirstOrDefaultAsync(x => x.Id == shipmentId);
        return shipment == null
            ? ServiceResult<ShipmentStatusResponse>.NotFound("Sevkiyat bulunamadı.")
            : ServiceResult<ShipmentStatusResponse>.Ok(await ToStatusResponseAsync(shipment));
    }

    public async Task<ServiceResult<ShipmentResultResponse>> GetResultAsync(Guid shipmentId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId == null)
        {
            return ServiceResult<ShipmentResultResponse>.Unauthorized();
        }

        var shipment = await BaseDriverQuery(userId.Value)
            .Include(x => x.WeighingRecord)
            .FirstOrDefaultAsync(x => x.Id == shipmentId);

        if (shipment == null)
        {
            return ServiceResult<ShipmentResultResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (shipment.Status != ShipmentStatus.Completed ||
            shipment.CompletedAt == null ||
            shipment.WeighingRecord?.LoadedWeight == null ||
            shipment.WeighingRecord.EmptyWeight == null ||
            shipment.WeighingRecord.NetAmount == null ||
            shipment.WeighingRecord.LoadedWeighDate == null ||
            shipment.WeighingRecord.EmptyWeighDate == null)
        {
            return ServiceResult<ShipmentResultResponse>.NotFound("Sevkiyat sonucu henüz hazır değil.");
        }

        return ServiceResult<ShipmentResultResponse>.Ok(new ShipmentResultResponse(
            shipment.Id,
            shipment.WeighingRecord.LoadedWeight.Value,
            shipment.WeighingRecord.EmptyWeight.Value,
            shipment.WeighingRecord.NetAmount.Value,
            shipment.WeighingRecord.LoadedWeighDate.Value,
            shipment.WeighingRecord.EmptyWeighDate.Value,
            shipment.CompletedAt.Value));
    }

    public async Task<ServiceResult<ActiveShipmentResponse>> ExitFacilityAsync(Guid shipmentId, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId == null)
        {
            return ServiceResult<ActiveShipmentResponse>.Unauthorized();
        }

        var completedShipment = await BaseDriverQuery(userId.Value)
            .FirstOrDefaultAsync(x => x.Id == shipmentId);
        if (completedShipment == null)
        {
            return ServiceResult<ActiveShipmentResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (completedShipment.Status != ShipmentStatus.Completed)
        {
            return ServiceResult<ActiveShipmentResponse>.BadRequest("Tesisten çıkış sadece tamamlanan sevkiyat için yapılabilir.");
        }

        var existingActive = await BaseDriverQuery(userId.Value)
            .Where(x => x.Id != shipmentId && x.Status != ShipmentStatus.Completed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
        if (existingActive != null)
        {
            return ServiceResult<ActiveShipmentResponse>.Ok(await ToActiveResponseAsync(existingActive));
        }

        var newShipment = await CreateDemoShipmentForDriverAsync(userId.Value);
        return newShipment == null
            ? ServiceResult<ActiveShipmentResponse>.NotFound("Aktif sevkiyat oluşturulamadı.")
            : ServiceResult<ActiveShipmentResponse>.Ok(await ToActiveResponseAsync(newShipment));
    }

    private IQueryable<Shipment> BaseDriverQuery(Guid userId) =>
        db.Shipments
            .IgnoreQueryFilters()
            .Include(x => x.Vehicle)
            .Where(x => x.UserId == userId || (x.UserId == null && x.Vehicle != null && x.Vehicle.UserId == userId));

    private async Task<Shipment?> CreateDemoShipmentForDriverAsync(Guid userId)
    {
        var driver = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == "Driver");
        if (driver == null)
        {
            return null;
        }

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            UserId = driver.Id,
            Status = ShipmentStatus.OnTheWay,
            RawMaterialName = "Demo Hammadde",
            SupplierName = "Demo Tedarikçi",
            CreatedAt = clock.UtcNow
        };
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();
        return shipment;
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private async Task<ActiveShipmentResponse> ToActiveResponseAsync(Shipment shipment)
    {
        var queueStats = await GetQueueStatsAsync(shipment);
        return new ActiveShipmentResponse(
            shipment.Id,
            shipment.VehicleId,
            shipment.Vehicle?.PlateNumber,
            shipment.Vehicle?.DriverName,
            (int)shipment.Status,
            StatusText(shipment.Status),
            shipment.QueueNumber,
            shipment.QueueDate,
            shipment.RawMaterialName,
            shipment.SupplierName,
            shipment.CreatedAt,
            shipment.QueuedAt,
            queueStats.TotalQueuedVehicles,
            queueStats.VehiclesAhead);
    }

    private async Task<ShipmentStatusResponse> ToStatusResponseAsync(Shipment shipment)
    {
        var queueStats = await GetQueueStatsAsync(shipment);
        return new ShipmentStatusResponse(
            shipment.Id,
            (int)shipment.Status,
            StatusText(shipment.Status),
            shipment.QueueNumber,
            shipment.QueueDate,
            shipment.QueuedAt,
            shipment.CompletedAt,
            queueStats.TotalQueuedVehicles,
            queueStats.VehiclesAhead);
    }

    private async Task<(int TotalQueuedVehicles, int? VehiclesAhead)> GetQueueStatsAsync(Shipment shipment)
    {
        if (shipment.QueueDate == null || shipment.QueueNumber == null)
        {
            return (0, null);
        }

        var total = await db.Shipments.CountAsync(x =>
            x.QueueDate == shipment.QueueDate &&
            x.QueueNumber != null &&
            x.Status != ShipmentStatus.Completed);

        var ahead = await db.Shipments.CountAsync(x =>
            x.QueueDate == shipment.QueueDate &&
            x.QueueNumber != null &&
            x.QueueNumber < shipment.QueueNumber &&
            x.Status != ShipmentStatus.Completed);

        return (total, ahead);
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
