using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages;

public class ShipmentsModel(AdminShipmentService adminShipmentService) : PageModel
{
    public List<AdminShipmentResponse> Items { get; set; } = [];
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Items = await adminShipmentService.GetActiveAsync();
    }

    public async Task<IActionResult> OnPostCallToScaleAsync(Guid id) => await Handle(await adminShipmentService.CallToScaleAsync(id), "Araç kantara çağrıldı.");

    public async Task<IActionResult> OnPostLoadedWeightAsync(Guid id, decimal weight) => await Handle(await adminShipmentService.SetLoadedWeightAsync(id, weight), "Dolu tartım kaydedildi.");

    public async Task<IActionResult> OnPostStartUnloadingAsync(Guid id) => await Handle(await adminShipmentService.StartUnloadingAsync(id), "Boşaltım başlatıldı.");

    public async Task<IActionResult> OnPostCompleteUnloadingAsync(Guid id) => await Handle(await adminShipmentService.CompleteUnloadingAsync(id), "Boşaltım tamamlandı.");

    public async Task<IActionResult> OnPostEmptyWeightAsync(Guid id, decimal weight) => await Handle(await adminShipmentService.SetEmptyWeightAsync(id, weight), "Boş tartım kaydedildi.");

    private async Task<IActionResult> Handle<T>(ServiceResult<T> result, string successMessage)
    {
        if (result.Succeeded)
        {
            SuccessMessage = successMessage;
            return RedirectToPage();
        }

        ErrorMessage = result.Error;
        Items = await adminShipmentService.GetActiveAsync();
        return Page();
    }
}
