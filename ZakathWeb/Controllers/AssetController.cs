using Microsoft.AspNetCore.Mvc;
using ZakathApp.Web.Models;
using ZakathApp.Web.Services;

namespace ZakathApp.Web.Controllers;

public class AssetController : Controller
{
    private readonly IApiClient _apiClient;
    public AssetController(IApiClient apiClient) => _apiClient = apiClient;
    private string? GetToken() => HttpContext.Session.GetString("AuthToken");
    private IActionResult CheckAuth() => string.IsNullOrEmpty(GetToken()) ? RedirectToAction("Login", "Auth") : null!;

    public async Task<IActionResult> Index()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        var response = await _apiClient.GetAssetsAsync(GetToken()!);
        return View(response.Data ?? new List<AssetModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        var categoriesResponse = await _apiClient.GetAssetCategoriesAsync();
        ViewBag.Categories = categoriesResponse.Data ?? new List<AssetCategoryModel>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(AssetRequest request)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        if (!ModelState.IsValid)
        {
            var categoriesResponse = await _apiClient.GetAssetCategoriesAsync();
            ViewBag.Categories = categoriesResponse.Data ?? new List<AssetCategoryModel>();
            return View(request);
        }
        var response = await _apiClient.CreateAssetAsync(request, GetToken()!);
        if (response.Success)
        {
            TempData["SuccessMessage"] = "Asset added successfully";
            return RedirectToAction("Index");
        }
        ViewBag.Error = response.Message;
        var cats = await _apiClient.GetAssetCategoriesAsync();
        ViewBag.Categories = cats.Data ?? new List<AssetCategoryModel>();
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var authCheck = CheckAuth();
        if (authCheck != null) return authCheck;
        await _apiClient.DeleteAssetAsync(id, GetToken()!);
        return RedirectToAction("Index");
    }
}
