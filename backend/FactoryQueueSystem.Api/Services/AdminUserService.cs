using System.Security.Claims;
using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class AdminUserService(AppDbContext db)
{
    public async Task<List<AdminUserResponse>> GetAllAsync(bool includeDeleted = false) =>
        await (includeDeleted ? db.Users.IgnoreQueryFilters() : db.Users)
            .OrderBy(x => x.IsDeleted)
            .ThenBy(x => x.Role)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => ToResponse(x))
            .ToListAsync();

    public async Task<ServiceResult<AdminUserResponse>> GetAsync(Guid id)
    {
        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        return user == null
            ? ServiceResult<AdminUserResponse>.NotFound("Kullanıcı bulunamadı.")
            : ServiceResult<AdminUserResponse>.Ok(ToResponse(user));
    }

    public async Task<ServiceResult<AdminUserResponse>> UpdateAsync(Guid id, AdminUserUpdateRequest request)
    {
        var user = await db.Users.Include(x => x.Vehicles).FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            return ServiceResult<AdminUserResponse>.NotFound("Kullanıcı bulunamadı.");
        }

        var validation = await ValidateUserFieldsAsync(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Role, id);
        if (validation != null)
        {
            return ServiceResult<AdminUserResponse>.BadRequest(validation);
        }

        if (request.Role != "Driver" && user.Vehicles.Count != 0)
        {
            return ServiceResult<AdminUserResponse>.BadRequest("Aracı olan kullanıcı Admin rolüne alınamaz.");
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = ContactValidation.NormalizeEmail(request.Email);
        user.PhoneNumber = ContactValidation.NormalizePhone(request.PhoneNumber);
        user.Role = request.Role;
        await SyncDriverNameAsync(user.Id, user.FirstName, user.LastName);
        await db.SaveChangesAsync();
        return ServiceResult<AdminUserResponse>.Ok(ToResponse(user));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, ClaimsPrincipal currentUser)
    {
        if (GetUserId(currentUser) == id)
        {
            return ServiceResult<bool>.BadRequest("Giriş yapan admin kendisini silemez.");
        }

        var user = await db.Users.IgnoreQueryFilters()
            .Include(x => x.Vehicles)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            return ServiceResult<bool>.NotFound("Kullanıcı bulunamadı.");
        }
        if (user.IsDeleted)
        {
            return ServiceResult<bool>.Ok(true);
        }

        var hasActiveShipment = await db.Shipments.IgnoreQueryFilters().AnyAsync(x =>
            (x.UserId == id || (x.Vehicle != null && x.Vehicle.UserId == id)) &&
            x.Status != ShipmentStatus.Completed);
        if (hasActiveShipment)
        {
            return ServiceResult<bool>.BadRequest("Aktif sevkiyatı bulunan kullanıcı silinemez.");
        }

        var now = DateTime.UtcNow;
        user.IsDeleted = true;
        user.DeletedAt = now;
        foreach (var vehicle in user.Vehicles.Where(x => !x.IsDeleted))
        {
            vehicle.IsDeleted = true;
            vehicle.DeletedAt = now;
        }

        await db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<AdminUserResponse>> RestoreAsync(Guid id)
    {
        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            return ServiceResult<AdminUserResponse>.NotFound("Kullanıcı bulunamadı.");
        }
        if (!user.IsDeleted)
        {
            return ServiceResult<AdminUserResponse>.Ok(ToResponse(user));
        }

        var email = ContactValidation.NormalizeEmail(user.Email);
        var phone = ContactValidation.NormalizePhone(user.PhoneNumber);
        var duplicate = await db.Users.IgnoreQueryFilters().AnyAsync(x =>
            x.Id != id && !x.IsDeleted &&
            ((email != null && x.Email == email) || (phone != null && x.PhoneNumber == phone)));
        if (duplicate)
        {
            return ServiceResult<AdminUserResponse>.BadRequest("Bu e-posta veya telefon başka bir kullanıcıda kayıtlı.");
        }

        user.IsDeleted = false;
        user.DeletedAt = null;
        await db.SaveChangesAsync();
        return ServiceResult<AdminUserResponse>.Ok(ToResponse(user));
    }

    public async Task<string?> ValidateUserFieldsAsync(string firstName, string lastName, string? emailValue, string? phoneValue, string role, Guid? excludedUserId)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return "Ad ve soyad zorunludur.";
        }

        if (role is not ("Driver" or "Admin"))
        {
            return "Rol Driver veya Admin olmalıdır.";
        }

        var email = ContactValidation.NormalizeEmail(emailValue);
        var phone = ContactValidation.NormalizePhone(phoneValue);
        if (email == null && phone == null)
        {
            return ContactValidation.MissingContactMessage;
        }

        if (email != null && !ContactValidation.IsValidEmail(email))
        {
            return ContactValidation.InvalidEmailMessage;
        }

        if (phone != null && !ContactValidation.IsValidPhone(phone))
        {
            return ContactValidation.InvalidPhoneMessage;
        }

        var duplicate = await db.Users.IgnoreQueryFilters().AnyAsync(x =>
            x.Id != excludedUserId &&
            ((email != null && x.Email == email) || (phone != null && x.PhoneNumber == phone)));
        return duplicate ? "Bu e-posta veya telefon başka bir kullanıcıda kayıtlı." : null;
    }

    public async Task SyncDriverNameAsync(Guid userId, string firstName, string lastName)
    {
        var driverName = $"{firstName.Trim()} {lastName.Trim()}".Trim();
        var vehicles = await db.Vehicles.Where(x => x.UserId == userId).ToListAsync();
        foreach (var vehicle in vehicles)
        {
            vehicle.DriverName = driverName;
        }
    }

    private static AdminUserResponse ToResponse(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.Role, user.CreatedAt, user.IsDeleted, user.DeletedAt);

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
