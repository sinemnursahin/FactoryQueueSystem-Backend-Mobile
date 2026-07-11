using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactoryQueueSystem.Api.Areas.Admin.Pages;

public class DashboardModel(AdminDashboardService adminDashboardService) : PageModel
{
    public AdminDashboardResponse Dashboard { get; set; } = null!;

    public async Task OnGetAsync()
    {
        Dashboard = await adminDashboardService.GetAsync();
    }
}
