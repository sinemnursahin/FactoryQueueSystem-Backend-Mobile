using FactoryQueueSystem.Api.DTOs.Profile;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize(Roles = "Driver")]
public class ProfileController(ProfileService profileService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => ToActionResult(await profileService.GetAsync(User));

    [HttpPut]
    public async Task<IActionResult> Update(ProfileUpdateRequest request) => ToActionResult(await profileService.UpdateAsync(User, request));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
