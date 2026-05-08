using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Services;
using System.Security.Claims;

namespace TranHuuPhuoc_2123110236.Controllers
{
    [ApiController]
    [Route("api/customer-auth")]
    public class CustomerAuthController : ControllerBase
    {
        private readonly ICustomerAuthService _authService;
        private readonly ILogger<CustomerAuthController> _logger;

        public CustomerAuthController(ICustomerAuthService authService, ILogger<CustomerAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/customer-auth/register
        [HttpPost("register")]
        public async Task<ActionResult<CustomerDto>> Register([FromBody] CustomerRegisterRequest request)
        {
            try
            {
                var customer = await _authService.Register(request);
                return CreatedAtAction(nameof(GetProfile), new { customerId = customer.CustomerId }, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Register error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/customer-auth/login
        [HttpPost("login")]
        public async Task<ActionResult<CustomerLoginResponse>> Login([FromBody] CustomerLoginRequest request)
        {
            try
            {
                var response = await _authService.Login(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET: api/customer-auth/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<CustomerDto>> GetProfile()
        {
            try
            {
                var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Không thể xác định khách hàng" });

                var customer = await _authService.GetProfile(customerId);
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/customer-auth/change-password
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] CustomerChangePasswordRequest request)
        {
            try
            {
                var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Không thể xác định khách hàng" });

                await _authService.ChangePassword(customerId, request);
                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/customer-auth/profile
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] CustomerUpdateProfileRequest request)
        {
            try
            {
                var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Không thể xác định khách hàng" });

                await _authService.UpdateProfile(customerId, request);
                return Ok(new { message = "Cập nhật hồ sơ thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/customer-auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.ForgotPassword(request.Email);
                return Ok(new { message = "Mã OTP đã được gửi đến email của bạn" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ForgotPassword error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/customer-auth/verify-otp
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var isValid = _authService.VerifyOtp(request.Email, request.Otp);
                if (!isValid)
                    return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn" });

                return Ok(new { message = "OTP hợp lệ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/customer-auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPassword(request.Email, request.Otp, request.NewPassword);
                return Ok(new { message = "Đặt lại mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ResetPassword error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}