using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages.Vehicles;

public class CreateModel : VehicleFormPageModel
{
    public CreateModel(AdminVehicleService adminVehicleService, AdminUserService adminUserService)
        : base(adminVehicleService, adminUserService)
    {
    }

    public async Task OnGetAsync()
    {
        await LoadDriversAsync();
    }

    public async Task<IActionResult> OnPostAsync() => await SaveAsync(null);
}
