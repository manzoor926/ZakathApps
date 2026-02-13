using Microsoft.AspNetCore.Mvc;
using ZakathApp.Web.Models;
using ZakathApp.Web.Services;

namespace ZakathApp.Web.Controllers;

public class ExpenseController : Controller
{
    private readonly IApiClient _apiClient;
    public ExpenseController(IApiClient apiClient) => _apiClient = apiClient;
    private string? GetToken() => HttpContext.Session.GetString("AuthToken");
    private IActionResult CheckAuth() => string.IsNullOrEmpty(GetToken()) ? RedirectToAction("Login", "Auth") : null!;

    public async Task<IActionResult> Index()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        var response = await _apiClient.GetExpensesAsync(GetToken()!);
        return View(response.Data ?? new List<ExpenseModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        var categoriesResponse = await _apiClient.GetCategoriesByTypeAsync("Expense");
        ViewBag.Categories = categoriesResponse.Data ?? new List<CategoryModel>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ExpenseRequest request)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        if (!ModelState.IsValid)
        {
            var categoriesResponse = await _apiClient.GetCategoriesByTypeAsync("Expense");
            ViewBag.Categories = categoriesResponse.Data ?? new List<CategoryModel>();
            return View(request);
        }
        var response = await _apiClient.CreateExpenseAsync(request, GetToken()!);
        if (response.Success)
        {
            TempData["SuccessMessage"] = "Expense added successfully";
            return RedirectToAction("Index");
        }
        ViewBag.Error = response.Message;
        var cats = await _apiClient.GetCategoriesByTypeAsync("Expense");
        ViewBag.Categories = cats.Data ?? new List<CategoryModel>();
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        await _apiClient.DeleteExpenseAsync(id, GetToken()!);
        return RedirectToAction("Index");
    }
}
