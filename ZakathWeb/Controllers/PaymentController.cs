using Microsoft.AspNetCore.Mvc;
using ZakathApp.Web.Models;
using ZakathApp.Web.Services;

namespace ZakathApp.Web.Controllers;

public class PaymentController : Controller
{
    private readonly IApiClient _apiClient;
    public PaymentController(IApiClient apiClient) => _apiClient = apiClient;
    private string? GetToken() => HttpContext.Session.GetString("AuthToken");
    private IActionResult CheckAuth() => string.IsNullOrEmpty(GetToken()) ? RedirectToAction("Login", "Auth") : null!;

    public async Task<IActionResult> Index()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        var response = await _apiClient.GetPaymentsAsync(GetToken()!);
        return View(response.Data ?? new List<PaymentModel>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(PaymentRequest request)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        if (!ModelState.IsValid) return View(request);
        var response = await _apiClient.CreatePaymentAsync(request, GetToken()!);
        if (response.Success)
        {
            TempData["SuccessMessage"] = "Payment recorded successfully";
            return RedirectToAction("Index");
        }
        ViewBag.Error = response.Message;
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        await _apiClient.DeletePaymentAsync(id, GetToken()!);
        return RedirectToAction("Index");
    }
}
