using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Services;
using System.Security.Claims;

namespace TranHuuPhuoc_2123110236.Controllers
{
    [ApiController]
    [Route("api/employee-auth")]
    public class EmployeeAuthController : ControllerBase
    {
        private readonly IEmployeeAuthService _authService;
        private readonly ILogger<EmployeeAuthController> _logger;

        public EmployeeAuthController(IEmployeeAuthService authService, ILogger<EmployeeAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/employee-auth/register (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<ActionResult<EmployeeDto>> Register([FromBody] EmployeeRegisterRequest request)
        {
            try
            {
                var employee = await _authService.Register(request);
                return CreatedAtAction(nameof(GetProfile), new { employeeId = employee.EmployeeId }, employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Register error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/employee-auth/login
        [HttpPost("login")]
        public async Task<ActionResult<EmployeeLoginResponse>> Login([FromBody] EmployeeLoginRequest request)
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

        // GET: api/employee-auth/profile
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("profile")]
        public async Task<ActionResult<EmployeeDto>> GetProfile()
        {
            try
            {
                var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(employeeId))
                    return Unauthorized(new { message = "Không thể xác định nhân viên" });

                var employee = await _authService.GetProfile(employeeId);
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/employee-auth/change-password
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] EmployeeChangePasswordRequest request)
        {
            try
            {
                var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(employeeId))
                    return Unauthorized(new { message = "Không thể xác định nhân viên" });

                await _authService.ChangePassword(employeeId, request);
                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/employee-auth/users/{employeeId}/role (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("users/{employeeId}/role")]
        public async Task<IActionResult> UpdateEmployeeRole(string employeeId, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                await _authService.UpdateEmployeeRole(employeeId, request.NewRole);
                return Ok(new { message = $"Cập nhật quyền của {employeeId} thành {request.NewRole} thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/employee-auth/users (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<List<EmployeeDto>>> GetAllEmployees()
        {
            try
            {
                var employees = await _authService.GetAllEmployees();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}