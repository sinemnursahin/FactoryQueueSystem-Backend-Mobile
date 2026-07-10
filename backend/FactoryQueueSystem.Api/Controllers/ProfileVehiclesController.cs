using FactoryQueueSystem.Api.DTOs.Profile;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/profile/vehicles")]
[Authorize(Roles = "Driver")]
public class ProfileVehiclesController(ProfileVehicleService profileVehicleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> All() => Ok(await profileVehicleService.GetVehiclesAsync(User));

    [HttpPost]
    public async Task<IActionResult> Create(ProfileVehicleRequest request) => ToActionResult(await profileVehicleService.CreateAsync(User, request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ProfileVehicleRequest request) => ToActionResult(await profileVehicleService.UpdateAsync(User, id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) => ToActionResult(await profileVehicleService.DeleteAsync(User, id));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
