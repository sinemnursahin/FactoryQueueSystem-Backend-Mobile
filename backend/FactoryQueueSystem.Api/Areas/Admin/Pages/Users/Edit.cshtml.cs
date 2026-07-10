using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages.Users;

public class EditModel(AdminUserService adminUserService) : PageModel
{
    [BindProperty]
    public UserInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var result = await adminUserService.GetAsync(id);
        if (!result.Succeeded || result.Value == null)
        {
            return NotFound();
        }

        Input = UserInput.From(result.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var result = await adminUserService.UpdateAsync(id, new AdminUserUpdateRequest(Input.FirstName, Input.LastName, Input.Email, Input.PhoneNumber, Input.Role));
        if (result.Succeeded)
        {
            SuccessMessage = "Kullanıcı güncellendi.";
            return RedirectToPage("/Users");
        }

        ErrorMessage = result.Error;
        ModelState.AddModelError(string.Empty, result.Error ?? "Kullanıcı güncellenemedi.");
        return Page();
    }

    public class UserInput
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "Driver";

        public static UserInput From(AdminUserResponse user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = ContactValidation.ToPhoneLocalPart(user.PhoneNumber),
            Role = user.Role
        };
    }
}
