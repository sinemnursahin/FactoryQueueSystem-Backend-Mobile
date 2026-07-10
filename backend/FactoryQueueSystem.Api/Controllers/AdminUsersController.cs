using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController(AdminUserService adminUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> All() => Ok(await adminUserService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) => ToActionResult(await adminUserService.GetAsync(id));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AdminUserUpdateRequest request) => ToActionResult(await adminUserService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) => ToActionResult(await adminUserService.DeleteAsync(id, User));

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id) => ToActionResult(await adminUserService.RestoreAsync(id));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
