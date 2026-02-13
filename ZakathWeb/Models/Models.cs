namespace ZakathApp.Web.Models;

// Auth Models
public class LoginRequest
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class UserModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PrimaryCurrency { get; set; }
}

// Dashboard Models
public class DashboardStats
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalAssetValue { get; set; }
    public decimal NetWorth { get; set; }
    public decimal CurrentZakathDue { get; set; }
    public decimal TotalZakathPaid { get; set; }
    public int TotalAssets { get; set; }
    public string PrimaryCurrency { get; set; } = "USD";
}

public class RecentTransaction
{
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? CurrencySymbol { get; set; }
}

// Income/Expense Models
public class IncomeModel
{
    public int IncomeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime IncomeDate { get; set; }
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class ExpenseModel
{
    public int ExpenseId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class IncomeRequest
{
    public decimal Amount { get; set; }
    public DateTime IncomeDate { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class ExpenseRequest
{
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

// Asset Models
public class AssetModel
{
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? CategoryName { get; set; }
    public bool IsZakathApplicable { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class AssetRequest
{
    public string AssetName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int AssetCategoryId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

// Zakath Models
public class ZakathResult
{
    public int CalculationId { get; set; }
    public DateTime CalculationDate { get; set; }
    public decimal TotalWealth { get; set; }
    public decimal NisabThreshold { get; set; }
    public decimal ZakathDue { get; set; }
    public bool IsZakathApplicable { get; set; }
}

public class CalculateZakathRequest
{
    public DateTime CalculationDate { get; set; } = DateTime.Now;
}

// Payment Models
public class PaymentModel
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public string? RecipientName { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public string? RecipientName { get; set; }
    public int? CalculationId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

// Category Models
public class CategoryModel
{
    public int CategoryId { get; set; }
    public string? CategoryNameEnglish { get; set; }
    public string? CategoryNameArabic { get; set; }
    public string? IconName { get; set; }
    public string? ColorCode { get; set; }
}

public class AssetCategoryModel
{
    public int AssetCategoryId { get; set; }
    public string? CategoryNameEnglish { get; set; }
    public string? CategoryNameArabic { get; set; }
    public bool? ZakathApplicable { get; set; }
    public bool? IsValuationRequired { get; set; }
    public string? IconName { get; set; }
    public string? Description { get; set; }
}

// API Response wrapper
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string? Token { get; set; }
    public UserModel? User { get; set; }
}