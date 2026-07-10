using System.Security.Claims;
using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Profile;
using FactoryQueueSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class ProfileVehicleService(AppDbContext db, FactoryClock clock)
{
    public async Task<List<ProfileVehicleResponse>> GetVehiclesAsync(ClaimsPrincipal currentUser)
    {
        var userId = GetUserId(currentUser);
        if (userId == null)
        {
            return [];
        }

        return await db.Vehicles
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.PlateNumber)
            .Select(x => new ProfileVehicleResponse(x.Id, x.PlateNumber))
            .ToListAsync();
    }

    public async Task<ServiceResult<ProfileVehicleResponse>> CreateAsync(ClaimsPrincipal currentUser, ProfileVehicleRequest request)
    {
        var userId = GetUserId(currentUser);
        if (userId == null)
        {
            return ServiceResult<ProfileVehicleResponse>.Unauthorized();
        }

        var driver = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == "Driver");
        if (driver == null)
        {
            return ServiceResult<ProfileVehicleResponse>.Unauthorized();
        }

        var result = await GetOrCreateOwnedVehicleAsync(driver, request.PlateNumber);
        return result.Succeeded && result.Value != null
            ? ServiceResult<ProfileVehicleResponse>.Ok(ToResponse(result.Value))
            : ServiceResult<ProfileVehicleResponse>.BadRequest(result.Error ?? "Araç oluşturulamadı.");
    }

    public async Task<ServiceResult<ProfileVehicleResponse>> UpdateAsync(ClaimsPrincipal currentUser, Guid id, ProfileVehicleRequest request)
    {
        var userId = GetUserId(currentUser);
        if (userId == null)
        {
            return ServiceResult<ProfileVehicleResponse>.Unauthorized();
        }

        var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (vehicle == null)
        {
            return ServiceResult<ProfileVehicleResponse>.NotFound("Araç bulunamadı.");
        }

        var plate = ContactValidation.NormalizePlate(request.PlateNumber);
        if (plate == null)
        {
            return ServiceResult<ProfileVehicleResponse>.BadRequest("Plaka zorunludur.");
        }
        if (!ContactValidation.IsValidPlate(plate))
        {
            return ServiceResult<ProfileVehicleResponse>.BadRequest(ContactValidation.InvalidPlateMessage);
        }

        var duplicate = await db.Vehicles.IgnoreQueryFilters().AnyAsync(x => x.Id != id && x.PlateNumber == plate);
        if (duplicate)
        {
            return ServiceResult<ProfileVehicleResponse>.BadRequest("Bu plaka başka bir kullanıcıya kayıtlı.");
        }

        vehicle.PlateNumber = plate;
        await db.SaveChangesAsync();
        return ServiceResult<ProfileVehicleResponse>.Ok(ToResponse(vehicle));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(ClaimsPrincipal currentUser, Guid id)
    {
        var userId = GetUserId(currentUser);
        if (userId == null)
        {
            return ServiceResult<bool>.Unauthorized();
        }

        var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (vehicle == null)
        {
            return ServiceResult<bool>.NotFound("Araç bulunamadı.");
        }

        var hasActiveShipment = await db.Shipments.AnyAsync(x => x.VehicleId == id && x.Status != ShipmentStatus.Completed);
        if (hasActiveShipment)
        {
            return ServiceResult<bool>.BadRequest("Aktif sevkiyatı bulunan araç silinemez.");
        }

        vehicle.IsDeleted = true;
        vehicle.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<Vehicle>> GetOrCreateOwnedVehicleAsync(User driver, string? plateNumber)
    {
        var plate = ContactValidation.NormalizePlate(plateNumber);
        if (plate == null)
        {
            return ServiceResult<Vehicle>.BadRequest("Plaka zorunludur.");
        }
        if (!ContactValidation.IsValidPlate(plate))
        {
            return ServiceResult<Vehicle>.BadRequest(ContactValidation.InvalidPlateMessage);
        }

        var existing = await db.Vehicles.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.PlateNumber == plate);
        if (existing != null)
        {
            return existing.UserId == driver.Id && !existing.IsDeleted
                ? ServiceResult<Vehicle>.Ok(existing)
                : ServiceResult<Vehicle>.BadRequest("Bu plaka başka bir kullanıcıya kayıtlı.");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            PlateNumber = plate,
            UserId = driver.Id,
            DriverName = $"{driver.FirstName} {driver.LastName}".Trim(),
            CreatedAt = clock.UtcNow
        };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        return ServiceResult<Vehicle>.Ok(vehicle);
    }

    private static ProfileVehicleResponse ToResponse(Vehicle vehicle) => new(vehicle.Id, vehicle.PlateNumber);

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
