using ZakathApp.API.DTOs;

namespace ZakathApp.API.Services
{
    // ========================================
    // AUTHENTICATION SERVICE INTERFACE
    // ========================================
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<ServiceResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<ServiceResult> ForgotPasswordAsync(string mobileNumber);
        Task<ServiceResult> VerifyOtpAsync(string mobileNumber, string otp);
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<ServiceResult> UpdateProfileAsync(int userId, UserDto userDto);
    }

    // ========================================
    // ZAKATH CALCULATION SERVICE INTERFACE
    // ========================================
    public interface IZakathCalculationService
    {
        Task<ZakathCalculationResultDto> CalculateZakathAsync(int userId, string baseCurrency = "USD", int madhabId = 1);
        Task<List<ZakathCalculationResultDto>> GetCalculationHistoryAsync(int userId);
        Task<ZakathCalculationResultDto> GetCalculationByIdAsync(int calculationId);
        Task<ServiceResult> SaveCalculationAsync(ZakathCalculationResultDto calculationDto);
        Task<decimal> GetNisabThresholdAsync(string currency, int madhabId);
    }

    // ========================================
    // INCOME SERVICE INTERFACE
    // ========================================
    public interface IIncomeService
    {
        Task<ServiceResult> CreateIncomeAsync(CreateIncomeDto incomeDto);
        Task<ServiceResult> UpdateIncomeAsync(int incomeId, CreateIncomeDto incomeDto);
        Task<ServiceResult> DeleteIncomeAsync(int incomeId);
        Task<IncomeDto> GetIncomeByIdAsync(int incomeId);
        Task<List<IncomeDto>> GetUserIncomesAsync(int userId);
        Task<decimal> GetTotalIncomeAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    }

    // ========================================
    // EXPENSE SERVICE INTERFACE
    // ========================================
    public interface IExpenseService
    {
        Task<ServiceResult> CreateExpenseAsync(CreateExpenseDto expenseDto);
        Task<ServiceResult> UpdateExpenseAsync(int expenseId, CreateExpenseDto expenseDto);
        Task<ServiceResult> DeleteExpenseAsync(int expenseId);
        Task<ExpenseDto> GetExpenseByIdAsync(int expenseId);
        Task<List<ExpenseDto>> GetUserExpensesAsync(int userId);
        Task<decimal> GetTotalExpensesAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    }

    // ========================================
    // ASSET SERVICE INTERFACE
    // ========================================
    public interface IAssetService
    {
        Task<ServiceResult> CreateAssetAsync(CreateAssetDto assetDto);
        Task<ServiceResult> UpdateAssetAsync(int assetId, CreateAssetDto assetDto);
        Task<ServiceResult> DeleteAssetAsync(int assetId);
        Task<AssetDto> GetAssetByIdAsync(int assetId);
        Task<List<AssetDto>> GetUserAssetsAsync(int userId);
        Task<decimal> GetTotalAssetsValueAsync(int userId);
        Task<ServiceResult> UpdateAssetValueAsync(int assetId, decimal newValue);
    }

    // ========================================
    // CURRENCY SERVICE INTERFACE
    // ========================================
    public interface ICurrencyService
    {
        Task<ServiceResult> GetAllCurrenciesAsync();
        Task<ServiceResult> GetUserCurrenciesAsync(int userId);
        Task<ServiceResult> AddUserCurrencyAsync(int userId, AddUserCurrencyDto dto);
        Task<ServiceResult> SetPrimaryCurrencyAsync(int userId, int currencyId);
        Task<ServiceResult> ConvertCurrencyAsync(CurrencyConversionDto dto);
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        Task<ServiceResult> UpdateExchangeRatesAsync();
    }

    // ========================================
    // MADHAB SERVICE INTERFACE
    // ========================================
    public interface IMadhabService
    {
        Task<ServiceResult> GetAllMadhabsAsync();
        Task<ServiceResult> GetMadhabRulesAsync(int madhabId);
        Task<ServiceResult> SetUserMadhabAsync(int userId, int madhabId);
        Task<ServiceResult> GetUserMadhabAsync(int userId);
    }

    // ========================================
    // TRANSLATION SERVICE INTERFACE
    // ========================================
    public interface ITranslationService
    {
        Task<ServiceResult> GetTranslationsAsync(string languageCode);
        Task<ServiceResult> GetTranslationByKeyAsync(string key, string languageCode);
        Task<ServiceResult> AddTranslationAsync(TranslationDto dto);
        Task<ServiceResult> GetSupportedLanguagesAsync();
    }

    // ========================================
    // NOTIFICATION SERVICE INTERFACE
    // ========================================
    public interface INotificationService
    {
        Task<ServiceResult> CreateNotificationAsync(int userId, string title, string message, string type);
        Task<ServiceResult> GetUserNotificationsAsync(int userId);
        Task<ServiceResult> MarkAsReadAsync(int notificationId);
        Task<ServiceResult> DeleteNotificationAsync(int notificationId);
        Task SendZakathReminderAsync(int userId);
    }

    // ========================================
    // HIJRI DATE SERVICE INTERFACE
    // ========================================
    public interface IHijriDateService
    {
        string ConvertToHijri(DateTime gregorianDate);
        DateTime? ConvertToGregorian(int hijriYear, int hijriMonth, int hijriDay);
        bool IsHawlComplete(DateTime startDate, DateTime endDate);
        DateTime GetHawlEndDate(DateTime startDate);
    }
}
