using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Entities;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminShipmentsController(AdminShipmentService adminShipmentService) : ControllerBase
{
    [HttpGet("vehicles/queue")]
    public async Task<IActionResult> Queue() => Ok(await adminShipmentService.GetQueueAsync());

    [HttpGet("shipments/status/{status:int}")]
    public async Task<IActionResult> ByStatus(int status) =>
        Enum.IsDefined(typeof(ShipmentStatus), status)
            ? Ok(await adminShipmentService.GetByStatusAsync((ShipmentStatus)status))
            : BadRequest(new { message = "Geçersiz durum." });

    [HttpGet("shipments/completed")]
    public async Task<IActionResult> Completed() => Ok(await adminShipmentService.GetCompletedAsync());

    [HttpPost("shipments/{id:guid}/call-to-scale")]
    public async Task<IActionResult> CallToScale(Guid id) => ToActionResult(await adminShipmentService.CallToScaleAsync(id));

    [HttpPost("shipments/{id:guid}/loaded-weight")]
    public async Task<IActionResult> LoadedWeight(Guid id, WeightRequest request) => ToActionResult(await adminShipmentService.SetLoadedWeightAsync(id, request.Weight));

    [HttpPost("shipments/{id:guid}/start-unloading")]
    public async Task<IActionResult> StartUnloading(Guid id) => ToActionResult(await adminShipmentService.StartUnloadingAsync(id));

    [HttpPost("shipments/{id:guid}/complete-unloading")]
    public async Task<IActionResult> CompleteUnloading(Guid id) => ToActionResult(await adminShipmentService.CompleteUnloadingAsync(id));

    [HttpPost("shipments/{id:guid}/empty-weight")]
    public async Task<IActionResult> EmptyWeight(Guid id, WeightRequest request) => ToActionResult(await adminShipmentService.SetEmptyWeightAsync(id, request.Weight));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
