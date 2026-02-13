using Microsoft.AspNetCore.Mvc;
using ZakathApp.Web.Models;
using ZakathApp.Web.Services;

namespace ZakathApp.Web.Controllers;

public class ZakathController : Controller
{
    private readonly IApiClient _apiClient;
    public ZakathController(IApiClient apiClient) => _apiClient = apiClient;
    private string? GetToken() => HttpContext.Session.GetString("AuthToken");
    private IActionResult CheckAuth() => string.IsNullOrEmpty(GetToken()) ? RedirectToAction("Login", "Auth") : null!;

    public async Task<IActionResult> Index()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        var historyResponse = await _apiClient.GetZakathHistoryAsync(GetToken()!);
        ViewBag.History = historyResponse.Data ?? new List<ZakathResult>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Calculate()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        var request = new CalculateZakathRequest { CalculationDate = DateTime.Now };
        var response = await _apiClient.CalculateZakathAsync(request, GetToken()!);
        if (response.Success)
        {
            TempData["SuccessMessage"] = "Zakath calculated successfully";
            TempData["ZakathResult"] = Newtonsoft.Json.JsonConvert.SerializeObject(response.Data);
        }
        else
        {
            TempData["ErrorMessage"] = response.Message;
        }
        return RedirectToAction("Index");
    }
}
