using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/admin/vehicles")]
[Authorize(Roles = "Admin")]
public class AdminVehiclesController(AdminVehicleService adminVehicleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> All() => Ok(await adminVehicleService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) => ToActionResult(await adminVehicleService.GetAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(AdminVehicleRequest request) => ToActionResult(await adminVehicleService.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AdminVehicleRequest request) => ToActionResult(await adminVehicleService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) => ToActionResult(await adminVehicleService.DeleteAsync(id));

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id) => ToActionResult(await adminVehicleService.RestoreAsync(id));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
