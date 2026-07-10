using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages.Vehicles;

public abstract class VehicleFormPageModel : PageModel
{
    private readonly AdminVehicleService _adminVehicleService;
    private readonly AdminUserService _adminUserService;

    protected VehicleFormPageModel(AdminVehicleService adminVehicleService, AdminUserService adminUserService)
    {
        _adminVehicleService = adminVehicleService;
        _adminUserService = adminUserService;
    }

    [BindProperty]
    public VehicleInput Input { get; set; } = new();

    public List<SelectListItem> DriverOptions { get; set; } = [];
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    protected async Task LoadDriversAsync()
    {
        DriverOptions = (await _adminUserService.GetAllAsync())
            .Where(x => x.Role == "Driver")
            .Select(x => new SelectListItem($"{x.FirstName} {x.LastName} ({x.Email ?? x.PhoneNumber})", x.Id.ToString()))
            .ToList();
    }

    protected async Task<IActionResult> SaveAsync(Guid? id)
    {
        var request = new AdminVehicleRequest(Input.PlateNumber, Input.UserId);
        var result = id == null
            ? await _adminVehicleService.CreateAsync(request)
            : await _adminVehicleService.UpdateAsync(id.Value, request);

        if (result.Succeeded)
        {
            SuccessMessage = id == null ? "Araç oluşturuldu." : "Araç güncellendi.";
            return RedirectToPage("/Vehicles");
        }

        ErrorMessage = result.Error;
        ModelState.AddModelError(string.Empty, result.Error ?? "Araç kaydedilemedi.");
        await LoadDriversAsync();
        return Page();
    }

    public class VehicleInput
    {
        public Guid Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        public static VehicleInput From(AdminVehicleResponse vehicle) => new()
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber,
            UserId = vehicle.UserId
        };
    }
}
