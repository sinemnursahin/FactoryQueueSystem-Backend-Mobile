using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class AdminVehicleService(AppDbContext db, FactoryClock clock)
{
    public async Task<List<AdminVehicleResponse>> GetAllAsync(bool includeDeleted = false) =>
        await (includeDeleted ? db.Vehicles.IgnoreQueryFilters() : db.Vehicles)
            .OrderBy(x => x.IsDeleted)
            .ThenBy(x => x.PlateNumber)
            .Select(x => ToResponse(x, x.Shipments.Where(s => s.Status != ShipmentStatus.Completed).OrderByDescending(s => s.CreatedAt).FirstOrDefault()))
            .ToListAsync();

    public async Task<ServiceResult<AdminVehicleResponse>> GetAsync(Guid id)
    {
        var vehicle = await db.Vehicles.IgnoreQueryFilters().Include(x => x.Shipments).FirstOrDefaultAsync(x => x.Id == id);
        return vehicle == null
            ? ServiceResult<AdminVehicleResponse>.NotFound("Araç bulunamadı.")
            : ServiceResult<AdminVehicleResponse>.Ok(ToResponse(vehicle, vehicle.Shipments.Where(x => x.Status != ShipmentStatus.Completed).OrderByDescending(x => x.CreatedAt).FirstOrDefault()));
    }

    public async Task<ServiceResult<AdminVehicleResponse>> CreateAsync(AdminVehicleRequest request)
    {
        var validation = await ValidateAsync(request, null);
        if (validation != null)
        {
            return ServiceResult<AdminVehicleResponse>.BadRequest(validation);
        }

        var driver = await db.Users.FirstAsync(x => x.Id == request.UserId);
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            PlateNumber = ContactValidation.NormalizePlate(request.PlateNumber)!,
            UserId = driver.Id,
            DriverName = $"{driver.FirstName} {driver.LastName}".Trim(),
            CreatedAt = clock.UtcNow
        };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        return ServiceResult<AdminVehicleResponse>.Ok(ToResponse(vehicle, null));
    }

    public async Task<ServiceResult<AdminVehicleResponse>> UpdateAsync(Guid id, AdminVehicleRequest request)
    {
        var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == id);
        if (vehicle == null)
        {
            return ServiceResult<AdminVehicleResponse>.NotFound("Araç bulunamadı.");
        }

        var validation = await ValidateAsync(request, id);
        if (validation != null)
        {
            return ServiceResult<AdminVehicleResponse>.BadRequest(validation);
        }

        var driver = await db.Users.FirstAsync(x => x.Id == request.UserId);
        vehicle.PlateNumber = ContactValidation.NormalizePlate(request.PlateNumber)!;
        vehicle.UserId = driver.Id;
        vehicle.DriverName = $"{driver.FirstName} {driver.LastName}".Trim();
        await db.SaveChangesAsync();
        return ServiceResult<AdminVehicleResponse>.Ok(ToResponse(vehicle, null));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        var vehicle = await db.Vehicles.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (vehicle == null)
        {
            return ServiceResult<bool>.NotFound("Araç bulunamadı.");
        }
        if (vehicle.IsDeleted)
        {
            return ServiceResult<bool>.Ok(true);
        }

        var hasActiveShipment = await db.Shipments.IgnoreQueryFilters().AnyAsync(x => x.VehicleId == id && x.Status != ShipmentStatus.Completed);
        if (hasActiveShipment)
        {
            return ServiceResult<bool>.BadRequest("Aktif sevkiyatı bulunan araç silinemez.");
        }

        vehicle.IsDeleted = true;
        vehicle.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<AdminVehicleResponse>> RestoreAsync(Guid id)
    {
        var vehicle = await db.Vehicles.IgnoreQueryFilters().Include(x => x.Shipments).FirstOrDefaultAsync(x => x.Id == id);
        if (vehicle == null)
        {
            return ServiceResult<AdminVehicleResponse>.NotFound("Araç bulunamadı.");
        }
        if (!vehicle.IsDeleted)
        {
            return ServiceResult<AdminVehicleResponse>.Ok(ToResponse(vehicle, null));
        }

        var duplicatePlate = await db.Vehicles.IgnoreQueryFilters().AnyAsync(x =>
            x.Id != id && !x.IsDeleted && x.PlateNumber == vehicle.PlateNumber);
        if (duplicatePlate)
        {
            return ServiceResult<AdminVehicleResponse>.BadRequest("Bu plaka zaten kayıtlı.");
        }

        vehicle.IsDeleted = false;
        vehicle.DeletedAt = null;
        await db.SaveChangesAsync();
        return ServiceResult<AdminVehicleResponse>.Ok(ToResponse(vehicle, vehicle.Shipments.Where(x => x.Status != ShipmentStatus.Completed).OrderByDescending(x => x.CreatedAt).FirstOrDefault()));
    }

    private async Task<string?> ValidateAsync(AdminVehicleRequest request, Guid? excludedVehicleId)
    {
        if (string.IsNullOrWhiteSpace(request.PlateNumber))
        {
            return "Plaka zorunludur.";
        }

        var plate = ContactValidation.NormalizePlate(request.PlateNumber);
        if (!ContactValidation.IsValidPlate(plate))
        {
            return ContactValidation.InvalidPlateMessage;
        }

        var duplicatePlate = await db.Vehicles.IgnoreQueryFilters().AnyAsync(x => x.Id != excludedVehicleId && x.PlateNumber == plate);
        if (duplicatePlate)
        {
            return "Bu plaka zaten kayıtlı.";
        }

        var driver = await db.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
        if (driver == null || driver.Role != "Driver")
        {
            return "Araç geçerli bir Driver kullanıcısına bağlanmalıdır.";
        }

        return null;
    }

    private static AdminVehicleResponse ToResponse(Vehicle vehicle, Shipment? currentShipment) =>
        new(
            vehicle.Id,
            vehicle.PlateNumber,
            vehicle.UserId,
            vehicle.DriverName,
            vehicle.CreatedAt,
            currentShipment?.Id,
            currentShipment == null ? null : (int)currentShipment.Status,
            currentShipment == null ? null : StatusText(currentShipment.Status),
            vehicle.IsDeleted,
            vehicle.DeletedAt);

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
