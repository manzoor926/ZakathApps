using System;
using System.ComponentModel.DataAnnotations;

namespace ZakathApp.API.DTOs
{
    // ========================================
    // AUTHENTICATION DTOs
    // ========================================
    
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [Phone]
        public string MobileNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
    }

    public class LoginDto
    {
        [Required]
        public string MobileNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserDto User { get; set; }
    }

    public class UserDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Country { get; set; }
        public string PreferredLanguage { get; set; }
        public int? PreferredMadhabID { get; set; }
        public string ProfileImageURL { get; set; }
        public bool NotificationEnabled { get; set; }
    }

    // ========================================
    // ZAKATH CALCULATION DTOs
    // ========================================
    
    public class CalculateZakathDto
    {
        public int UserID { get; set; }
        public string BaseCurrency { get; set; } = "USD";
        public int MadhabID { get; set; } = 1;
        public bool IncludeNonZakathable { get; set; } = false;
    }

    public class ZakathCalculationResultDto
    {
        public int CalculationID { get; set; }
        public DateTime CalculationDate { get; set; }
        public string HijriDate { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal NetWorth { get; set; }
        public decimal NisabThreshold { get; set; }
        public bool IsZakathDue { get; set; }
        public decimal ZakathAmount { get; set; }
        public decimal ZakathPercentage { get; set; }
        public string Currency { get; set; }
        public AssetBreakdownDto AssetBreakdown { get; set; }
        public List<CurrencyConversionDto> CurrencyConversions { get; set; }
    }

    public class AssetBreakdownDto
    {
        public decimal Cash { get; set; }
        public decimal Gold { get; set; }
        public decimal Silver { get; set; }
        public decimal Investments { get; set; }
        public decimal OtherAssets { get; set; }
    }

    // ========================================
    // INCOME DTOs
    // ========================================
    
    public class CreateIncomeDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DateReceived { get; set; }

        public int? CategoryID { get; set; }
        public string SourceName { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurringFrequency { get; set; }
        public int? CurrencyID { get; set; }
        public bool IsZakathEligible { get; set; } = true;
    }

    public class IncomeDto
    {
        public int IncomeID { get; set; }
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateReceived { get; set; }
        public string HijriDateReceived { get; set; }
        public string CategoryName { get; set; }
        public string SourceName { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public bool IsZakathEligible { get; set; }
    }

    // ========================================
    // EXPENSE DTOs
    // ========================================
    
    public class CreateExpenseDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DateOfTransaction { get; set; }

        public int? CategoryID { get; set; }
        public string AccountDeductedFrom { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurringFrequency { get; set; }
        public int? CurrencyID { get; set; }
    }

    public class ExpenseDto
    {
        public int ExpenseID { get; set; }
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public string HijriDateOfTransaction { get; set; }
        public string CategoryName { get; set; }
        public string AccountDeductedFrom { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
    }

    // ========================================
    // ASSET DTOs
    // ========================================
    
    public class CreateAssetDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public string ItemName { get; set; }

        public int? ItemMasterID { get; set; }
        public int? AssetCategoryID { get; set; }

        [Required]
        public DateTime DateAcquired { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PurchaseValue { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal CurrentValue { get; set; }

        public decimal Quantity { get; set; } = 1;
        public string MeasurementUnit { get; set; }
        public bool IsZakathApplicable { get; set; } = true;
        public string Notes { get; set; }
        public int? CurrencyID { get; set; }
    }

    public class AssetDto
    {
        public int AssetID { get; set; }
        public int UserID { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public DateTime DateAcquired { get; set; }
        public string HijriDateAcquired { get; set; }
        public decimal PurchaseValue { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Quantity { get; set; }
        public string MeasurementUnit { get; set; }
        public bool IsZakathApplicable { get; set; }
        public string CategoryName { get; set; }
        public string Notes { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public DateTime? LastValuationDate { get; set; }
    }

    // ========================================
    // CURRENCY DTOs
    // ========================================
    
    public class CurrencyDto
    {
        public int CurrencyID { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public int DecimalPlaces { get; set; }
    }

    public class AddUserCurrencyDto
    {
        [Required]
        public int CurrencyID { get; set; }
        public bool IsPrimary { get; set; } = false;
    }

    public class UserCurrencyDto
    {
        public int UserCurrencyID { get; set; }
        public int CurrencyID { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class CurrencyConversionDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string FromCurrency { get; set; }

        [Required]
        public string ToCurrency { get; set; }
    }

    public class CurrencyConversionResultDto
    {
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal Rate { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public DateTime ConversionDate { get; set; }
    }

    // ========================================
    // MADHAB DTOs
    // ========================================
    
    public class MadhabDto
    {
        public int MadhabID { get; set; }
        public string MadhabNameEnglish { get; set; }
        public string MadhabNameArabic { get; set; }
        public string Description { get; set; }
    }

    public class SetMadhabDto
    {
        [Required]
        public int MadhabID { get; set; }
    }

    public class MadhabRuleDto
    {
        public int MadhabRuleID { get; set; }
        public int MadhabID { get; set; }
        public string RuleType { get; set; }
        public decimal NisabValue { get; set; }
        public decimal ZakathPercentage { get; set; }
        public int HawlPeriodDays { get; set; }
        public string SpecialConditions { get; set; }
        public string ReferenceSource { get; set; }
    }

    // ========================================
    // TRANSLATION DTOs
    // ========================================
    
    public class TranslationDto
    {
        [Required]
        public string TranslationKey { get; set; }

        [Required]
        public string LanguageCode { get; set; }

        [Required]
        public string TranslatedText { get; set; }

        public string Category { get; set; }
    }

    public class LanguageDto
    {
        public int LanguageID { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string LanguageNameNative { get; set; }
        public bool IsRTL { get; set; }
    }

    // ========================================
    // PAYMENT DTOs
    // ========================================
    
    public class CreatePaymentDto
    {
        [Required]
        public int UserID { get; set; }

        public int? CalculationID { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public string RecipientName { get; set; }
        public string RecipientCategory { get; set; }
        public string RecipientContact { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public string Notes { get; set; }
    }

    public class PaymentDto
    {
        public int PaymentID { get; set; }
        public int UserID { get; set; }
        public int? CalculationID { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string HijriPaymentDate { get; set; }
        public string RecipientName { get; set; }
        public string RecipientCategory { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public bool IsVerified { get; set; }
    }

    // ========================================
    // NOTIFICATION DTOs
    // ========================================
    
    public class NotificationDto
    {
        public int NotificationID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public bool IsRead { get; set; }
        public bool IsSent { get; set; }
        public string Priority { get; set; }
    }

    // ========================================
    // COMMON DTOs
    // ========================================
    
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public List<string> Errors { get; set; }

        public ServiceResult()
        {
            Errors = new List<string>();
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // ========================================
    // DASHBOARD DTOs
    // ========================================
    
    public class DashboardStatsDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal NetWorth { get; set; }
        public decimal ZakathDue { get; set; }
        public decimal ZakathPaid { get; set; }
        public bool IsZakathDue { get; set; }
        public int DaysUntilHawl { get; set; }
        public string PrimaryCurrency { get; set; }
        public DateTime LastCalculationDate { get; set; }
    }

    public class RecentTransactionDto
    {
        public string Type { get; set; } // Income, Expense, Asset
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string CurrencySymbol { get; set; }
    }
}
