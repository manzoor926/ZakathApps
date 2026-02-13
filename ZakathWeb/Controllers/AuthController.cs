using Microsoft.AspNetCore.Mvc;
using ZakathApp.Web.Models;
using ZakathApp.Web.Services;

namespace ZakathApp.Web.Controllers;

public class AuthController : Controller
{
    private readonly IApiClient _apiClient;

    public AuthController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // If already logged in, redirect to dashboard
        var token = HttpContext.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var response = await _apiClient.LoginAsync(request);

        if (response.Success && response.Token != null)
        {
            HttpContext.Session.SetString("AuthToken", response.Token);
            HttpContext.Session.SetString("UserName", response.User?.FullName ?? "User");
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = response.Message ?? "Login failed";
        return View(request);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        if (request.Password != request.ConfirmPassword)
        {
            ViewBag.Error = "Passwords do not match";
            return View(request);
        }

        try
        {
            var response = await _apiClient.RegisterAsync(request);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = $"Registration failed: {response.Message ?? "Unknown error"}";
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Error: {ex.Message}";
        }

        return View(request);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}