using System.Security.Claims;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages;

[AllowAnonymous]
public class LoginModel(AuthService authService) : PageModel
{
    [BindProperty]
    public string EmailOrPhone { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var admin = await authService.ValidateAdminAsync(EmailOrPhone, Password);
        if (admin == null)
        {
            ErrorMessage = "Admin bilgileri hatalı.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new(ClaimTypes.Name, $"{admin.FirstName} {admin.LastName}".Trim()),
            new(ClaimTypes.Role, admin.Role)
        };

        if (!string.IsNullOrWhiteSpace(admin.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, admin.Email));
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        return RedirectToPage("/Dashboard", new { area = "Admin" });
    }
}
