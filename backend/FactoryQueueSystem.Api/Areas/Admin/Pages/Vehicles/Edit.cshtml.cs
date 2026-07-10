using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages.Vehicles;

public class EditModel : VehicleFormPageModel
{
    private readonly AdminVehicleService _adminVehicleService;

    public EditModel(AdminVehicleService adminVehicleService, AdminUserService adminUserService)
        : base(adminVehicleService, adminUserService)
    {
        _adminVehicleService = adminVehicleService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var result = await _adminVehicleService.GetAsync(id);
        if (!result.Succeeded || result.Value == null)
        {
            return NotFound();
        }

        Input = VehicleInput.From(result.Value);
        await LoadDriversAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id) => await SaveAsync(id);
}
