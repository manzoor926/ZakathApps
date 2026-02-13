using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZakathApp.API.DTOs;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    // ========================================
    // CURRENCY CONTROLLER
    // ========================================
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Unauthorized");
        }

        /// <summary>
        /// Get all supported currencies
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var result = await _currencyService.GetAllCurrenciesAsync();
            return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
        }

        /// <summary>
        /// Get user's currencies
        /// </summary>
        [HttpGet("my-currencies")]
        [Authorize]
        public async Task<IActionResult> GetUserCurrencies()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _currencyService.GetUserCurrenciesAsync(userId);
                return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Add currency to user account
        /// </summary>
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddCurrency([FromBody] AddUserCurrencyDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _currencyService.AddUserCurrencyAsync(userId, dto);
                return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Set primary currency
        /// </summary>
        [HttpPut("set-primary/{currencyId}")]
        [Authorize]
        public async Task<IActionResult> SetPrimaryCurrency(int currencyId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _currencyService.SetPrimaryCurrencyAsync(userId, currencyId);
                return Ok(new ApiResponse<object>(result.Success, result.Message, null));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Convert currency
        /// </summary>
        [HttpPost("convert")]
        [AllowAnonymous]
        public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionDto dto)
        {
            var result = await _currencyService.ConvertCurrencyAsync(dto);
            return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
        }

        /// <summary>
        /// Get exchange rate
        /// </summary>
        [HttpGet("rate/{fromCurrency}/{toCurrency}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            var rate = await _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency);
            return Ok(new ApiResponse<object>(true, "Rate retrieved", new { fromCurrency, toCurrency, rate }));
        }
    }

    // ========================================
    // MADHAB CONTROLLER
    // ========================================
    [Route("api/[controller]")]
    [ApiController]
    public class MadhabController : ControllerBase
    {
        private readonly IMadhabService _madhabService;
        private readonly ILogger<MadhabController> _logger;

        public MadhabController(IMadhabService madhabService, ILogger<MadhabController> logger)
        {
            _madhabService = madhabService;
            _logger = logger;
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Unauthorized");
        }

        /// <summary>
        /// Get all madhabs
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMadhabs()
        {
            var result = await _madhabService.GetAllMadhabsAsync();
            return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
        }

        /// <summary>
        /// Get madhab-specific zakath rules
        /// </summary>
        [HttpGet("{madhabId}/rules")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMadhabRules(int madhabId)
        {
            var result = await _madhabService.GetMadhabRulesAsync(madhabId);
            return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
        }

        /// <summary>
        /// Set user's preferred madhab
        /// </summary>
        [HttpPost("set-preference")]
        [Authorize]
        public async Task<IActionResult> SetMadhabPreference([FromBody] SetMadhabDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _madhabService.SetUserMadhabAsync(userId, dto.MadhabID);
                return Ok(new ApiResponse<object>(result.Success, result.Message, null));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Get user's madhab preference
        /// </summary>
        [HttpGet("my-preference")]
        [Authorize]
        public async Task<IActionResult> GetMyMadhab()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _madhabService.GetUserMadhabAsync(userId);
                return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }
    }

    // ========================================
    // TRANSLATION CONTROLLER
    // ========================================
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;
        private readonly ILogger<TranslationController> _logger;

        public TranslationController(ITranslationService translationService, ILogger<TranslationController> logger)
        {
            _translationService = translationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all translations for a language
        /// </summary>
        [HttpGet("{languageCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTranslations(string languageCode)
        {
            var result = await _translationService.GetTranslationsAsync(languageCode);
            return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
        }

        /// <summary>
        /// Get supported languages
        /// </summary>
        [HttpGet("languages")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSupportedLanguages()
        {
            var result = await _translationService.GetSupportedLanguagesAsync();
            return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
        }

        /// <summary>
        /// Get translation by key
        /// </summary>
        [HttpGet("{languageCode}/{key}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTranslationByKey(string languageCode, string key)
        {
            var result = await _translationService.GetTranslationByKeyAsync(key, languageCode);
            return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
        }
    }
}
