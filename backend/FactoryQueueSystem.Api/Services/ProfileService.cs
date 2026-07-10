using System.Security.Claims;
using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Profile;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class ProfileService(AppDbContext db, AdminUserService adminUserService)
{
    public async Task<ServiceResult<ProfileResponse>> GetAsync(ClaimsPrincipal currentUser)
    {
        var userId = GetUserId(currentUser);
        if (userId == null)
        {
            return ServiceResult<ProfileResponse>.Unauthorized();
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == "Driver");
        return user == null
            ? ServiceResult<ProfileResponse>.NotFound("Profil bulunamadı.")
            : ServiceResult<ProfileResponse>.Ok(new ProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber));
    }

    public async Task<ServiceResult<ProfileResponse>> UpdateAsync(ClaimsPrincipal currentUser, ProfileUpdateRequest request)
    {
        var userId = GetUserId(currentUser);
        if (userId == null)
        {
            return ServiceResult<ProfileResponse>.Unauthorized();
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == "Driver");
        if (user == null)
        {
            return ServiceResult<ProfileResponse>.NotFound("Profil bulunamadı.");
        }

        var validation = await adminUserService.ValidateUserFieldsAsync(request.FirstName, request.LastName, request.Email, request.PhoneNumber, "Driver", user.Id);
        if (validation != null)
        {
            return ServiceResult<ProfileResponse>.BadRequest(validation);
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = ContactValidation.NormalizeEmail(request.Email);
        user.PhoneNumber = ContactValidation.NormalizePhone(request.PhoneNumber);
        await adminUserService.SyncDriverNameAsync(user.Id, user.FirstName, user.LastName);
        await db.SaveChangesAsync();
        return ServiceResult<ProfileResponse>.Ok(new ProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber));
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
