using FactoryQueueSystem.Api.Services;
using FactoryQueueSystem.Api.DTOs.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize(Roles = "Driver")]
public class ShipmentsController(ShipmentService shipmentService) : ControllerBase
{
    [HttpGet("active")]
    public async Task<IActionResult> Active() => ToActionResult(await shipmentService.GetActiveAsync(User));

    [HttpPost("{id:guid}/queue")]
    public async Task<IActionResult> Queue(Guid id) => ToActionResult(await shipmentService.QueueAsync(id, User));

    [HttpPost("{id:guid}/assign-vehicle")]
    public async Task<IActionResult> AssignVehicle(Guid id, AssignShipmentVehicleRequest request) => ToActionResult(await shipmentService.AssignVehicleAsync(id, User, request));

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> Status(Guid id) => ToActionResult(await shipmentService.GetStatusAsync(id, User));

    [HttpGet("{id:guid}/result")]
    public async Task<IActionResult> Result(Guid id) => ToActionResult(await shipmentService.GetResultAsync(id, User));

    [HttpPost("{id:guid}/exit-facility")]
    public async Task<IActionResult> ExitFacility(Guid id) => ToActionResult(await shipmentService.ExitFacilityAsync(id, User));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
