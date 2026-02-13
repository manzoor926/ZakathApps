using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZakathApp.API.Models
{
    // =============================================
    // 1. USER MODEL
    // =============================================
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(150)]
        public string? Area { get; set; }

        [MaxLength(150)]
        public string? Place { get; set; }

        [Required]
        [MaxLength(20)]
        public string MobileNumber { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? SocialMediaAccount { get; set; }

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(500)]
        public string PasswordSalt { get; set; }

        public bool IsActive { get; set; } = true;
        public bool EmailVerified { get; set; } = false;
        public bool MobileVerified { get; set; } = false;

        [MaxLength(500)]
        public string? ProfileImageURL { get; set; }

        [MaxLength(10)]
        public string? PreferredLanguage { get; set; } = "en";

        [MaxLength(50)]
        public string? HijriDateOfBirth { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public bool NotificationEnabled { get; set; } = true;
        public int ReminderDaysBefore { get; set; } = 30;

        // NEW: Madhab preference
        public int? PreferredMadhabID { get; set; }

        // Navigation Properties
        [ForeignKey("PreferredMadhabID")]
        public virtual Madhab PreferredMadhab { get; set; }

        public virtual ICollection<IncomeDetail> Incomes { get; set; }
        public virtual ICollection<ExpenseDetail> Expenses { get; set; }
        public virtual ICollection<CurrentAsset> Assets { get; set; }
        public virtual ICollection<ZakathCalculation> ZakathCalculations { get; set; }
        public virtual ICollection<ZakathPayment> ZakathPayments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<UserCurrency> UserCurrencies { get; set; }
    }

    // =============================================
    // 2. CATEGORY MASTER
    // =============================================
    [Table("CategoryMaster")]
    public class CategoryMaster
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(200)]
        public string? CategoryNameArabic { get; set; }

        [Required]
        [MaxLength(200)]
        public string? CategoryNameEnglish { get; set; }

        [Required]
        [MaxLength(20)]
        public string? CategoryType { get; set; }

        [MaxLength(100)]
        public string? IconName { get; set; }

        [MaxLength(20)]
        public string? ColorCode { get; set; }

        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    // =============================================
    // 3. ASSET CATEGORY MASTER
    // =============================================
    [Table("AssetCategoryMaster")]
    public class AssetCategoryMaster
    {
        [Key]
        public int AssetCategoryID { get; set; }

        [Required]
        [MaxLength(200)]
        public string? CategoryNameArabic { get; set; }

        [Required]
        [MaxLength(200)]
        public string? CategoryNameEnglish { get; set; }

        public bool ZakathApplicable { get; set; } = true;
        public bool IsValuationRequired { get; set; } = true;

        [MaxLength(100)]
        public string? IconName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<ItemMaster> Items { get; set; }
        public virtual ICollection<CurrentAsset> Assets { get; set; }
    }

    // =============================================
    // 4. ITEM MASTER
    // =============================================
    [Table("ItemMaster")]
    public class ItemMaster
    {
        [Key]
        public int ItemID { get; set; }

        [Required]
        [MaxLength(200)]
        public string? ItemNameArabic { get; set; }

        [Required]
        [MaxLength(200)]
        public string? ItemNameEnglish { get; set; }

        public int? AssetCategoryID { get; set; }

        [MaxLength(50)]
        public string? MeasurementUnit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentMarketValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? NisabThreshold { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal ZakathPercentage { get; set; } = 2.5m;

        public bool IsActive { get; set; } = true;
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;
        public bool AutoUpdateValue { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("AssetCategoryID")]
        public virtual AssetCategoryMaster AssetCategory { get; set; }
    }

    // =============================================
    // 5. ZAKATH MASTER
    // =============================================
    [Table("ZakathMaster")]
    public class ZakathMaster
    {
        [Key]
        public int ZakathRuleID { get; set; }

        [Required]
        [MaxLength(300)]
        public string? RuleNameArabic { get; set; }

        [Required]
        [MaxLength(300)]
        public string? RuleNameEnglish { get; set; }

        [Required]
        [MaxLength(50)]
        public string? RuleType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NisabValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ZakathPercentage { get; set; } = 2.5m;

        public int HawlPeriodDays { get; set; } = 354;

        public string? DescriptionArabic { get; set; }
        public string? DescriptionEnglish { get; set; }

        [MaxLength(500)]
        public string? ReferenceVerse { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFromDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    }

    // =============================================
    // 6. INCOME DETAILS
    // =============================================
    [Table("IncomeDetails")]
    public class IncomeDetail
    {
        [Key]
        public int IncomeID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DateReceived { get; set; }

        [MaxLength(50)]
        public string? HijriDateReceived { get; set; }

        public int? CategoryID { get; set; }

        [MaxLength(200)]
        public string? SourceName { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsRecurring { get; set; } = false;

        [MaxLength(20)]
        public string? RecurringFrequency { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "SAR";

        public bool IsZakathEligible { get; set; } = true;

        // NEW: Currency support
        public int? CurrencyID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("CategoryID")]
        public virtual CategoryMaster Category { get; set; }

        [ForeignKey("CurrencyID")]
        public virtual Currency CurrencyEntity { get; set; }
    }

    // =============================================
    // 7. EXPENSE DETAILS
    // =============================================
    [Table("ExpenseDetails")]
    public class ExpenseDetail
    {
        [Key]
        public int ExpenseID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? AccountDeductedFrom { get; set; }

        [Required]
        public DateTime DateOfTransaction { get; set; }

        [MaxLength(50)]
        public string? HijriDateOfTransaction { get; set; }

        public int? CategoryID { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsRecurring { get; set; } = false;

        [MaxLength(20)]
        public string? RecurringFrequency { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "SAR";

        [MaxLength(500)]
        public string? ReceiptImageURL { get; set; }

        // NEW: Currency support
        public int? CurrencyID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("CategoryID")]
        public virtual CategoryMaster Category { get; set; }

        [ForeignKey("CurrencyID")]
        public virtual Currency CurrencyEntity { get; set; }
    }

    // =============================================
    // 8. CURRENT ASSETS
    // =============================================
    [Table("CurrentAssets")]
    public class CurrentAsset
    {
        [Key]
        public int AssetID { get; set; }

        [Required]
        public int UserID { get; set; }

        [MaxLength(50)]
        public string? ItemNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string? ItemName { get; set; }

        public int? ItemMasterID { get; set; }
        public int? AssetCategoryID { get; set; }

        [Required]
        public DateTime DateAcquired { get; set; }

        [MaxLength(50)]
        public string? HijriDateAcquired { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentValue { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; } = 1;

        [MaxLength(50)]
        public string? MeasurementUnit { get; set; }

        public bool IsZakathApplicable { get; set; } = true;
        public DateTime? LastValuationDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ImageURL { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "SAR";

        // NEW: Currency support
        public int? CurrencyID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("ItemMasterID")]
        public virtual ItemMaster ItemMaster { get; set; }

        [ForeignKey("AssetCategoryID")]
        public virtual AssetCategoryMaster AssetCategory { get; set; }

        [ForeignKey("CurrencyID")]
        public virtual Currency CurrencyEntity { get; set; }

        public virtual ICollection<AssetZakathEligibility> AssetZakathEligibilities { get; set; }
    }

    // =============================================
    // 9. ZAKATH CALCULATION
    // =============================================
    [Table("ZakathCalculation")]
    public class ZakathCalculation
    {
        [Key]
        public int CalculationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public DateTime CalculationDate { get; set; }

        [MaxLength(50)]
        public string? HijriCalculationDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAssets { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalLiabilities { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NisabThreshold { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ZakathAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal ZakathPercentage { get; set; } = 2.5m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CashAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GoldAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SilverAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InvestmentAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAssetsAmount { get; set; } = 0;

        public string? CalculationNotes { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "SAR";

        // NEW: Currency support
        public int? BaseCurrencyID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("BaseCurrencyID")]
        public virtual Currency BaseCurrency { get; set; }
    }

    // =============================================
    // 10. ZAKATH PAYMENTS
    // =============================================
    [Table("ZakathPayments")]
    public class ZakathPayment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int UserID { get; set; }

        public int? CalculationID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [MaxLength(50)]
        public string? HijriPaymentDate { get; set; }

        [MaxLength(200)]
        public string? RecipientName { get; set; }

        [MaxLength(100)]
        public string? RecipientCategory { get; set; }

        [MaxLength(100)]
        public string? RecipientContact { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ReceiptImageURL { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "SAR";

        public bool IsVerified { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("CalculationID")]
        public virtual ZakathCalculation ZakathCalculation { get; set; }
    }

    // =============================================
    // 11. ZAKATH RECIPIENTS
    // =============================================
    [Table("ZakathRecipients")]
    public class ZakathRecipient
    {
        [Key]
        public int RecipientID { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        public int? Age { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        public int? FamilyMembers { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyIncome { get; set; }

        [MaxLength(50)]
        public string? VerificationStatus { get; set; } = "Pending";

        public int? VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    // =============================================
    // 12. NOTIFICATIONS
    // =============================================
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? Message { get; set; }

        [MaxLength(50)]
        public string? NotificationType { get; set; }

        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsSent { get; set; } = false;

        [MaxLength(20)]
        public string? Priority { get; set; } = "Normal";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    // =============================================
    // 13. APP SETTINGS
    // =============================================
    [Table("AppSettings")]
    public class AppSetting
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; }

        [MaxLength(500)]
        public string? SettingValueArabic { get; set; }

        [MaxLength(500)]
        public string? SettingValueEnglish { get; set; }

        [MaxLength(50)]
        public string? SettingDataType { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsUserModifiable { get; set; } = false;
        public int? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    // =============================================
    // 14. AUDIT LOG
    // =============================================
    [Table("AuditLog")]
    public class AuditLog
    {
        [Key]
        public int AuditID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; }

        [MaxLength(100)]
        public string TableName { get; set; }

        public int? RecordID { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        [MaxLength(200)]
        public string? DeviceInfo { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;
    }

    // =============================================
    // 15. MARKET VALUE UPDATES
    // =============================================
    [Table("MarketValueUpdates")]
    public class MarketValueUpdate
    {
        [Key]
        public int UpdateID { get; set; }

        public int? ItemMasterID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PreviousValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NewValue { get; set; }

        public DateTime UpdateDate { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? Source { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "SAR";

        // Navigation
        [ForeignKey("ItemMasterID")]
        public virtual ItemMaster ItemMaster { get; set; }
    }

    // =============================================
    // 16. CURRENCY (NEW)
    // =============================================
    [Table("Currencies")]
    public class Currency
    {
        [Key]
        public int CurrencyID { get; set; }

        [Required]
        [MaxLength(10)]
        public string CurrencyCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string? CurrencyName { get; set; }

        [Required]
        [MaxLength(10)]
        public string? CurrencySymbol { get; set; }

        public int DecimalPlaces { get; set; } = 2;
        public bool IsActive { get; set; } = true;
        public int? DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<UserCurrency> UserCurrencies { get; set; }
    }

    // =============================================
    // 17. USER CURRENCY (NEW)
    // =============================================
    [Table("UserCurrencies")]
    public class UserCurrency
    {
        [Key]
        public int UserCurrencyID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int CurrencyID { get; set; }

        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("CurrencyID")]
        public virtual Currency Currency { get; set; }
    }

    // =============================================
    // 18. CURRENCY EXCHANGE RATE (NEW)
    // =============================================
    [Table("CurrencyExchangeRates")]
    public class CurrencyExchangeRate
    {
        [Key]
        public int ExchangeRateID { get; set; }

        public int FromCurrencyID { get; set; }
        public int ToCurrencyID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal Rate { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? Source { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("FromCurrencyID")]
        public virtual Currency FromCurrency { get; set; }

        [ForeignKey("ToCurrencyID")]
        public virtual Currency ToCurrency { get; set; }
    }

    // =============================================
    // 19. SUPPORTED LANGUAGE (NEW)
    // =============================================
    [Table("SupportedLanguages")]
    public class SupportedLanguage
    {
        [Key]
        public int LanguageID { get; set; }

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string? LanguageName { get; set; }

        [Required]
        [MaxLength(100)]
        public string? LanguageNameNative { get; set; }

        public bool IsRTL { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int? DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    // =============================================
    // 20. TRANSLATION (NEW)
    // =============================================
    [Table("Translations")]
    public class Translation
    {
        [Key]
        public int TranslationID { get; set; }

        [Required]
        [MaxLength(200)]
        public string TranslationKey { get; set; }

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; }

        [Required]
        public string? TranslatedText { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    }

    // =============================================
    // 21. MADHAB (NEW)
    // =============================================
    [Table("Madhabs")]
    public class Madhab
    {
        [Key]
        public int MadhabID { get; set; }

        [Required]
        [MaxLength(100)]
        public string? MadhabNameEnglish { get; set; }

        [Required]
        [MaxLength(100)]
        public string? MadhabNameArabic { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int? DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<MadhabZakathRule> MadhabZakathRules { get; set; }
    }

    // =============================================
    // 22. MADHAB ZAKATH RULE (NEW)
    // =============================================
    [Table("MadhabZakathRules")]
    public class MadhabZakathRule
    {
        [Key]
        public int MadhabRuleID { get; set; }

        public int MadhabID { get; set; }

        [Required]
        [MaxLength(50)]
        public string? RuleType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NisabValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ZakathPercentage { get; set; }

        public int HawlPeriodDays { get; set; } = 354;
        public string? SpecialConditions { get; set; }

        [MaxLength(500)]
        public string? ReferenceSource { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("MadhabID")]
        public virtual Madhab Madhab { get; set; }
    }

    // =============================================
    // 23. ASSET ZAKATH ELIGIBILITY (NEW)
    // =============================================
    [Table("AssetZakathEligibility")]
    public class AssetZakathEligibility
    {
        [Key]
        public int EligibilityID { get; set; }

        [Required]
        public int AssetID { get; set; }

        public bool IsZakathable { get; set; } = true;
        public string? Reason { get; set; }
        public int? MadhabID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("AssetID")]
        public virtual CurrentAsset CurrentAsset { get; set; }

        [ForeignKey("MadhabID")]
        public virtual Madhab Madhab { get; set; }
    }
}
