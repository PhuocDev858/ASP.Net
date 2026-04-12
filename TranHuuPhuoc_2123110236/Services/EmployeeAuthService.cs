using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TranHuuPhuoc_2123110236.Services
{
    public class EmployeeAuthService : IEmployeeAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeAuthService> _logger;

        public EmployeeAuthService(AppDbContext context, IConfiguration configuration, ILogger<EmployeeAuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // Đăng kí (chỉ Admin gọi)
        public async Task<EmployeeDto> Register(EmployeeRegisterRequest request)
        {
            try
            {
                // ❌ Xóa validation EmployeeId

                if (string.IsNullOrWhiteSpace(request.FullName))
                    throw new Exception("Tên đầy đủ không được để trống");

                if (string.IsNullOrWhiteSpace(request.Email))
                    throw new Exception("Email không được để trống");

                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                    throw new Exception("Số điện thoại không được để trống");

                if (string.IsNullOrWhiteSpace(request.Address))
                    throw new Exception("Địa chỉ không được để trống");

                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                    throw new Exception("Mật khẩu phải có ít nhất 6 ký tự");

                if (request.Password != request.ConfirmPassword)
                    throw new Exception("Mật khẩu xác nhận không khớp");

                // ❌ Xóa check duplicate EmployeeId

                var existingEmail = await _context.Employee.FirstOrDefaultAsync(e => e.Email == request.Email);
                if (existingEmail != null)
                    throw new Exception("Email đã được đăng kí");

                // ✅ Tự sinh EmployeeId
                var employeeId = GenerateEmployeeId();

                var employee = new Employee
                {
                    EmployeeId = employeeId,  // ← Tự sinh
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    Department = request.Department,  // ✅ Thêm
                    Salary = request.Salary,          // ✅ Thêm
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = "Staff",  // Mặc định là Staff
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Employee.Add(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nhân viên {employeeId} đã được tạo thành công");

                return MapToEmployeeDto(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đăng kí nhân viên: {ex.Message}");
                throw new Exception("Lỗi khi đăng kí nhân viên: " + ex.Message);
            }
        }

        // Đăng nhập
        public async Task<EmployeeLoginResponse> Login(EmployeeLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    throw new Exception("Email và mật khẩu không được để trống");

                var employee = await _context.Employee.FirstOrDefaultAsync(e => e.Email == request.Email);

                if (employee == null)
                    throw new Exception("Email hoặc mật khẩu không chính xác");

                if (!employee.IsActive)
                    throw new Exception("Tài khoản của bạn đã bị vô hiệu hóa");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash))
                    throw new Exception("Email ho���c mật khẩu không chính xác");

                var token = GenerateJwtToken(employee);

                _logger.LogInformation($"Nhân viên {employee.EmployeeId} đã đăng nhập");

                return new EmployeeLoginResponse
                {
                    Token = token,
                    Employee = MapToEmployeeDto(employee),
                    Message = "Đăng nhập thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đăng nhập nhân viên: {ex.Message}");
                throw new Exception("Lỗi khi đăng nhập: " + ex.Message);
            }
        }

        // Tạo JWT Token
        public string GenerateJwtToken(Employee employee)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"];
            var jwtExpireMinutes = int.Parse(_configuration["JwtSettings:ExpiresIn"] ?? "60");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId),
                    new Claim(ClaimTypes.Email, employee.Email),
                    new Claim(ClaimTypes.Name, employee.FullName),
                    new Claim(ClaimTypes.Role, employee.Role),
                    new Claim("UserType", "Employee"),
                    new Claim("IsActive", employee.IsActive.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(jwtExpireMinutes),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Đổi mật khẩu
        public async Task<bool> ChangePassword(string employeeId, EmployeeChangePasswordRequest request)
        {
            try
            {
                if (request.NewPassword != request.ConfirmPassword)
                    throw new Exception("Mật khẩu xác nhận không khớp");

                if (request.NewPassword.Length < 6)
                    throw new Exception("Mật khẩu mới phải có ít nhất 6 ký tự");

                var employee = await _context.Employee.FindAsync(employeeId);
                if (employee == null)
                    throw new Exception("Nhân viên không tồn tại");

                if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, employee.PasswordHash))
                    throw new Exception("Mật khẩu cũ không chính xác");

                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                employee.UpdatedAt = DateTime.Now;

                _context.Employee.Update(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nhân viên {employeeId} đã đổi mật khẩu");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đổi mật khẩu: {ex.Message}");
                throw new Exception("Lỗi khi đổi mật khẩu: " + ex.Message);
            }
        }

        // Lấy profil
        public async Task<EmployeeDto> GetProfile(string employeeId)
        {
            try
            {
                var employee = await _context.Employee.FindAsync(employeeId);
                if (employee == null)
                    throw new Exception("Nhân viên không tồn tại");

                return MapToEmployeeDto(employee);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy profil: " + ex.Message);
            }
        }

        // Cập nhật profil - ✅ Thêm method mới
        public async Task<bool> UpdateProfile(string employeeId, EmployeeUpdateProfileRequest request)
        {
            try
            {
                var employee = await _context.Employee.FindAsync(employeeId);
                if (employee == null)
                    throw new Exception("Nhân viên không tồn tại");

                employee.FullName = request.FullName;
                employee.PhoneNumber = request.PhoneNumber;
                employee.Address = request.Address;
                employee.Department = request.Department;
                employee.Salary = request.Salary;
                employee.UpdatedAt = DateTime.Now;

                _context.Employee.Update(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nhân viên {employeeId} đã cập nhật hồ sơ");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật hồ sơ: {ex.Message}");
                throw new Exception("Lỗi khi cập nhật hồ sơ: " + ex.Message);
            }
        }

        // Cập nhật role (Admin only)
        public async Task<bool> UpdateEmployeeRole(string employeeId, string newRole)
        {
            try
            {
                if (newRole != "Admin" && newRole != "Staff")
                    throw new Exception("Quyền không hợp lệ (Admin hoặc Staff)");

                var employee = await _context.Employee.FindAsync(employeeId);
                if (employee == null)
                    throw new Exception("Nhân viên không tồn tại");

                employee.Role = newRole;
                employee.UpdatedAt = DateTime.Now;

                _context.Employee.Update(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Quyền của {employeeId} đã cập nhật thành {newRole}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật quyền: {ex.Message}");
                throw new Exception("Lỗi khi cập nhật quyền: " + ex.Message);
            }
        }

        // Lấy danh sách nhân viên (Admin only)
        public async Task<List<EmployeeDto>> GetAllEmployees()
        {
            try
            {
                var employees = await _context.Employee.ToListAsync();
                return employees.Select(MapToEmployeeDto).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách nhân viên: " + ex.Message);
            }
        }

        // ✅ Helper methods
        private string GenerateEmployeeId()
        {
            return "EMP" + DateTime.Now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }

        private EmployeeDto MapToEmployeeDto(Employee employee)
        {
            return new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Address = employee.Address,
                Department = employee.Department,  // ✅ Thêm
                Salary = employee.Salary,          // ✅ Thêm
                Role = employee.Role,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt
            };
        }
    }
}