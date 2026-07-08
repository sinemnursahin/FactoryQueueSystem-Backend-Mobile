using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Auth;
using FactoryQueueSystem.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class AuthService(AppDbContext db, JwtTokenService jwtTokenService)
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<ServiceResult<AuthResponse>> RegisterDriverAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return ServiceResult<AuthResponse>.BadRequest("E-posta veya telefon numarası zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(request.PlateNumber))
        {
            return ServiceResult<AuthResponse>.BadRequest("Plaka zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return ServiceResult<AuthResponse>.BadRequest("Şifre en az 6 karakter olmalıdır.");
        }

        var email = Normalize(request.Email);
        var phone = Normalize(request.PhoneNumber);
        var exists = await db.Users.AnyAsync(x =>
            (email != null && x.Email == email) ||
            (phone != null && x.PhoneNumber == phone));

        if (exists)
        {
            return ServiceResult<AuthResponse>.BadRequest("Bu e-posta veya telefon ile kayıtlı kullanıcı var.");
        }

        var plate = NormalizePlate(request.PlateNumber);
        var plateExists = await db.Vehicles.AnyAsync(x => x.PlateNumber == plate);
        if (plateExists)
        {
            return ServiceResult<AuthResponse>.BadRequest("Bu plaka zaten kayıtlı.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PhoneNumber = phone,
            Role = "Driver",
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            PlateNumber = plate,
            UserId = user.Id,
            DriverName = $"{user.FirstName} {user.LastName}".Trim(),
            CreatedAt = user.CreatedAt
        };
        db.Vehicles.Add(vehicle);
        db.Shipments.Add(new Shipment
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Status = ShipmentStatus.OnTheWay,
            RawMaterialName = "Demo Hammadde",
            SupplierName = "Demo Tedarikçi",
            CreatedAt = user.CreatedAt
        });
        await db.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(ToAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var login = Normalize(request.EmailOrPhone);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == login || x.PhoneNumber == login);
        if (user == null)
        {
            return ServiceResult<AuthResponse>.Unauthorized("Giriş bilgileri hatalı.");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return ServiceResult<AuthResponse>.Unauthorized("Giriş bilgileri hatalı.");
        }

        return ServiceResult<AuthResponse>.Ok(ToAuthResponse(user));
    }

    public async Task<User?> ValidateAdminAsync(string emailOrPhone, string password)
    {
        var result = await LoginAsync(new LoginRequest(emailOrPhone, password));
        return result.Succeeded && result.Value?.Role == "Admin"
            ? await db.Users.FirstAsync(x => x.Id == result.Value.UserId)
            : null;
    }

    private AuthResponse ToAuthResponse(User user) =>
        new(jwtTokenService.CreateToken(user), user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.Role);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string NormalizePlate(string value) => value.Trim().ToUpperInvariant();
}
