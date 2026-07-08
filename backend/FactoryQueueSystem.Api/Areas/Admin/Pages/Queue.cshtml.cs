using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages;

public class QueueModel(AdminShipmentService adminShipmentService) : PageModel
{
    public List<AdminShipmentResponse> Items { get; set; } = [];
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Items = await adminShipmentService.GetQueueAsync();
    }

    public async Task<IActionResult> OnPostCallToScaleAsync(Guid id)
    {
        var result = await adminShipmentService.CallToScaleAsync(id);
        if (!result.Succeeded)
        {
            ErrorMessage = result.Error;
            Items = await adminShipmentService.GetQueueAsync();
            return Page();
        }

        SuccessMessage = "Araç kantara çağrıldı.";
        return RedirectToPage();
    }
}
