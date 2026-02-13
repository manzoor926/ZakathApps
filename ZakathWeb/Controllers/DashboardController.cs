using Microsoft.AspNetCore.Mvc;
using ZakathApp.Web.Services;

namespace ZakathApp.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IApiClient _apiClient;

    public DashboardController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private string? GetToken() => HttpContext.Session.GetString("AuthToken");

    private IActionResult CheckAuth()
    {
        if (string.IsNullOrEmpty(GetToken()))
        {
            return RedirectToAction("Login", "Auth");
        }
        return null!;
    }

    public async Task<IActionResult> Index()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;

        var token = GetToken()!;
        
        var statsResponse = await _apiClient.GetDashboardStatsAsync(token);
        var transactionsResponse = await _apiClient.GetRecentTransactionsAsync(token);

        ViewBag.Stats = statsResponse.Data;
        ViewBag.Transactions = transactionsResponse.Data ?? new List<Models.RecentTransaction>();
        ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "User";

        return View();
    }
}
