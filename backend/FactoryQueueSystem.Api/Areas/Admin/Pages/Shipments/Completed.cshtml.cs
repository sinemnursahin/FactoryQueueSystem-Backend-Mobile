using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages.Shipments;

public class CompletedModel(AdminShipmentService adminShipmentService) : PageModel
{
    public List<AdminShipmentResponse> Items { get; set; } = [];

    public async Task OnGetAsync()
    {
        Items = await adminShipmentService.GetCompletedAsync();
    }
}
