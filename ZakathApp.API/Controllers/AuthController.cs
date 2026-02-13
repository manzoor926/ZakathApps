using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZakathApp.API.DTOs;
using ZakathApp.API.Services;

namespace ZakathApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid data", ModelState));
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>(false, result.Message, null));
            }

            return Ok(new ApiResponse<object>(true, result.Message, result.Data));
        }

        /// <summary>
        /// Login with mobile number and password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid credentials", ModelState));
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
            {
                return Unauthorized(new ApiResponse<object>(false, result.Message, null));
            }

            return Ok(new ApiResponse<LoginResponseDto>(true, result.Message, result));
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>(false, "User not found", null));
            }

            return Ok(new ApiResponse<UserDto>(true, "Profile retrieved successfully", user));
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto userDto)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }

            var result = await _authService.UpdateProfileAsync(userId, userDto);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>(false, result.Message, null));
            }

            return Ok(new ApiResponse<object>(true, result.Message, null));
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null));
            }

            var result = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>(false, result.Message, null));
            }

            return Ok(new ApiResponse<object>(true, result.Message, null));
        }

        /// <summary>
        /// Forgot password - Send OTP
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto.MobileNumber);
            return Ok(new ApiResponse<object>(true, result.Message, null));
        }

        /// <summary>
        /// Verify OTP
        /// </summary>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto.MobileNumber, dto.Otp);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>(false, result.Message, null));
            }

            return Ok(new ApiResponse<object>(true, result.Message, null));
        }
    }

    // Additional DTOs needed for AuthController
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ForgotPasswordDto
    {
        public string MobileNumber { get; set; }
    }

    public class VerifyOtpDto
    {
        public string MobileNumber { get; set; }
        public string Otp { get; set; }
    }
}
