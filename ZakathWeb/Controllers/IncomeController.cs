using Microsoft.AspNetCore.Mvc;
using ZakathApp.Web.Models;
using ZakathApp.Web.Services;

namespace ZakathApp.Web.Controllers;

public class IncomeController : Controller
{
    private readonly IApiClient _apiClient;

    public IncomeController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private string? GetToken() => HttpContext.Session.GetString("AuthToken");
    private IActionResult CheckAuth() => string.IsNullOrEmpty(GetToken()) ? RedirectToAction("Login", "Auth") : null!;

    public async Task<IActionResult> Index()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;

        var response = await _apiClient.GetIncomesAsync(GetToken()!);
        return View(response.Data ?? new List<IncomeModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;

        var categoriesResponse = await _apiClient.GetCategoriesByTypeAsync("Income");
        ViewBag.Categories = categoriesResponse.Data ?? new List<CategoryModel>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(IncomeRequest request)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;

        if (!ModelState.IsValid)
        {
            var categoriesResponse = await _apiClient.GetCategoriesByTypeAsync("Income");
            ViewBag.Categories = categoriesResponse.Data ?? new List<CategoryModel>();
            return View(request);
        }

        var response = await _apiClient.CreateIncomeAsync(request, GetToken()!);
        
        if (response.Success)
        {
            TempData["SuccessMessage"] = "Income added successfully";
            return RedirectToAction("Index");
        }

        ViewBag.Error = response.Message;
        var cats = await _apiClient.GetCategoriesByTypeAsync("Income");
        ViewBag.Categories = cats.Data ?? new List<CategoryModel>();
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;

        await _apiClient.DeleteIncomeAsync(id, GetToken()!);
        return RedirectToAction("Index");
    }
}
