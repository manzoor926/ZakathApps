using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZakathApp.API.Data;
using ZakathApp.API.DTOs;
using ZakathApp.API.Models;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    [Route("api/Dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ZakathDbContext _context;
        private readonly IIncomeService _incomeService;
        private readonly IExpenseService _expenseService;
        private readonly IAssetService _assetService;
        private readonly IZakathCalculationService _zakathService;

        public DashboardController(
            ZakathDbContext context,
            IIncomeService incomeService,
            IExpenseService expenseService,
            IAssetService assetService,
            IZakathCalculationService zakathService)
        {
            _context        = context;
            _incomeService  = incomeService;
            _expenseService = expenseService;
            _assetService   = assetService;
            _zakathService  = zakathService;
        }

        // GET /api/Dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            int userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid token" });

            // date boundaries
            var now        = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart  = new DateTime(now.Year, 1, 1);

            // parallel fetches
            var totalIncome   = await _incomeService.GetTotalIncomeAsync(userId, yearStart, now);
            var totalExpenses = await _expenseService.GetTotalExpensesAsync(userId, yearStart, now);
            var totalAssets   = await _assetService.GetTotalAssetsValueAsync(userId);

            // last calculation (may be null)
            var history = await _zakathService.GetCalculationHistoryAsync(userId);
            var latest  = history.Count > 0 ? history[0] : null;

            // total zakath paid this year
            var zakathPaid = await _context.ZakathPayments
                .Where(p => p.UserID == userId && p.PaymentDate >= yearStart)
                .SumAsync(p => p.PaymentAmount);

            // primary currency
            var primaryCurrency = await _context.UserCurrencies
                .Include(uc => uc.Currency)
                .FirstOrDefaultAsync(uc => uc.UserID == userId && uc.IsPrimary);

            var stats = new DashboardStatsDto
            {
                TotalIncome          = totalIncome,
                TotalExpenses        = totalExpenses,
                TotalAssets          = totalAssets,
                NetWorth             = totalAssets - totalExpenses,
                ZakathDue            = latest?.ZakathAmount ?? 0,
                ZakathPaid           = zakathPaid,
                IsZakathDue          = latest?.IsZakathDue ?? false,
                DaysUntilHawl        = 365,   // simplified; real impl tracks per-user hawl start
                PrimaryCurrency      = primaryCurrency?.Currency?.CurrencyCode ?? "USD",
                LastCalculationDate  = latest?.CalculationDate ?? DateTime.MinValue
            };

            return Ok(new { Success = true, Message = "Dashboard stats retrieved", Data = stats });
        }

        // GET /api/Dashboard/recent-transactions?limit=10
        [HttpGet("recent-transactions")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int limit = 10)
        {
            int userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new { Success = false, Message = "Invalid token" });

            var transactions = new List<RecentTransactionDto>();

            // income
            var incomes = await _context.IncomeDetails
                .Include(i => i.Category)
                .Include(i => i.CurrencyEntity)
                .Where(i => i.UserID == userId)
                .OrderByDescending(i => i.DateReceived)
                .Take(limit)
                .ToListAsync();

            foreach (var i in incomes)
            {
                transactions.Add(new RecentTransactionDto
                {
                    Type           = "Income",
                    Amount         = i.Amount,
                    Date           = i.DateReceived,
                    Description    = i.SourceName ?? "Income",
                    Category       = i.Category?.CategoryNameEnglish ?? "Other",
                    CurrencySymbol = i.CurrencyEntity?.CurrencySymbol ?? "$"
                });
            }

            // expenses
            var expenses = await _context.ExpenseDetails
                .Include(e => e.Category)
                .Include(e => e.CurrencyEntity)
                .Where(e => e.UserID == userId)
                .OrderByDescending(e => e.DateOfTransaction)
                .Take(limit)
                .ToListAsync();

            foreach (var e in expenses)
            {
                transactions.Add(new RecentTransactionDto
                {
                    Type           = "Expense",
                    Amount         = e.Amount,
                    Date           = e.DateOfTransaction,
                    Description    = e.AccountDeductedFrom ?? "Expense",
                    Category       = e.Category?.CategoryNameEnglish ?? "Other",
                    CurrencySymbol = e.CurrencyEntity?.CurrencySymbol ?? "$"
                });
            }

            // assets added recently
            var assets = await _context.CurrentAssets
                .Include(a => a.AssetCategory)
                .Include(a => a.CurrencyEntity)
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.CreatedDate)
                .Take(limit)
                .ToListAsync();

            foreach (var a in assets)
            {
                transactions.Add(new RecentTransactionDto
                {
                    Type           = "Asset",
                    Amount         = a.CurrentValue,
                    Date           = a.CreatedDate,
                    Description    = a.ItemName ?? "Asset",
                    Category       = a.AssetCategory?.CategoryNameEnglish ?? "Other",
                    CurrencySymbol = a.CurrencyEntity?.CurrencySymbol ?? "$"
                });
            }

            // sort everything by date, take the most recent `limit` items
            var sorted = transactions
                .OrderByDescending(t => t.Date)
                .Take(limit)
                .ToList();

            return Ok(new { Success = true, Message = "Recent transactions retrieved", Data = sorted });
        }

        // GET /api/Dashboard/income-summary?year=2025
        [HttpGet("income-summary")]
        public async Task<IActionResult> GetIncomeSummary([FromQuery] int year = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            if (year == 0) year = DateTime.Now.Year;

            var incomes = await _context.IncomeDetails
                .Where(i => i.UserID == userId && i.DateReceived.Year == year)
                .ToListAsync();

            // group by month
            var monthly = incomes
                .GroupBy(i => i.DateReceived.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(i => i.Amount) })
                .OrderBy(g => g.Month)
                .ToList();

            return Ok(new { Success = true, Message = "Income summary retrieved", Data = monthly, Year = year });
        }

        // GET /api/Dashboard/expense-summary?year=2025
        [HttpGet("expense-summary")]
        public async Task<IActionResult> GetExpenseSummary([FromQuery] int year = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            if (year == 0) year = DateTime.Now.Year;

            var expenses = await _context.ExpenseDetails
                .Where(e => e.UserID == userId && e.DateOfTransaction.Year == year)
                .ToListAsync();

            var monthly = expenses
                .GroupBy(e => e.DateOfTransaction.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(e => e.Amount) })
                .OrderBy(g => g.Month)
                .ToList();

            return Ok(new { Success = true, Message = "Expense summary retrieved", Data = monthly, Year = year });
        }

        // GET /api/Dashboard/asset-breakdown
        [HttpGet("asset-breakdown")]
        public async Task<IActionResult> GetAssetBreakdown()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var assets = await _context.CurrentAssets
                .Include(a => a.AssetCategory)
                .Where(a => a.UserID == userId)
                .ToListAsync();

            var grouped = assets
                .GroupBy(a => a.AssetCategory?.CategoryNameEnglish ?? "Other")
                .Select(g => new
                {
                    Category    = g.Key,
                    TotalValue  = g.Sum(a => a.CurrentValue * a.Quantity),
                    ItemCount   = g.Count()
                })
                .OrderByDescending(g => g.TotalValue)
                .ToList();

            return Ok(new { Success = true, Message = "Asset breakdown retrieved", Data = grouped });
        }

        // ─── helper ───────────────────────────────────────────────────
        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
