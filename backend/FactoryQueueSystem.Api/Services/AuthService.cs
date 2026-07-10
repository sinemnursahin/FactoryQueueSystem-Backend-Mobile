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
        var email = ContactValidation.NormalizeEmail(request.Email);
        var phone = ContactValidation.NormalizePhone(request.PhoneNumber);

        if (email == null && phone == null)
        {
            return ServiceResult<AuthResponse>.BadRequest(ContactValidation.MissingContactMessage);
        }

        if (email != null && !ContactValidation.IsValidEmail(email))
        {
            return ServiceResult<AuthResponse>.BadRequest(ContactValidation.InvalidEmailMessage);
        }

        if (phone != null && !ContactValidation.IsValidPhone(phone))
        {
            return ServiceResult<AuthResponse>.BadRequest(ContactValidation.InvalidPhoneMessage);
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return ServiceResult<AuthResponse>.BadRequest("Sifre en az 6 karakter olmalidir.");
        }

        var exists = await db.Users.IgnoreQueryFilters().AnyAsync(x =>
            (email != null && x.Email == email) ||
            (phone != null && x.PhoneNumber == phone));

        if (exists)
        {
            return ServiceResult<AuthResponse>.BadRequest("Bu e-posta veya telefon ile kayitli kullanici var.");
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
        db.Shipments.Add(new Shipment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Status = ShipmentStatus.OnTheWay,
            RawMaterialName = "Demo Hammadde",
            SupplierName = "Demo Tedarikci",
            CreatedAt = user.CreatedAt
        });
        await db.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(ToAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var login = ContactValidation.NormalizeEmail(request.EmailOrPhone);
        if (login != null && login.Contains('@') && !ContactValidation.IsValidEmail(login))
        {
            return ServiceResult<AuthResponse>.BadRequest(ContactValidation.InvalidEmailMessage);
        }

        if (login != null && !login.Contains('@'))
        {
            login = ContactValidation.NormalizePhone(request.EmailOrPhone);
        }

        if (login == null)
        {
            return ServiceResult<AuthResponse>.Unauthorized("Giris bilgileri hatali.");
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == login || x.PhoneNumber == login);
        if (user == null)
        {
            return ServiceResult<AuthResponse>.Unauthorized("Giris bilgileri hatali.");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return ServiceResult<AuthResponse>.Unauthorized("Giris bilgileri hatali.");
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
}
