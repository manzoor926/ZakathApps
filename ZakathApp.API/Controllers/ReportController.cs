using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ZakathApp.API.Data;
using ZakathApp.API.Models;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    [Route("api/Report")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ZakathDbContext _context;
        private readonly IZakathCalculationService _zakathService;
        private readonly IIncomeService _incomeService;
        private readonly IExpenseService _expenseService;
        private readonly IAssetService _assetService;

        public ReportController(
            ZakathDbContext context,
            IZakathCalculationService zakathService,
            IIncomeService incomeService,
            IExpenseService expenseService,
            IAssetService assetService)
        {
            _context        = context;
            _zakathService  = zakathService;
            _incomeService  = incomeService;
            _expenseService = expenseService;
            _assetService   = assetService;
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/zakath-summary
        // Full zakath report: calculations + payments + net status
        // ──────────────────────────────────────────────────────────
        [HttpGet("zakath-summary")]
        public async Task<IActionResult> GetZakathSummary([FromQuery] int year = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();
            if (year == 0) year = DateTime.Now.Year;

            var calculations = await _context.ZakathCalculations
                .Where(c => c.UserID == userId && c.CalculationDate.Year == year)
                .OrderByDescending(c => c.CalculationDate)
                .ToListAsync();

            var payments = await _context.ZakathPayments
                .Where(p => p.UserID == userId && p.PaymentDate.Year == year)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            decimal totalDue  = calculations.Count > 0 ? calculations[0].ZakathAmount : 0;
            decimal totalPaid = payments.Sum(p => p.PaymentAmount);

            var report = new
            {
                Year            = year,
                TotalDue        = totalDue,
                TotalPaid       = totalPaid,
                RemainingDue    = Math.Max(0, totalDue - totalPaid),
                FullyPaid       = totalPaid >= totalDue && totalDue > 0,
                Calculations    = calculations.Select(c => new
                {
                    c.CalculationID,
                    c.CalculationDate,
                    c.HijriCalculationDate,
                    c.TotalAssets,
                    c.TotalLiabilities,
                    NetWorth        = c.TotalAssets - c.TotalLiabilities,
                    c.NisabThreshold,
                    c.ZakathAmount,
                    c.ZakathPercentage,
                    c.Currency,
                    c.CashAmount,
                    c.GoldAmount,
                    c.SilverAmount,
                    c.InvestmentAmount,
                    c.OtherAssetsAmount
                }).ToList(),
                Payments = payments.Select(p => new
                {
                    p.PaymentID,
                    p.PaymentDate,
                    p.HijriPaymentDate,
                    p.PaymentAmount,
                    p.RecipientName,
                    p.RecipientCategory,
                    p.PaymentMethod,
                    p.TransactionReference,
                    p.IsVerified
                }).ToList()
            };

            return Ok(new { Success = true, Message = "Zakath summary report", Data = report });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/income?year=2025&month=0
        // Income report: monthly breakdown + category breakdown
        // ──────────────────────────────────────────────────────────
        [HttpGet("income")]
        public async Task<IActionResult> GetIncomeReport([FromQuery] int year = 0, [FromQuery] int month = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();
            if (year == 0) year = DateTime.Now.Year;

            var q = _context.IncomeDetails
                .Include(i => i.Category)
                .Include(i => i.CurrencyEntity)
                .Where(i => i.UserID == userId && i.DateReceived.Year == year);

            if (month > 0) q = q.Where(i => i.DateReceived.Month == month);

            var records = await q.OrderByDescending(i => i.DateReceived).ToListAsync();

            var monthly = records
                .GroupBy(i => i.DateReceived.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(i => i.Amount), Count = g.Count() })
                .OrderBy(g => g.Month)
                .ToList();

            var byCategory = records
                .GroupBy(i => i.Category?.CategoryNameEnglish ?? "Other")
                .Select(g => new { Category = g.Key, Total = g.Sum(i => i.Amount), Count = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToList();

            var report = new
            {
                Year            = year,
                Month           = month,
                GrandTotal      = records.Sum(i => i.Amount),
                TotalRecords    = records.Count,
                MonthlyBreakdown = monthly,
                CategoryBreakdown = byCategory,
                Records = records.Select(i => new
                {
                    i.IncomeID,
                    i.Amount,
                    i.DateReceived,
                    i.HijriDateReceived,
                    CategoryName   = i.Category?.CategoryNameEnglish ?? "Other",
                    i.SourceName,
                    CurrencyCode   = i.CurrencyEntity?.CurrencyCode ?? "USD",
                    CurrencySymbol = i.CurrencyEntity?.CurrencySymbol ?? "$",
                    i.IsZakathEligible
                }).ToList()
            };

            return Ok(new { Success = true, Message = "Income report", Data = report });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/expense?year=2025&month=0
        // ──────────────────────────────────────────────────────────
        [HttpGet("expense")]
        public async Task<IActionResult> GetExpenseReport([FromQuery] int year = 0, [FromQuery] int month = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();
            if (year == 0) year = DateTime.Now.Year;

            var q = _context.ExpenseDetails
                .Include(e => e.Category)
                .Include(e => e.CurrencyEntity)
                .Where(e => e.UserID == userId && e.DateOfTransaction.Year == year);

            if (month > 0) q = q.Where(e => e.DateOfTransaction.Month == month);

            var records = await q.OrderByDescending(e => e.DateOfTransaction).ToListAsync();

            var monthly = records
                .GroupBy(e => e.DateOfTransaction.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
                .OrderBy(g => g.Month)
                .ToList();

            var byCategory = records
                .GroupBy(e => e.Category?.CategoryNameEnglish ?? "Other")
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToList();

            var report = new
            {
                Year              = year,
                Month             = month,
                GrandTotal        = records.Sum(e => e.Amount),
                TotalRecords      = records.Count,
                MonthlyBreakdown  = monthly,
                CategoryBreakdown = byCategory,
                Records = records.Select(e => new
                {
                    e.ExpenseID,
                    e.Amount,
                    e.DateOfTransaction,
                    e.HijriDateOfTransaction,
                    CategoryName   = e.Category?.CategoryNameEnglish ?? "Other",
                    e.AccountDeductedFrom,
                    CurrencyCode   = e.CurrencyEntity?.CurrencyCode ?? "USD",
                    CurrencySymbol = e.CurrencyEntity?.CurrencySymbol ?? "$"
                }).ToList()
            };

            return Ok(new { Success = true, Message = "Expense report", Data = report });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/asset
        // All assets with category breakdown and totals
        // ──────────────────────────────────────────────────────────
        [HttpGet("asset")]
        public async Task<IActionResult> GetAssetReport()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var assets = await _context.CurrentAssets
                .Include(a => a.AssetCategory)
                .Include(a => a.CurrencyEntity)
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.CurrentValue)
                .ToListAsync();

            var byCategory = assets
                .GroupBy(a => a.AssetCategory?.CategoryNameEnglish ?? "Other")
                .Select(g => new
                {
                    Category       = g.Key,
                    TotalValue     = g.Sum(a => a.CurrentValue * a.Quantity),
                    TotalPurchase  = g.Sum(a => a.PurchaseValue * a.Quantity),
                    ItemCount      = g.Count(),
                    ZakathApplicable = g.All(a => a.IsZakathApplicable)
                })
                .OrderByDescending(g => g.TotalValue)
                .ToList();

            var zakathableTotal    = assets.Where(a => a.IsZakathApplicable).Sum(a => a.CurrentValue * a.Quantity);
            var nonZakathableTotal = assets.Where(a => !a.IsZakathApplicable).Sum(a => a.CurrentValue * a.Quantity);

            var report = new
            {
                TotalCurrentValue  = assets.Sum(a => a.CurrentValue * a.Quantity),
                TotalPurchaseValue = assets.Sum(a => a.PurchaseValue * a.Quantity),
                ZakathableTotal    = zakathableTotal,
                NonZakathableTotal = nonZakathableTotal,
                TotalItems         = assets.Count,
                CategoryBreakdown  = byCategory,
                Assets = assets.Select(a => new
                {
                    a.AssetID,
                    a.ItemName,
                    a.ItemNumber,
                    a.DateAcquired,
                    a.HijriDateAcquired,
                    a.PurchaseValue,
                    a.CurrentValue,
                    a.Quantity,
                    a.MeasurementUnit,
                    a.IsZakathApplicable,
                    CategoryName   = a.AssetCategory?.CategoryNameEnglish ?? "Other",
                    CurrencyCode   = a.CurrencyEntity?.CurrencyCode ?? "USD",
                    CurrencySymbol = a.CurrencyEntity?.CurrencySymbol ?? "$",
                    a.LastValuationDate
                }).ToList()
            };

            return Ok(new { Success = true, Message = "Asset report", Data = report });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/csv/income?year=2025
        // Download income as CSV
        // ──────────────────────────────────────────────────────────
        [HttpGet("csv/income")]
        public async Task<IActionResult> ExportIncomeCsv([FromQuery] int year = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();
            if (year == 0) year = DateTime.Now.Year;

            var records = await _context.IncomeDetails
                .Include(i => i.Category)
                .Include(i => i.CurrencyEntity)
                .Where(i => i.UserID == userId && i.DateReceived.Year == year)
                .OrderByDescending(i => i.DateReceived)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("IncomeID,Date,HijriDate,Amount,Currency,Category,Source,IsRecurring,IsZakathEligible");
            foreach (var i in records)
            {
                sb.AppendLine($"{i.IncomeID},{i.DateReceived:yyyy-MM-dd},{i.HijriDateReceived},{i.Amount},{i.CurrencyEntity?.CurrencyCode ?? "USD"},{i.Category?.CategoryNameEnglish ?? "Other"},{i.SourceName ?? ""},{i.IsRecurring},{i.IsZakathEligible}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"Income_Report_{year}.csv");
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/csv/expense?year=2025
        // ──────────────────────────────────────────────────────────
        [HttpGet("csv/expense")]
        public async Task<IActionResult> ExportExpenseCsv([FromQuery] int year = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();
            if (year == 0) year = DateTime.Now.Year;

            var records = await _context.ExpenseDetails
                .Include(e => e.Category)
                .Include(e => e.CurrencyEntity)
                .Where(e => e.UserID == userId && e.DateOfTransaction.Year == year)
                .OrderByDescending(e => e.DateOfTransaction)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ExpenseID,Date,HijriDate,Amount,Currency,Category,Account,IsRecurring");
            foreach (var e in records)
            {
                sb.AppendLine($"{e.ExpenseID},{e.DateOfTransaction:yyyy-MM-dd},{e.HijriDateOfTransaction},{e.Amount},{e.CurrencyEntity?.CurrencyCode ?? "USD"},{e.Category?.CategoryNameEnglish ?? "Other"},{e.AccountDeductedFrom ?? ""},{e.IsRecurring}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"Expense_Report_{year}.csv");
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/csv/asset
        // ──────────────────────────────────────────────────────────
        [HttpGet("csv/asset")]
        public async Task<IActionResult> ExportAssetCsv()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var records = await _context.CurrentAssets
                .Include(a => a.AssetCategory)
                .Include(a => a.CurrencyEntity)
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.CurrentValue)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("AssetID,ItemName,DateAcquired,HijriDate,PurchaseValue,CurrentValue,Quantity,Unit,Currency,Category,IsZakathApplicable");
            foreach (var a in records)
            {
                sb.AppendLine($"{a.AssetID},{a.ItemName ?? ""},{a.DateAcquired:yyyy-MM-dd},{a.HijriDateAcquired},{a.PurchaseValue},{a.CurrentValue},{a.Quantity},{a.MeasurementUnit ?? ""},{a.CurrencyEntity?.CurrencyCode ?? "USD"},{a.AssetCategory?.CategoryNameEnglish ?? "Other"},{a.IsZakathApplicable}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "Asset_Report.csv");
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/Report/csv/zakath?year=2025
        // ──────────────────────────────────────────────────────────
        [HttpGet("csv/zakath")]
        public async Task<IActionResult> ExportZakathCsv([FromQuery] int year = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();
            if (year == 0) year = DateTime.Now.Year;

            var calcs = await _context.ZakathCalculations
                .Where(c => c.UserID == userId && c.CalculationDate.Year == year)
                .OrderByDescending(c => c.CalculationDate)
                .ToListAsync();

            var payments = await _context.ZakathPayments
                .Where(p => p.UserID == userId && p.PaymentDate.Year == year)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("=== ZAKATH CALCULATIONS ===");
            sb.AppendLine("CalcID,Date,HijriDate,TotalAssets,Liabilities,Nisab,ZakathAmount,Currency");
            foreach (var c in calcs)
                sb.AppendLine($"{c.CalculationID},{c.CalculationDate:yyyy-MM-dd},{c.HijriCalculationDate},{c.TotalAssets},{c.TotalLiabilities},{c.NisabThreshold},{c.ZakathAmount},{c.Currency}");

            sb.AppendLine("");
            sb.AppendLine("=== ZAKATH PAYMENTS ===");
            sb.AppendLine("PaymentID,Date,HijriDate,Amount,Recipient,Category,Method,Reference,Verified");
            foreach (var p in payments)
                sb.AppendLine($"{p.PaymentID},{p.PaymentDate:yyyy-MM-dd},{p.HijriPaymentDate},{p.PaymentAmount},{p.RecipientName ?? ""},{p.RecipientCategory ?? ""},{p.PaymentMethod ?? ""},{p.TransactionReference ?? ""},{p.IsVerified}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"Zakath_Report_{year}.csv");
        }
    }
}
