using Microsoft.EntityFrameworkCore;
using ZakathApp.API.Data;
using ZakathApp.API.DTOs;
using ZakathApp.API.Helpers;
using ZakathApp.API.Models;

namespace ZakathApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ZakathDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtHelper _jwtHelper;
        private readonly HijriDateHelper _hijriHelper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ZakathDbContext context,
            PasswordHasher passwordHasher,
            JwtHelper jwtHelper,
            HijriDateHelper hijriHelper,
            ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtHelper = jwtHelper;
            _hijriHelper = hijriHelper;
            _logger = logger;
        }

        public async Task<ServiceResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if mobile number already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.MobileNumber == registerDto.MobileNumber);

                if (existingUser != null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Mobile number already registered"
                    };
                }

                // Check if email already exists (if provided)
                if (!string.IsNullOrEmpty(registerDto.Email))
                {
                    var existingEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

                    if (existingEmail != null)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Email already registered"
                        };
                    }
                }

                // Hash password
                var passwordHash = _passwordHasher.HashPassword(registerDto.Password);
                var salt = _passwordHasher.GenerateSalt();

                // Create user
                var user = new User
                {
                    FullName = registerDto.FullName,
                    MobileNumber = registerDto.MobileNumber,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = salt,
                    DateOfBirth = registerDto.DateOfBirth,
                    Gender = registerDto.Gender,
                    Country = registerDto.Country,
                    State = registerDto.State,
                    City = registerDto.City,
                    HijriDateOfBirth = registerDto.DateOfBirth.HasValue ? _hijriHelper.ConvertToHijri(registerDto.DateOfBirth.Value) : null,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    PreferredLanguage = "en",
                    PreferredMadhabID = 1 // Default to Shafi'i
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Add default currency (USD)
                var defaultCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == "USD");
                if (defaultCurrency != null)
                {
                    var userCurrency = new UserCurrency
                    {
                        UserID = user.UserID,
                        CurrencyID = defaultCurrency.CurrencyID,
                        IsPrimary = true,
                        IsActive = true
                    };
                    _context.UserCurrencies.Add(userCurrency);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"New user registered: {user.FullName} ({user.MobileNumber})");

                return new ServiceResult
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = user.UserID
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Registration failed. Please try again."
                };
            }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by mobile number
                var user = await _context.Users
                    .Include(u => u.PreferredMadhab)
                    .FirstOrDefaultAsync(u => u.MobileNumber == loginDto.MobileNumber);

                if (user == null)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid mobile number or password"
                    };
                }

                // Verify password
                if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid mobile number or password"
                    };
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Account is inactive. Please contact support."
                    };
                }

                // Generate JWT token
                var token = _jwtHelper.GenerateToken(user.UserID, user.FullName, user.MobileNumber);

                // Update last login date
                user.LastLoginDate = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User logged in: {user.FullName} ({user.MobileNumber})");

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserDto
                    {
                        UserID = user.UserID,
                        FullName = user.FullName,
                        MobileNumber = user.MobileNumber,
                        Email = user.Email,
                        Gender = user.Gender,
                        DateOfBirth = user.DateOfBirth,
                        Country = user.Country,
                        PreferredLanguage = user.PreferredLanguage,
                        PreferredMadhabID = user.PreferredMadhabID,
                        ProfileImageURL = user.ProfileImageURL,
                        NotificationEnabled = user.NotificationEnabled
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Login failed. Please try again."
                };
            }
        }

        public async Task<ServiceResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Verify old password
                if (!_passwordHasher.VerifyPassword(oldPassword, user.PasswordHash))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                // Hash new password
                user.PasswordHash = _passwordHasher.HashPassword(newPassword);
                user.LastModifiedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to change password"
                };
            }
        }

        public async Task<ServiceResult> ForgotPasswordAsync(string mobileNumber)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber);

                if (user == null)
                {
                    // Don't reveal if user exists or not
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "If this mobile number is registered, you will receive an OTP"
                    };
                }

                // Generate OTP (6 digits)
                var otp = new Random().Next(100000, 999999).ToString();

                // TODO: Send OTP via SMS
                // For now, just log it
                _logger.LogInformation($"OTP for {mobileNumber}: {otp}");

                // Store OTP in database or cache with expiry
                // Implementation depends on your OTP storage strategy

                return new ServiceResult
                {
                    Success = true,
                    Message = "OTP sent successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to process request"
                };
            }
        }

        public async Task<ServiceResult> VerifyOtpAsync(string mobileNumber, string otp)
        {
            try
            {
                // TODO: Implement OTP verification logic
                // This is a placeholder implementation

                return new ServiceResult
                {
                    Success = true,
                    Message = "OTP verified successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return new ServiceResult
                {
                    Success = false,
                    Message = "OTP verification failed"
                };
            }
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.PreferredMadhab)
                    .FirstOrDefaultAsync(u => u.UserID == userId);

                if (user == null)
                    return null;

                return new UserDto
                {
                    UserID = user.UserID,
                    FullName = user.FullName,
                    MobileNumber = user.MobileNumber,
                    Email = user.Email,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Country = user.Country,
                    PreferredLanguage = user.PreferredLanguage,
                    PreferredMadhabID = user.PreferredMadhabID,
                    ProfileImageURL = user.ProfileImageURL,
                    NotificationEnabled = user.NotificationEnabled
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID");
                return null;
            }
        }

        public async Task<ServiceResult> UpdateProfileAsync(int userId, UserDto userDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                user.FullName = userDto.FullName;
                user.Email = userDto.Email;
                user.Gender = userDto.Gender;
                user.Country = userDto.Country;
                user.PreferredLanguage = userDto.PreferredLanguage;
                user.PreferredMadhabID = userDto.PreferredMadhabID;
                user.ProfileImageURL = userDto.ProfileImageURL;
                user.NotificationEnabled = userDto.NotificationEnabled;
                user.LastModifiedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = "Profile updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to update profile"
                };
            }
        }
    }
}
