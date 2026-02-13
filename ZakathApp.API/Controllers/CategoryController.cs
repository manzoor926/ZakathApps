using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZakathApp.API.Data;
using ZakathApp.API.Models;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    // ========================================
    // CATEGORY CONTROLLER  (Income / Expense categories)
    // ========================================
    [Route("api/Category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ZakathDbContext _context;

        public CategoryController(ZakathDbContext context)
        {
            _context = context;
        }

        // GET /api/Category
        // Returns every active category grouped by type
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.CategoryMasters
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryType)
                .ThenBy(c => c.CategoryID)
                .ToListAsync();

            var grouped = categories
                .GroupBy(c => c.CategoryType ?? "Other")
                .Select(g => new
                {
                    Type       = g.Key,
                    Categories = g.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryNameEnglish,
                        c.CategoryNameArabic,
                        c.IconName,
                        c.ColorCode
                    }).ToList()
                });

            return Ok(new { Success = true, Message = "Categories retrieved", Data = grouped });
        }

        // GET /api/Category/{type}   e.g. "Income" or "Expense"
        [HttpGet("{type}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByType(string type)
        {
            var list = await _context.CategoryMasters
                .Where(c => c.IsActive && c.CategoryType == type)
                .OrderBy(c => c.CategoryID)
                .Select(c => new
                {
                    c.CategoryID,
                    c.CategoryNameEnglish,
                    c.CategoryNameArabic,
                    c.IconName,
                    c.ColorCode
                })
                .ToListAsync();

            return Ok(new { Success = true, Message = $"{type} categories retrieved", Data = list });
        }

        // GET /api/Category/id/{id}
        [HttpGet("id/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _context.CategoryMasters.FindAsync(id);
            if (c == null)
                return NotFound(new { Success = false, Message = "Category not found" });

            return Ok(new { Success = true, Message = "Category retrieved", Data = c });
        }
    }

    // ========================================
    // ASSET CATEGORY CONTROLLER
    // ========================================
    [Route("api/AssetCategory")]
    [ApiController]
    public class AssetCategoryController : ControllerBase
    {
        private readonly ZakathDbContext _context;

        public AssetCategoryController(ZakathDbContext context)
        {
            _context = context;
        }

        // GET /api/AssetCategory
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.AssetCategoryMasters
                .Where(c => c.IsActive)
                .OrderBy(c => c.AssetCategoryID)
                .Select(c => new
                {
                    c.AssetCategoryID,
                    c.CategoryNameEnglish,
                    c.CategoryNameArabic,
                    c.ZakathApplicable,
                    c.IsValuationRequired,
                    c.IconName,
                    c.Description
                })
                .ToListAsync();

            return Ok(new { Success = true, Message = "Asset categories retrieved", Data = list });
        }

        // GET /api/AssetCategory/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _context.AssetCategoryMasters.FindAsync(id);
            if (c == null)
                return NotFound(new { Success = false, Message = "Asset category not found" });

            return Ok(new { Success = true, Message = "Asset category retrieved", Data = c });
        }

        // GET /api/AssetCategory/{id}/items   — master items under this category
        [HttpGet("{id}/items")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItemsByCategory(int id)
        {
            var items = await _context.ItemMasters
                .Where(i => i.AssetCategoryID == id && i.IsActive)
                .Select(i => new
                {
                    i.ItemID,
                    i.ItemNameEnglish,
                    i.ItemNameArabic,
                    i.MeasurementUnit,
                    i.ZakathPercentage
                })
                .ToListAsync();

            return Ok(new { Success = true, Message = "Items retrieved", Data = items });
        }
    }

    // ========================================
    // NOTIFICATION CONTROLLER
    // ========================================
    [Route("api/Notification")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        // GET /api/Notification
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var result = await _service.GetUserNotificationsAsync(userId);
            return Ok(new { Success = true, Message = "Notifications retrieved", Data = result.Data });
        }

        // PUT /api/Notification/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var result = await _service.MarkAsReadAsync(id);
            return Ok(new { Success = result.Success, Message = result.Message });
        }

        // DELETE /api/Notification/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteNotificationAsync(id);
            return Ok(new { Success = result.Success, Message = result.Message });
        }
    }
}
