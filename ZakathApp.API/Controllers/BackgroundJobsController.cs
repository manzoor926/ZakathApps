using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZakathApp.API.Data;
using ZakathApp.API.Models;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    /// <summary>
    /// Background-job endpoints.  In production call these on a cron schedule
    /// (e.g. every 6 hours).  They are [Authorize] so only authenticated users
    /// trigger their own data; a dedicated admin/cron token can be added later.
    /// </summary>
    [Route("api/Jobs")]
    [ApiController]
    [Authorize]
    public class BackgroundJobsController : ControllerBase
    {
        private readonly ZakathDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IZakathCalculationService _zakathService;

        public BackgroundJobsController(
            ZakathDbContext context,
            INotificationService notificationService,
            IZakathCalculationService zakathService)
        {
            _context              = context;
            _notificationService  = notificationService;
            _zakathService        = zakathService;
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/Jobs/check-hawl-reminders
        // Looks at the user's last calculation date.
        // If it is more than 324 days ago (30 days before next hawl)
        // a reminder notification is created.
        // ──────────────────────────────────────────────────────────
        [HttpPost("check-hawl-reminders")]
        public async Task<IActionResult> CheckHawlReminders()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            int reminderDays = user.ReminderDaysBefore > 0 ? user.ReminderDaysBefore : 30;
            var lastCalc = await _context.ZakathCalculations
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.CalculationDate)
                .FirstOrDefaultAsync();

            if (lastCalc == null)
            {
                // Never calculated — send a first-time reminder
                await _notificationService.CreateNotificationAsync(userId,
                    "First Zakath Calculation",
                    "You have not yet calculated your Zakath. Please add your assets and calculate now.",
                    "Reminder");

                return Ok(new { Success = true, Message = "First-time reminder sent" });
            }

            var daysSinceCalc = (DateTime.Now - lastCalc.CalculationDate).Days;
            var hawlDays      = 354;                          // Islamic year
            var daysUntilNext = hawlDays - daysSinceCalc;

            if (daysUntilNext <= reminderDays && daysUntilNext > 0)
            {
                // Check we haven't already sent one in the last 7 days
                var recentReminder = await _context.Notifications
                    .Where(n => n.UserID == userId
                              && n.NotificationType == "HawlReminder"
                              && n.CreatedDate >= DateTime.Now.AddDays(-7))
                    .FirstOrDefaultAsync();

                if (recentReminder == null)
                {
                    await _notificationService.CreateNotificationAsync(userId,
                        "Zakath Hawl Reminder",
                        $"Your next Zakath calculation is due in {daysUntilNext} days. Please review your assets.",
                        "HawlReminder");

                    return Ok(new { Success = true, Message = $"Hawl reminder sent. Due in {daysUntilNext} days" });
                }

                return Ok(new { Success = true, Message = "Reminder already sent this week" });
            }

            return Ok(new { Success = true, Message = $"No reminder needed. Next hawl in {daysUntilNext} days" });
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/Jobs/auto-calculate
        // Re-runs the full Zakath calculation for the current user
        // using their preferred madhab and primary currency.
        // ──────────────────────────────────────────────────────────
        [HttpPost("auto-calculate")]
        public async Task<IActionResult> AutoCalculate()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            // resolve preferred madhab  (default 1)
            var user = await _context.Users.FindAsync(userId);
            int madhabId = user?.PreferredMadhabID ?? 1;

            // resolve primary currency  (default USD)
            var primaryCurr = await _context.UserCurrencies
                .Include(uc => uc.Currency)
                .FirstOrDefaultAsync(uc => uc.UserID == userId && uc.IsPrimary);
            string baseCurrency = primaryCurr?.Currency?.CurrencyCode ?? "USD";

            var result = await _zakathService.CalculateZakathAsync(userId, baseCurrency, madhabId);

            return Ok(new
            {
                Success = true,
                Message = "Auto-calculation complete",
                Data    = new
                {
                    result.CalculationID,
                    result.CalculationDate,
                    result.ZakathAmount,
                    result.IsZakathDue,
                    result.Currency
                }
            });
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/Jobs/refresh-exchange-rates
        // Forces a refresh of all cached exchange rates
        // ──────────────────────────────────────────────────────────
        [HttpPost("refresh-exchange-rates")]
        public async Task<IActionResult> RefreshRates([FromServices] ICurrencyService currencyService)
        {
            var result = await currencyService.UpdateExchangeRatesAsync();
            return Ok(new { Success = result.Success, Message = result.Message });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Jobs/status
        // Quick status: last calculation date, last reminder, etc.
        // ──────────────────────────────────────────────────────────
        [HttpGet("status")]
        public async Task<IActionResult> GetJobStatus()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var lastCalc = await _context.ZakathCalculations
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.CalculationDate)
                .FirstOrDefaultAsync();

            var lastReminder = await _context.Notifications
                .Where(n => n.UserID == userId && n.NotificationType == "HawlReminder")
                .OrderByDescending(n => n.CreatedDate)
                .FirstOrDefaultAsync();

            var lastExchangeUpdate = await _context.CurrencyExchangeRates
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();

            int daysSinceCalc  = lastCalc != null ? (DateTime.Now - lastCalc.CalculationDate).Days : -1;
            int daysUntilHawl  = lastCalc != null ? 354 - daysSinceCalc : -1;

            return Ok(new
            {
                Success = true,
                Message = "Job status",
                Data    = new
                {
                    LastCalculationDate   = lastCalc?.CalculationDate,
                    DaysSinceCalculation  = daysSinceCalc,
                    DaysUntilNextHawl     = daysUntilHawl,
                    LastReminderDate      = lastReminder?.CreatedDate,
                    LastExchangeRateDate  = lastExchangeUpdate?.EffectiveDate
                }
            });
        }
    }
}
