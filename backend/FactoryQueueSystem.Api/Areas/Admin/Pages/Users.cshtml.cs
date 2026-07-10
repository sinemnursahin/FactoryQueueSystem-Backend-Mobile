using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages;

public class UsersModel(AdminUserService adminUserService) : PageModel
{
    public List<AdminUserResponse> Items { get; set; } = [];
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ShowDeleted { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Items = await adminUserService.GetAllAsync(ShowDeleted);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var result = await adminUserService.DeleteAsync(id, User);
        if (result.Succeeded)
        {
            SuccessMessage = "Kayıt sistemden kaldırıldı. Geçmiş veriler korunmaktadır.";
            return RedirectToPage(new { showDeleted = ShowDeleted });
        }

        ErrorMessage = result.Error;
        Items = await adminUserService.GetAllAsync(ShowDeleted);
        return Page();
    }

    public async Task<IActionResult> OnPostRestoreAsync(Guid id)
    {
        var result = await adminUserService.RestoreAsync(id);
        if (result.Succeeded)
        {
            SuccessMessage = "Kayıt geri yüklendi.";
            return RedirectToPage(new { showDeleted = true });
        }

        ErrorMessage = result.Error;
        ShowDeleted = true;
        Items = await adminUserService.GetAllAsync(true);
        return Page();
    }
}
