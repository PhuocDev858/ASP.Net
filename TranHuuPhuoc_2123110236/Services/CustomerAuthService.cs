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
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomerAuthService> _logger;

        public CustomerAuthService(AppDbContext context, IConfiguration configuration, ILogger<CustomerAuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // Đăng kí khách hàng
        public async Task<CustomerDto> Register(CustomerRegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CustomerId))
                    throw new Exception("ID khách hàng không được để trống");

                if (string.IsNullOrWhiteSpace(request.FullName))
                    throw new Exception("Tên đầy đủ không được để trống");

                if (string.IsNullOrWhiteSpace(request.Email))
                    throw new Exception("Email không được để trống");

                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                    throw new Exception("Mật khẩu phải có ít nhất 6 ký tự");

                if (request.Password != request.ConfirmPassword)
                    throw new Exception("Mật khẩu xác nhận không khớp");

                var existingId = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);
                if (existingId != null)
                    throw new Exception("ID khách hàng đã tồn tại");

                var existingEmail = await _context.Customer.FirstOrDefaultAsync(c => c.Email == request.Email);
                if (existingEmail != null)
                    throw new Exception("Email đã được đăng kí");

                var customer = new Customer
                {
                    CustomerId = request.CustomerId,
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    TotalSpent = 0,
                    TotalOrders = 0
                };

                _context.Customer.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Khách hàng {request.CustomerId} đã đăng kí");

                return new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    Address = customer.Address,
                    TotalSpent = customer.TotalSpent,
                    TotalOrders = customer.TotalOrders,
                    IsActive = customer.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đăng kí khách hàng: {ex.Message}");
                throw new Exception("Lỗi khi đăng kí: " + ex.Message);
            }
        }

        // Đăng nhập khách hàng
        public async Task<CustomerLoginResponse> Login(CustomerLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    throw new Exception("Email và mật khẩu không được để trống");

                var customer = await _context.Customer.FirstOrDefaultAsync(c => c.Email == request.Email);

                if (customer == null)
                    throw new Exception("Email hoặc mật khẩu không chính xác");

                if (!customer.IsActive)
                    throw new Exception("Tài khoản của bạn đã bị vô hiệu hóa");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
                    throw new Exception("Email hoặc mật khẩu không chính xác");

                var token = GenerateJwtToken(customer);

                _logger.LogInformation($"Khách hàng {customer.CustomerId} đã đăng nhập");

                return new CustomerLoginResponse
                {
                    Token = token,
                    Customer = new CustomerDto
                    {
                        CustomerId = customer.CustomerId,
                        FullName = customer.FullName,
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        Address = customer.Address,
                        TotalSpent = customer.TotalSpent,
                        TotalOrders = customer.TotalOrders,
                        IsActive = customer.IsActive
                    },
                    Message = "Đăng nhập thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đăng nhập khách hàng: {ex.Message}");
                throw new Exception("Lỗi khi đăng nhập: " + ex.Message);
            }
        }

        // Tạo JWT Token
        public string GenerateJwtToken(Customer customer)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"];
            var jwtExpireMinutes = int.Parse(_configuration["JwtSettings:ExpiresIn"] ?? "60");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.CustomerId),
                    new Claim(ClaimTypes.Email, customer.Email),
                    new Claim(ClaimTypes.Name, customer.FullName),
                    new Claim("UserType", "Customer"),
                    new Claim("IsActive", customer.IsActive.ToString())
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
        public async Task<bool> ChangePassword(string customerId, CustomerChangePasswordRequest request)
        {
            try
            {
                if (request.NewPassword != request.ConfirmPassword)
                    throw new Exception("Mật khẩu xác nhận không khớp");

                if (request.NewPassword.Length < 6)
                    throw new Exception("Mật khẩu mới phải có ít nhất 6 ký tự");

                var customer = await _context.Customer.FindAsync(customerId);
                if (customer == null)
                    throw new Exception("Khách hàng không tồn tại");

                if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, customer.PasswordHash))
                    throw new Exception("Mật khẩu cũ không chính xác");

                customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                customer.UpdatedAt = DateTime.Now;

                _context.Customer.Update(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Khách hàng {customerId} đã đổi mật khẩu");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đổi mật khẩu: {ex.Message}");
                throw new Exception("Lỗi khi đổi mật khẩu: " + ex.Message);
            }
        }

        // Lấy profil
        public async Task<CustomerDto> GetProfile(string customerId)
        {
            try
            {
                var customer = await _context.Customer.FindAsync(customerId);
                if (customer == null)
                    throw new Exception("Khách hàng không tồn tại");

                return new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    Address = customer.Address,
                    TotalSpent = customer.TotalSpent,
                    TotalOrders = customer.TotalOrders,
                    IsActive = customer.IsActive
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy profil: " + ex.Message);
            }
        }

        // Cập nhật profil
        public async Task<bool> UpdateProfile(string customerId, CustomerUpdateProfileRequest request)
        {
            try
            {
                var customer = await _context.Customer.FindAsync(customerId);
                if (customer == null)
                    throw new Exception("Khách hàng không tồn tại");

                customer.FullName = request.FullName;
                customer.PhoneNumber = request.PhoneNumber;
                customer.Address = request.Address;
                customer.UpdatedAt = DateTime.Now;

                _context.Customer.Update(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Khách hàng {customerId} đã cập nhật hồ sơ");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật hồ sơ: {ex.Message}");
                throw new Exception("Lỗi khi cập nhật hồ sơ: " + ex.Message);
            }
        }
    }
}