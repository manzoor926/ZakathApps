using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZakathApp.API.DTOs;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ZakathController : ControllerBase
    {
        private readonly IZakathCalculationService _zakathService;
        private readonly ILogger<ZakathController> _logger;

        public ZakathController(IZakathCalculationService zakathService, ILogger<ZakathController> logger)
        {
            _zakathService = zakathService;
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
        /// Calculate Zakath for current user
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateZakath([FromBody] CalculateZakathDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _zakathService.CalculateZakathAsync(userId, dto.BaseCurrency, dto.MadhabID);

                return Ok(new ApiResponse<ZakathCalculationResultDto>(true, "Zakath calculated successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating zakath");
                return StatusCode(500, new ApiResponse<object>(false, "Error calculating zakath", null));
            }
        }

        /// <summary>
        /// Get zakath calculation history
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var history = await _zakathService.GetCalculationHistoryAsync(userId);

                return Ok(new ApiResponse<List<ZakathCalculationResultDto>>(true, "History retrieved", history));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history");
                return StatusCode(500, new ApiResponse<object>(false, "Error retrieving history", null));
            }
        }

        /// <summary>
        /// Get specific calculation by ID
        /// </summary>
        [HttpGet("{calculationId}")]
        public async Task<IActionResult> GetCalculation(int calculationId)
        {
            try
            {
                var result = await _zakathService.GetCalculationByIdAsync(calculationId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<object>(false, "Calculation not found", null));
                }

                return Ok(new ApiResponse<ZakathCalculationResultDto>(true, "Calculation retrieved", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving calculation");
                return StatusCode(500, new ApiResponse<object>(false, "Error retrieving calculation", null));
            }
        }

        /// <summary>
        /// Get Nisab threshold for a currency
        /// </summary>
        [HttpGet("nisab/{currency}")]
        public async Task<IActionResult> GetNisab(string currency, [FromQuery] int madhabId = 1)
        {
            try
            {
                var nisab = await _zakathService.GetNisabThresholdAsync(currency, madhabId);

                return Ok(new ApiResponse<object>(true, "Nisab retrieved", new { nisab, currency, madhabId }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nisab");
                return StatusCode(500, new ApiResponse<object>(false, "Error retrieving nisab", null));
            }
        }
    }
}
