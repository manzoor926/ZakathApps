using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZakathApp.API.DTOs;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    // ========================================
    // INCOME CONTROLLER
    // ========================================
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController : ControllerBase
    {
        private readonly IIncomeService _incomeService;
        private readonly ILogger<IncomeController> _logger;

        public IncomeController(IIncomeService incomeService, ILogger<IncomeController> logger)
        {
            _incomeService = incomeService;
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
        /// Get all income records for current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var incomes = await _incomeService.GetUserIncomesAsync(userId);
                return Ok(new ApiResponse<List<IncomeDto>>(true, "Incomes retrieved", incomes));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Get income by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var income = await _incomeService.GetIncomeByIdAsync(id);
            if (income == null)
            {
                return NotFound(new ApiResponse<object>(false, "Income not found", null));
            }
            return Ok(new ApiResponse<IncomeDto>(true, "Income retrieved", income));
        }

        /// <summary>
        /// Create new income record
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIncomeDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                dto.UserID = userId;
                var result = await _incomeService.CreateIncomeAsync(dto);
                return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Update income record
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateIncomeDto dto)
        {
            var result = await _incomeService.UpdateIncomeAsync(id, dto);
            return Ok(new ApiResponse<object>(result.Success, result.Message, null));
        }

        /// <summary>
        /// Delete income record
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _incomeService.DeleteIncomeAsync(id);
            return Ok(new ApiResponse<object>(result.Success, result.Message, null));
        }

        /// <summary>
        /// Get total income for user
        /// </summary>
        [HttpGet("total")]
        public async Task<IActionResult> GetTotal([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var total = await _incomeService.GetTotalIncomeAsync(userId, startDate, endDate);
                return Ok(new ApiResponse<object>(true, "Total calculated", new { total, startDate, endDate }));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }
    }

    // ========================================
    // EXPENSE CONTROLLER
    // ========================================
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IExpenseService expenseService, ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService;
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
        /// Get all expense records for current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var expenses = await _expenseService.GetUserExpensesAsync(userId);
                return Ok(new ApiResponse<List<ExpenseDto>>(true, "Expenses retrieved", expenses));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Get expense by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
            {
                return NotFound(new ApiResponse<object>(false, "Expense not found", null));
            }
            return Ok(new ApiResponse<ExpenseDto>(true, "Expense retrieved", expense));
        }

        /// <summary>
        /// Create new expense record
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                dto.UserID = userId;
                var result = await _expenseService.CreateExpenseAsync(dto);
                return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Update expense record
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateExpenseDto dto)
        {
            var result = await _expenseService.UpdateExpenseAsync(id, dto);
            return Ok(new ApiResponse<object>(result.Success, result.Message, null));
        }

        /// <summary>
        /// Delete expense record
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _expenseService.DeleteExpenseAsync(id);
            return Ok(new ApiResponse<object>(result.Success, result.Message, null));
        }

        /// <summary>
        /// Get total expenses for user
        /// </summary>
        [HttpGet("total")]
        public async Task<IActionResult> GetTotal([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var total = await _expenseService.GetTotalExpensesAsync(userId, startDate, endDate);
                return Ok(new ApiResponse<object>(true, "Total calculated", new { total, startDate, endDate }));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }
    }

    // ========================================
    // ASSET CONTROLLER
    // ========================================
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly ILogger<AssetController> _logger;

        public AssetController(IAssetService assetService, ILogger<AssetController> logger)
        {
            _assetService = assetService;
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
        /// Get all assets for current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var assets = await _assetService.GetUserAssetsAsync(userId);
                return Ok(new ApiResponse<List<AssetDto>>(true, "Assets retrieved", assets));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Get asset by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                return NotFound(new ApiResponse<object>(false, "Asset not found", null));
            }
            return Ok(new ApiResponse<AssetDto>(true, "Asset retrieved", asset));
        }

        /// <summary>
        /// Create new asset
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAssetDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                dto.UserID = userId;
                var result = await _assetService.CreateAssetAsync(dto);
                return Ok(new ApiResponse<object>(result.Success, result.Message, result.Data));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Update asset
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateAssetDto dto)
        {
            var result = await _assetService.UpdateAssetAsync(id, dto);
            return Ok(new ApiResponse<object>(result.Success, result.Message, null));
        }

        /// <summary>
        /// Delete asset
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _assetService.DeleteAssetAsync(id);
            return Ok(new ApiResponse<object>(result.Success, result.Message, null));
        }

        /// <summary>
        /// Get total assets value for user
        /// </summary>
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalValue()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var total = await _assetService.GetTotalAssetsValueAsync(userId);
                return Ok(new ApiResponse<object>(true, "Total calculated", new { total }));
            }
            catch
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }
        }

        /// <summary>
        /// Update asset value
        /// </summary>
        [HttpPatch("{id}/value")]
        public async Task<IActionResult> UpdateValue(int id, [FromBody] UpdateAssetValueDto dto)
        {
            var result = await _assetService.UpdateAssetValueAsync(id, dto.NewValue);
            return Ok(new ApiResponse<object>(result.Success, result.Message, null));
        }
    }

    // Additional DTO
    public class UpdateAssetValueDto
    {
        public decimal NewValue { get; set; }
    }
}
