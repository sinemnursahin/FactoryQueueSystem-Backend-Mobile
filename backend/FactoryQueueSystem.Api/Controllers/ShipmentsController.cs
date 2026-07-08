using FactoryQueueSystem.Api.Services;
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

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> Status(Guid id) => ToActionResult(await shipmentService.GetStatusAsync(id, User));

    [HttpGet("{id:guid}/result")]
    public async Task<IActionResult> Result(Guid id) => ToActionResult(await shipmentService.GetResultAsync(id, User));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
