using Microsoft.EntityFrameworkCore;
using ZakathApp.API.Data;
using ZakathApp.API.DTOs;
using ZakathApp.API.Helpers;
using ZakathApp.API.Models;

namespace ZakathApp.API.Services
{
    // ========================================
    // ZAKATH CALCULATION SERVICE
    // ========================================
    public class ZakathCalculationService : IZakathCalculationService
    {
        private readonly ZakathDbContext _context;
        private readonly ILogger<ZakathCalculationService> _logger;
        private readonly HijriDateHelper _hijriHelper;
        private readonly ICurrencyService _currencyService;

        public ZakathCalculationService(
            ZakathDbContext context,
            ILogger<ZakathCalculationService> logger,
            HijriDateHelper hijriHelper,
            ICurrencyService currencyService)
        {
            _context = context;
            _logger = logger;
            _hijriHelper = hijriHelper;
            _currencyService = currencyService;
        }

        public async Task<ZakathCalculationResultDto> CalculateZakathAsync(int userId, string baseCurrency = "USD", int madhabId = 1)
        {
            _logger.LogInformation($"Starting Zakath calculation for UserID: {userId}");

            // resolve base currency entity
            var currencyEntity = await _context.Currencies
                .FirstOrDefaultAsync(c => c.CurrencyCode == baseCurrency);
            if (currencyEntity == null)
            {
                baseCurrency = "USD";
                currencyEntity = await _context.Currencies
                    .FirstOrDefaultAsync(c => c.CurrencyCode == "USD");
            }

            // --------------------------------------------------------
            // 1.  Gather every zakath-applicable asset, bucket it
            // --------------------------------------------------------
            var assets = await _context.CurrentAssets
                .Include(a => a.AssetCategory)
                .Include(a => a.CurrencyEntity)
                .Where(a => a.UserID == userId && a.IsZakathApplicable)
                .ToListAsync();

            decimal cash = 0, gold = 0, silver = 0, investments = 0, other = 0;

            foreach (var a in assets)
            {
                decimal val = await ConvertAmount(
                    a.CurrentValue * a.Quantity,
                    a.CurrencyEntity?.CurrencyCode,
                    baseCurrency);

                string label = ((a.AssetCategory?.CategoryNameEnglish ?? "") + " " + (a.ItemName ?? "")).ToLower();

                if      (label.Contains("cash")   || label.Contains("bank")   || label.Contains("saving"))  cash        += val;
                else if (label.Contains("gold"))                                                              gold        += val;
                else if (label.Contains("silver"))                                                            silver      += val;
                else if (label.Contains("invest") || label.Contains("stock")  || label.Contains("bond"))    investments += val;
                else                                                                                          other       += val;
            }

            // --------------------------------------------------------
            // 2.  Add hawl-period income to cash bucket
            // --------------------------------------------------------
            var hawlStart = DateTime.Now.AddDays(-354);

            var incomes = await _context.IncomeDetails
                .Include(i => i.CurrencyEntity)
                .Where(i => i.UserID == userId && i.IsZakathEligible && i.DateReceived >= hawlStart)
                .ToListAsync();

            foreach (var i in incomes)
                cash += await ConvertAmount(i.Amount, i.CurrencyEntity?.CurrencyCode, baseCurrency);

            // --------------------------------------------------------
            // 3.  Liabilities = hawl-period expenses
            // --------------------------------------------------------
            var expenses = await _context.ExpenseDetails
                .Include(e => e.CurrencyEntity)
                .Where(e => e.UserID == userId && e.DateOfTransaction >= hawlStart)
                .ToListAsync();

            decimal liabilities = 0;
            foreach (var e in expenses)
                liabilities += await ConvertAmount(e.Amount, e.CurrencyEntity?.CurrencyCode, baseCurrency);

            // --------------------------------------------------------
            // 4.  Core calculation
            // --------------------------------------------------------
            decimal totalAssets = cash + gold + silver + investments + other;
            decimal netWorth    = totalAssets - liabilities;

            decimal nisab = await GetNisabThresholdAsync(baseCurrency, madhabId);
            bool isZakathDue = netWorth >= nisab;

            // madhab-specific percentage (default 2.5 %)
            decimal zakathPct = 2.5m;
            var rule = await _context.MadhabZakathRules
                .FirstOrDefaultAsync(r => r.MadhabID == madhabId && r.IsActive);
            if (rule != null) zakathPct = rule.ZakathPercentage;

            decimal zakathAmount = isZakathDue ? Math.Round(netWorth * zakathPct / 100m, 2) : 0m;

            // --------------------------------------------------------
            // 5.  Persist
            // --------------------------------------------------------
            var calc = new ZakathCalculation
            {
                UserID                = userId,
                CalculationDate       = DateTime.Now,
                HijriCalculationDate  = _hijriHelper.ConvertToHijri(DateTime.Now),
                TotalAssets           = totalAssets,
                TotalLiabilities      = liabilities,
                NisabThreshold        = nisab,
                ZakathAmount          = zakathAmount,
                ZakathPercentage      = zakathPct,
                CashAmount            = cash,
                GoldAmount            = gold,
                SilverAmount          = silver,
                InvestmentAmount      = investments,
                OtherAssetsAmount     = other,
                Currency              = baseCurrency,
                BaseCurrencyID        = currencyEntity?.CurrencyID,
                CalculationNotes      = $"Madhab ID: {madhabId}",
                CreatedDate           = DateTime.Now
            };

            _context.ZakathCalculations.Add(calc);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Zakath done. Amount: {zakathAmount} {baseCurrency}");

            return new ZakathCalculationResultDto
            {
                CalculationID    = calc.CalculationID,
                CalculationDate  = calc.CalculationDate,
                HijriDate        = calc.HijriCalculationDate,
                TotalAssets      = totalAssets,
                TotalLiabilities = liabilities,
                NetWorth         = netWorth,
                NisabThreshold   = nisab,
                IsZakathDue      = isZakathDue,
                ZakathAmount     = zakathAmount,
                ZakathPercentage = zakathPct,
                Currency         = baseCurrency,
                AssetBreakdown   = new AssetBreakdownDto
                {
                    Cash = cash, Gold = gold, Silver = silver,
                    Investments = investments, OtherAssets = other
                }
            };
        }

