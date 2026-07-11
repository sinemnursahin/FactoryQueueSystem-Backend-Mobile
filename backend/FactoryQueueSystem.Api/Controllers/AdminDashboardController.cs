using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController(AdminDashboardService adminDashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await adminDashboardService.GetAsync());
}