        public async Task<decimal> GetNisabThresholdAsync(string currency, int madhabId)
        {
            var r = await _context.MadhabZakathRules
                .FirstOrDefaultAsync(x => x.MadhabID == madhabId && x.RuleType == "Silver" && x.IsActive);
            return r?.NisabValue ?? 595m;   // 595 g silver fallback
        }

        public async Task<List<ZakathCalculationResultDto>> GetCalculationHistoryAsync(int userId)
        {
            var list = await _context.ZakathCalculations
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.CalculationDate)
                .Take(50)
                .ToListAsync();

            return list.Select(MapCalc).ToList();
        }

        public async Task<ZakathCalculationResultDto> GetCalculationByIdAsync(int calculationId)
        {
            var c = await _context.ZakathCalculations.FindAsync(calculationId);
            return c == null ? null : MapCalc(c);
        }

        public async Task<ServiceResult> SaveCalculationAsync(ZakathCalculationResultDto calculationDto)
        {
            // CalculateZakathAsync already persists; this is a no-op safety net
            return new ServiceResult { Success = true, Message = "Calculation saved" };
        }

        // --- private helpers ---------------------------------------------------
        private async Task<decimal> ConvertAmount(decimal amount, string fromCode, string toCode)
        {
            if (string.IsNullOrEmpty(fromCode) || fromCode == toCode) return amount;
            return amount * await _currencyService.GetExchangeRateAsync(fromCode, toCode);
        }

        private static ZakathCalculationResultDto MapCalc(ZakathCalculation c) => new()
        {
            CalculationID    = c.CalculationID,
            CalculationDate  = c.CalculationDate,
            HijriDate        = c.HijriCalculationDate,
            TotalAssets      = c.TotalAssets,
            TotalLiabilities = c.TotalLiabilities,
            NetWorth         = c.TotalAssets - c.TotalLiabilities,
            NisabThreshold   = c.NisabThreshold,
            IsZakathDue      = c.ZakathAmount > 0,
            ZakathAmount     = c.ZakathAmount,
            ZakathPercentage = c.ZakathPercentage,
            Currency         = c.Currency,
            AssetBreakdown   = new AssetBreakdownDto
            {
                Cash = c.CashAmount, Gold = c.GoldAmount, Silver = c.SilverAmount,
                Investments = c.InvestmentAmount, OtherAssets = c.OtherAssetsAmount
            }
        };
    }

    // ========================================
    // INCOME SERVICE
    // ========================================
    public class IncomeService : IIncomeService
    {
        private readonly ZakathDbContext _context;
        private readonly ILogger<IncomeService> _logger;
        private readonly HijriDateHelper _hijriHelper;

        public IncomeService(ZakathDbContext context, ILogger<IncomeService> logger, HijriDateHelper hijriHelper)
        {
            _context     = context;
            _logger      = logger;
            _hijriHelper = hijriHelper;
        }

        public async Task<ServiceResult> CreateIncomeAsync(CreateIncomeDto dto)
        {
            var entity = new IncomeDetail
            {
                UserID             = dto.UserID,
                Amount             = dto.Amount,
                DateReceived       = dto.DateReceived,
                HijriDateReceived  = _hijriHelper.ConvertToHijri(dto.DateReceived),
                CategoryID         = dto.CategoryID,
                SourceName         = dto.SourceName,
                Notes              = dto.Notes,
                IsRecurring        = dto.IsRecurring,
                RecurringFrequency = dto.RecurringFrequency,
                CurrencyID         = dto.CurrencyID,
                IsZakathEligible   = dto.IsZakathEligible,
                CreatedDate        = DateTime.Now,
                LastModifiedDate   = DateTime.Now
            };

            _context.IncomeDetails.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Income {entity.IncomeID} created for User {dto.UserID}");

            return new ServiceResult { Success = true, Message = "Income created successfully", Data = entity.IncomeID };
        }

        public async Task<ServiceResult> UpdateIncomeAsync(int incomeId, CreateIncomeDto dto)
        {
            var entity = await _context.IncomeDetails.FindAsync(incomeId);
            if (entity == null)
                return new ServiceResult { Success = false, Message = "Income not found" };

            entity.Amount             = dto.Amount;
            entity.DateReceived       = dto.DateReceived;
            entity.HijriDateReceived  = _hijriHelper.ConvertToHijri(dto.DateReceived);
            entity.CategoryID         = dto.CategoryID;
            entity.SourceName         = dto.SourceName;
            entity.Notes              = dto.Notes;
            entity.IsRecurring        = dto.IsRecurring;
            entity.RecurringFrequency = dto.RecurringFrequency;
            entity.CurrencyID         = dto.CurrencyID;
            entity.IsZakathEligible   = dto.IsZakathEligible;
            entity.LastModifiedDate   = DateTime.Now;

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Income updated successfully" };
        }

        public async Task<ServiceResult> DeleteIncomeAsync(int incomeId)
        {
            var entity = await _context.IncomeDetails.FindAsync(incomeId);
            if (entity == null)
                return new ServiceResult { Success = false, Message = "Income not found" };

            _context.IncomeDetails.Remove(entity);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Income deleted successfully" };
        }

        public async Task<IncomeDto> GetIncomeByIdAsync(int incomeId)
        {
            var e = await _context.IncomeDetails
                .Include(x => x.Category)
                .Include(x => x.CurrencyEntity)
                .FirstOrDefaultAsync(x => x.IncomeID == incomeId);

            return e == null ? null : MapIncome(e);
        }

        public async Task<List<IncomeDto>> GetUserIncomesAsync(int userId)
        {
            var list = await _context.IncomeDetails
                .Include(x => x.Category)
                .Include(x => x.CurrencyEntity)
                .Where(x => x.UserID == userId)
                .OrderByDescending(x => x.DateReceived)
                .ToListAsync();

            return list.Select(MapIncome).ToList();
        }

        public async Task<decimal> GetTotalIncomeAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var q = _context.IncomeDetails.Where(i => i.UserID == userId);
            if (startDate.HasValue) q = q.Where(i => i.DateReceived >= startDate.Value);
            if (endDate.HasValue)   q = q.Where(i => i.DateReceived <= endDate.Value);
            return await q.SumAsync(i => i.Amount);
        }

        private static IncomeDto MapIncome(IncomeDetail i) => new()
        {
            IncomeID          = i.IncomeID,
            UserID            = i.UserID,
            Amount            = i.Amount,
            DateReceived      = i.DateReceived,
            HijriDateReceived = i.HijriDateReceived,
            CategoryName      = i.Category?.CategoryNameEnglish,
            SourceName        = i.SourceName,
            Notes             = i.Notes,
            IsRecurring       = i.IsRecurring,
            CurrencyCode      = i.CurrencyEntity?.CurrencyCode   ?? "USD",
            CurrencySymbol    = i.CurrencyEntity?.CurrencySymbol  ?? "$",
            IsZakathEligible  = i.IsZakathEligible
        };
    }

    // ========================================
    // EXPENSE SERVICE
    // ========================================
    public class ExpenseService : IExpenseService
    {
        private readonly ZakathDbContext _context;
        private readonly ILogger<ExpenseService> _logger;
        private readonly HijriDateHelper _hijriHelper;

        public ExpenseService(ZakathDbContext context, ILogger<ExpenseService> logger, HijriDateHelper hijriHelper)
        {
            _context     = context;
            _logger      = logger;
            _hijriHelper = hijriHelper;
        }

        public async Task<ServiceResult> CreateExpenseAsync(CreateExpenseDto dto)
        {
            var entity = new ExpenseDetail
            {
                UserID                   = dto.UserID,
                Amount                   = dto.Amount,
                DateOfTransaction        = dto.DateOfTransaction,
                HijriDateOfTransaction   = _hijriHelper.ConvertToHijri(dto.DateOfTransaction),
                CategoryID               = dto.CategoryID,
                AccountDeductedFrom      = dto.AccountDeductedFrom,
                Notes                    = dto.Notes,
                IsRecurring              = dto.IsRecurring,
                RecurringFrequency       = dto.RecurringFrequency,
                CurrencyID               = dto.CurrencyID,
                CreatedDate              = DateTime.Now,
                LastModifiedDate         = DateTime.Now
            };

            _context.ExpenseDetails.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Expense {entity.ExpenseID} created for User {dto.UserID}");

            return new ServiceResult { Success = true, Message = "Expense created successfully", Data = entity.ExpenseID };
        }

        public async Task<ServiceResult> UpdateExpenseAsync(int expenseId, CreateExpenseDto dto)
        {
            var entity = await _context.ExpenseDetails.FindAsync(expenseId);
            if (entity == null)
                return new ServiceResult { Success = false, Message = "Expense not found" };

            entity.Amount                 = dto.Amount;
            entity.DateOfTransaction      = dto.DateOfTransaction;
            entity.HijriDateOfTransaction = _hijriHelper.ConvertToHijri(dto.DateOfTransaction);
            entity.CategoryID             = dto.CategoryID;
            entity.AccountDeductedFrom    = dto.AccountDeductedFrom;
            entity.Notes                  = dto.Notes;
            entity.IsRecurring            = dto.IsRecurring;
            entity.RecurringFrequency     = dto.RecurringFrequency;
            entity.CurrencyID             = dto.CurrencyID;
            entity.LastModifiedDate       = DateTime.Now;

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Expense updated successfully" };
        }

        public async Task<ServiceResult> DeleteExpenseAsync(int expenseId)
        {
            var entity = await _context.ExpenseDetails.FindAsync(expenseId);
            if (entity == null)
                return new ServiceResult { Success = false, Message = "Expense not found" };

            _context.ExpenseDetails.Remove(entity);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Expense deleted successfully" };
        }

        public async Task<ExpenseDto> GetExpenseByIdAsync(int expenseId)
        {
            var e = await _context.ExpenseDetails
                .Include(x => x.Category)
                .Include(x => x.CurrencyEntity)
                .FirstOrDefaultAsync(x => x.ExpenseID == expenseId);

            return e == null ? null : MapExpense(e);
        }

        public async Task<List<ExpenseDto>> GetUserExpensesAsync(int userId)
        {
            var list = await _context.ExpenseDetails
                .Include(x => x.Category)
                .Include(x => x.CurrencyEntity)
                .Where(x => x.UserID == userId)
                .OrderByDescending(x => x.DateOfTransaction)
                .ToListAsync();

            return list.Select(MapExpense).ToList();
        }

        public async Task<decimal> GetTotalExpensesAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var q = _context.ExpenseDetails.Where(e => e.UserID == userId);
            if (startDate.HasValue) q = q.Where(e => e.DateOfTransaction >= startDate.Value);
            if (endDate.HasValue)   q = q.Where(e => e.DateOfTransaction <= endDate.Value);
            return await q.SumAsync(e => e.Amount);
        }

        private static ExpenseDto MapExpense(ExpenseDetail e) => new()
        {
            ExpenseID                = e.ExpenseID,
            UserID                   = e.UserID,
            Amount                   = e.Amount,
            DateOfTransaction        = e.DateOfTransaction,
            HijriDateOfTransaction   = e.HijriDateOfTransaction,
            CategoryName             = e.Category?.CategoryNameEnglish,
            AccountDeductedFrom      = e.AccountDeductedFrom,
            Notes                    = e.Notes,
            IsRecurring              = e.IsRecurring,
            CurrencyCode             = e.CurrencyEntity?.CurrencyCode   ?? "USD",
            CurrencySymbol           = e.CurrencyEntity?.CurrencySymbol  ?? "$"
        };
    }

    // ========================================
    // ASSET SERVICE
    // ========================================
    public class AssetService : IAssetService
    {
        private readonly ZakathDbContext _context;
        private readonly ILogger<AssetService> _logger;
        private readonly HijriDateHelper _hijriHelper;

        public AssetService(ZakathDbContext context, ILogger<AssetService> logger, HijriDateHelper hijriHelper)
        {
            _context     = context;
            _logger      = logger;
            _hijriHelper = hijriHelper;
        }

        public async Task<ServiceResult> CreateAssetAsync(CreateAssetDto dto)
        {
            var entity = new CurrentAsset
            {
                UserID              = dto.UserID,
                ItemName            = dto.ItemName,
                ItemMasterID        = dto.ItemMasterID,
                AssetCategoryID     = dto.AssetCategoryID,
                DateAcquired        = dto.DateAcquired,
                HijriDateAcquired   = _hijriHelper.ConvertToHijri(dto.DateAcquired),
                PurchaseValue       = dto.PurchaseValue,
                CurrentValue        = dto.CurrentValue,
                Quantity            = dto.Quantity,
                MeasurementUnit     = dto.MeasurementUnit,
                IsZakathApplicable  = dto.IsZakathApplicable,
                Notes               = dto.Notes,
                CurrencyID          = dto.CurrencyID,
                LastValuationDate   = DateTime.Now,
                CreatedDate         = DateTime.Now,
                LastModifiedDate    = DateTime.Now
            };

            _context.CurrentAssets.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Asset {entity.AssetID} created for User {dto.UserID}");

            return new ServiceResult { Success = true, Message = "Asset created successfully", Data = entity.AssetID };
        }

        public async Task<ServiceResult> UpdateAssetAsync(int assetId, CreateAssetDto dto)
        {
            var entity = await _context.CurrentAssets.FindAsync(assetId);
            if (entity == null)
                return new ServiceResult { Success = false, Message = "Asset not found" };

            entity.ItemName           = dto.ItemName;
            entity.ItemMasterID       = dto.ItemMasterID;
            entity.AssetCategoryID    = dto.AssetCategoryID;
            entity.DateAcquired       = dto.DateAcquired;
            entity.HijriDateAcquired  = _hijriHelper.ConvertToHijri(dto.DateAcquired);
            entity.PurchaseValue      = dto.PurchaseValue;
            entity.CurrentValue       = dto.CurrentValue;
            entity.Quantity           = dto.Quantity;
            entity.MeasurementUnit    = dto.MeasurementUnit;
            entity.IsZakathApplicable = dto.IsZakathApplicable;
            entity.Notes              = dto.Notes;
            entity.CurrencyID         = dto.CurrencyID;
            entity.LastValuationDate  = DateTime.Now;
            entity.LastModifiedDate   = DateTime.Now;

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Asset updated successfully" };
        }

        public async Task<ServiceResult> DeleteAssetAsync(int assetId)
        {
            var entity = await _context.CurrentAssets.FindAsync(assetId);
            if (entity == null)
                return new ServiceResult { Success = false, Message = "Asset not found" };

            _context.CurrentAssets.Remove(entity);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Asset deleted successfully" };
        }

        public async Task<AssetDto> GetAssetByIdAsync(int assetId)
        {
            var e = await _context.CurrentAssets
                .Include(x => x.AssetCategory)
                .Include(x => x.CurrencyEntity)
                .FirstOrDefaultAsync(x => x.AssetID == assetId);

            return e == null ? null : MapAsset(e);
        }

        public async Task<List<AssetDto>> GetUserAssetsAsync(int userId)
        {
            var list = await _context.CurrentAssets
                .Include(x => x.AssetCategory)
                .Include(x => x.CurrencyEntity)
                .Where(x => x.UserID == userId)
                .OrderByDescending(x => x.DateAcquired)
                .ToListAsync();

            return list.Select(MapAsset).ToList();
        }

        public async Task<decimal> GetTotalAssetsValueAsync(int userId)
        {
            return await _context.CurrentAssets
                .Where(a => a.UserID == userId && a.IsZakathApplicable)
                .SumAsync(a => a.CurrentValue * a.Quantity);
        }

        public async Task<ServiceResult> UpdateAssetValueAsync(int assetId, decimal newValue)
        {
            var entity = await _context.CurrentAssets.FindAsync(assetId);
            if (entity == null)
                return new ServiceResult { Success = false, Message = "Asset not found" };

            entity.CurrentValue     = newValue;
            entity.LastValuationDate = DateTime.Now;
            entity.LastModifiedDate  = DateTime.Now;

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Asset value updated successfully" };
        }

        private static AssetDto MapAsset(CurrentAsset a) => new()
        {
            AssetID            = a.AssetID,
            UserID             = a.UserID,
            ItemName           = a.ItemName,
            ItemNumber         = a.ItemNumber,
            DateAcquired       = a.DateAcquired,
            HijriDateAcquired  = a.HijriDateAcquired,
            PurchaseValue      = a.PurchaseValue,
            CurrentValue       = a.CurrentValue,
            Quantity           = a.Quantity,
            MeasurementUnit    = a.MeasurementUnit,
            IsZakathApplicable = a.IsZakathApplicable,
            CategoryName       = a.AssetCategory?.CategoryNameEnglish,
            Notes              = a.Notes,
            CurrencyCode       = a.CurrencyEntity?.CurrencyCode   ?? "USD",
            CurrencySymbol     = a.CurrencyEntity?.CurrencySymbol  ?? "$",
            LastValuationDate  = a.LastValuationDate
        };
    }

    // ========================================
    // CURRENCY SERVICE
    // ========================================
    public class CurrencyService : ICurrencyService
    {
        private readonly ZakathDbContext _context;
        private readonly ILogger<CurrencyService> _logger;

        // Static lookup: every value = how many units of that currency per 1 USD
        private static readonly Dictionary<string, decimal> PerUsd = new()
        {
            ["USD"] = 1.0m,
            ["SAR"] = 3.75m,
            ["AED"] = 3.67m,
            ["INR"] = 83.12m,
            ["EUR"] = 0.92m,
            ["GBP"] = 0.79m,
            ["KWD"] = 0.31m,
            ["QAR"] = 3.64m,
            ["OMR"] = 0.385m,
            ["BHD"] = 0.376m,
            ["MYR"] = 4.47m,
            ["SGD"] = 1.34m
        };

        public CurrencyService(ZakathDbContext context, ILogger<CurrencyService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<ServiceResult> GetAllCurrenciesAsync()
        {
            var list = await _context.Currencies
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var dtos = list.Select(c => new CurrencyDto
            {
                CurrencyID     = c.CurrencyID,
                CurrencyCode   = c.CurrencyCode,
                CurrencyName   = c.CurrencyName,
                CurrencySymbol = c.CurrencySymbol,
                DecimalPlaces  = c.DecimalPlaces
            }).ToList();

            return new ServiceResult { Success = true, Message = "Currencies retrieved successfully", Data = dtos };
        }

        public async Task<ServiceResult> GetUserCurrenciesAsync(int userId)
        {
            var list = await _context.UserCurrencies
                .Include(uc => uc.Currency)
                .Where(uc => uc.UserID == userId && uc.IsActive)
                .ToListAsync();

            var dtos = list.Select(uc => new UserCurrencyDto
            {
                UserCurrencyID = uc.UserCurrencyID,
                CurrencyID     = uc.CurrencyID,
                CurrencyCode   = uc.Currency.CurrencyCode,
                CurrencyName   = uc.Currency.CurrencyName,
                CurrencySymbol = uc.Currency.CurrencySymbol,
                IsPrimary      = uc.IsPrimary
            }).ToList();

            return new ServiceResult { Success = true, Message = "User currencies retrieved", Data = dtos };
        }

        public async Task<ServiceResult> AddUserCurrencyAsync(int userId, AddUserCurrencyDto dto)
        {
            bool exists = await _context.UserCurrencies
                .AnyAsync(uc => uc.UserID == userId && uc.CurrencyID == dto.CurrencyID);
            if (exists)
                return new ServiceResult { Success = false, Message = "Currency already added to your account" };

            // If caller wants this one as primary, clear every other primary first
            if (dto.IsPrimary)
            {
                var others = await _context.UserCurrencies
                    .Where(uc => uc.UserID == userId && uc.IsPrimary).ToListAsync();
                others.ForEach(uc => uc.IsPrimary = false);
            }

            _context.UserCurrencies.Add(new UserCurrency
            {
                UserID     = userId,
                CurrencyID = dto.CurrencyID,
                IsPrimary  = dto.IsPrimary,
                IsActive   = true
            });

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Currency added successfully" };
        }

        public async Task<ServiceResult> SetPrimaryCurrencyAsync(int userId, int currencyId)
        {
            var all = await _context.UserCurrencies
                .Where(uc => uc.UserID == userId).ToListAsync();

            foreach (var uc in all)
                uc.IsPrimary = (uc.CurrencyID == currencyId);

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Primary currency updated" };
        }

        public async Task<ServiceResult> ConvertCurrencyAsync(CurrencyConversionDto dto)
        {
            decimal rate = await GetExchangeRateAsync(dto.FromCurrency, dto.ToCurrency);

            return new ServiceResult
            {
                Success = true,
                Message = "Currency converted successfully",
                Data    = new CurrencyConversionResultDto
                {
                    OriginalAmount  = dto.Amount,
                    ConvertedAmount = Math.Round(dto.Amount * rate, 2),
                    Rate            = rate,
                    FromCurrency    = dto.FromCurrency,
                    ToCurrency      = dto.ToCurrency,
                    ConversionDate  = DateTime.Now
                }
            };
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency) return 1.0m;

            // 1) Try a cached row that is less than 7 days old
            var cached = await _context.CurrencyExchangeRates
                .Include(r => r.FromCurrency)
                .Include(r => r.ToCurrency)
                .Where(r => r.FromCurrency.CurrencyCode == fromCurrency
                         && r.ToCurrency.CurrencyCode   == toCurrency
                         && r.IsActive)
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();

            if (cached != null && (DateTime.Now - cached.EffectiveDate).TotalDays <= 7)
                return cached.Rate;

            // 2) Compute from the static table and cache it
            if (PerUsd.TryGetValue(fromCurrency, out var fromVal) &&
                PerUsd.TryGetValue(toCurrency,   out var toVal))
            {
                decimal rate = toVal / fromVal;   // cross-rate through USD

                // persist for future cache hits
                var fromEntity = await _context.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == fromCurrency);
                var toEntity   = await _context.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == toCurrency);

                if (fromEntity != null && toEntity != null)
                {
                    _context.CurrencyExchangeRates.Add(new CurrencyExchangeRate
                    {
                        FromCurrencyID = fromEntity.CurrencyID,
                        ToCurrencyID   = toEntity.CurrencyID,
                        Rate           = rate,
                        EffectiveDate  = DateTime.Now,
                        Source         = "System",
                        IsActive       = true
                    });
                    await _context.SaveChangesAsync();
                }

                return rate;
            }

            _logger.LogWarning($"No rate found: {fromCurrency} → {toCurrency}. Returning 1.0");
            return 1.0m;
        }

        public async Task<ServiceResult> UpdateExchangeRatesAsync()
        {
            int count = 0;
            foreach (var from in PerUsd.Keys)
                foreach (var to in PerUsd.Keys)
                    if (from != to) { await GetExchangeRateAsync(from, to); count++; }

            return new ServiceResult { Success = true, Message = $"Refreshed {count} exchange rates" };
        }
    }

    // ========================================
    // MADHAB SERVICE
    // ========================================
    public class MadhabService : IMadhabService
    {
        private readonly ZakathDbContext _context;

        public MadhabService(ZakathDbContext context) { _context = context; }

        public async Task<ServiceResult> GetAllMadhabsAsync()
        {
            var list = await _context.Madhabs
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            var dtos = list.Select(m => new MadhabDto
            {
                MadhabID           = m.MadhabID,
                MadhabNameEnglish  = m.MadhabNameEnglish,
                MadhabNameArabic   = m.MadhabNameArabic,
                Description        = m.Description
            }).ToList();

            return new ServiceResult { Success = true, Message = "Madhabs retrieved", Data = dtos };
        }

        public async Task<ServiceResult> GetMadhabRulesAsync(int madhabId)
        {
            var list = await _context.MadhabZakathRules
                .Where(r => r.MadhabID == madhabId && r.IsActive)
                .ToListAsync();

            var dtos = list.Select(r => new MadhabRuleDto
            {
                MadhabRuleID      = r.MadhabRuleID,
                MadhabID          = r.MadhabID,
                RuleType          = r.RuleType,
                NisabValue        = r.NisabValue,
                ZakathPercentage  = r.ZakathPercentage,
                HawlPeriodDays    = r.HawlPeriodDays,
                SpecialConditions = r.SpecialConditions,
                ReferenceSource   = r.ReferenceSource
            }).ToList();

            return new ServiceResult { Success = true, Message = "Rules retrieved", Data = dtos };
        }

        public async Task<ServiceResult> SetUserMadhabAsync(int userId, int madhabId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new ServiceResult { Success = false, Message = "User not found" };

            user.PreferredMadhabID = madhabId;
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Madhab preference updated" };
        }

        public async Task<ServiceResult> GetUserMadhabAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.PreferredMadhab)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user?.PreferredMadhab == null)
                return new ServiceResult { Success = true, Message = "No madhab set", Data = null };

            var dto = new MadhabDto
            {
                MadhabID          = user.PreferredMadhab.MadhabID,
                MadhabNameEnglish = user.PreferredMadhab.MadhabNameEnglish,
                MadhabNameArabic  = user.PreferredMadhab.MadhabNameArabic,
                Description       = user.PreferredMadhab.Description
            };

            return new ServiceResult { Success = true, Message = "Madhab retrieved", Data = dto };
        }
    }

    // ========================================
    // TRANSLATION SERVICE
    // ========================================
    public class TranslationService : ITranslationService
    {
        private readonly ZakathDbContext _context;

        public TranslationService(ZakathDbContext context) { _context = context; }

        public async Task<ServiceResult> GetTranslationsAsync(string languageCode)
        {
            var list = await _context.Translations
                .Where(t => t.LanguageCode == languageCode)
                .ToListAsync();

            var dtos = list.Select(t => new TranslationDto
            {
                TranslationKey = t.TranslationKey,
                LanguageCode   = t.LanguageCode,
                TranslatedText = t.TranslatedText,
                Category       = t.Category
            }).ToList();

            return new ServiceResult { Success = true, Message = "Translations retrieved", Data = dtos };
        }

        public async Task<ServiceResult> GetTranslationByKeyAsync(string key, string languageCode)
        {
            var t = await _context.Translations
                .FirstOrDefaultAsync(x => x.TranslationKey == key && x.LanguageCode == languageCode);

            if (t == null)
                return new ServiceResult { Success = false, Message = "Translation not found" };

            return new ServiceResult
            {
                Success = true,
                Message = "Translation retrieved",
                Data    = new TranslationDto
                {
                    TranslationKey = t.TranslationKey,
                    LanguageCode   = t.LanguageCode,
                    TranslatedText = t.TranslatedText,
                    Category       = t.Category
                }
            };
        }

        public async Task<ServiceResult> AddTranslationAsync(TranslationDto dto)
        {
            _context.Translations.Add(new Translation
            {
                TranslationKey   = dto.TranslationKey,
                LanguageCode     = dto.LanguageCode,
                TranslatedText   = dto.TranslatedText,
                Category         = dto.Category,
                CreatedDate      = DateTime.Now,
                LastModifiedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Translation added" };
        }

        public async Task<ServiceResult> GetSupportedLanguagesAsync()
        {
            var list = await _context.SupportedLanguages
                .Where(l => l.IsActive)
                .OrderBy(l => l.DisplayOrder)
                .ToListAsync();

            var dtos = list.Select(l => new LanguageDto
            {
                LanguageID         = l.LanguageID,
                LanguageCode       = l.LanguageCode,
                LanguageName       = l.LanguageName,
                LanguageNameNative = l.LanguageNameNative,
                IsRTL              = l.IsRTL
            }).ToList();

            return new ServiceResult { Success = true, Message = "Languages retrieved", Data = dtos };
        }
    }

    // ========================================
    // NOTIFICATION SERVICE
    // ========================================
    public class NotificationService : INotificationService
    {
        private readonly ZakathDbContext _context;

        public NotificationService(ZakathDbContext context) { _context = context; }

        public async Task<ServiceResult> CreateNotificationAsync(int userId, string title, string message, string type)
        {
            _context.Notifications.Add(new Notification
            {
                UserID           = userId,
                Title            = title,
                Message          = message,
                NotificationType = type,
                IsRead           = false,
                IsSent           = false,
                CreatedDate      = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Notification created" };
        }

        public async Task<ServiceResult> GetUserNotificationsAsync(int userId)
        {
            var list = await _context.Notifications
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(50)
                .ToListAsync();

            var dtos = list.Select(n => new NotificationDto
            {
                NotificationID   = n.NotificationID,
                Title            = n.Title,
                Message          = n.Message,
                NotificationType = n.NotificationType,
                ScheduledDate    = n.ScheduledDate,
                SentDate         = n.SentDate,
                IsRead           = n.IsRead,
                IsSent           = n.IsSent,
                Priority         = n.Priority
            }).ToList();

            return new ServiceResult { Success = true, Message = "Notifications retrieved", Data = dtos };
        }

        public async Task<ServiceResult> MarkAsReadAsync(int notificationId)
        {
            var n = await _context.Notifications.FindAsync(notificationId);
            if (n == null)
                return new ServiceResult { Success = false, Message = "Notification not found" };

            n.IsRead = true;
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Marked as read" };
        }

        public async Task<ServiceResult> DeleteNotificationAsync(int notificationId)
        {
            var n = await _context.Notifications.FindAsync(notificationId);
            if (n == null)
                return new ServiceResult { Success = false, Message = "Notification not found" };

            _context.Notifications.Remove(n);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Notification deleted" };
        }

        public async Task SendZakathReminderAsync(int userId)
        {
            await CreateNotificationAsync(userId,
                "Zakath Reminder",
                "It is time to review and calculate your Zakath.",
                "Reminder");
        }
    }

    // ========================================
    // HIJRI DATE SERVICE
    // ========================================
    public class HijriDateService : IHijriDateService
    {
        private readonly HijriDateHelper _helper;

        public HijriDateService(HijriDateHelper helper) { _helper = helper; }

        public string ConvertToHijri(DateTime gregorianDate)                          => _helper.ConvertToHijri(gregorianDate);
        public DateTime? ConvertToGregorian(int hijriYear, int hijriMonth, int hijriDay) => _helper.ConvertToGregorian(hijriYear, hijriMonth, hijriDay);
        public bool IsHawlComplete(DateTime startDate, DateTime endDate)             => _helper.IsHawlComplete(startDate, endDate);
        public DateTime GetHawlEndDate(DateTime startDate)                           => _helper.GetHawlEndDate(startDate);
    }
}
