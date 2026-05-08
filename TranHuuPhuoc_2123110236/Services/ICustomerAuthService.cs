using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface ICustomerAuthService
    {
        Task<CustomerLoginResponse> Login(CustomerLoginRequest request);
        Task<CustomerDto> Register(CustomerRegisterRequest request);
        Task<bool> ChangePassword(string customerId, CustomerChangePasswordRequest request);
        Task<CustomerDto> GetProfile(string customerId);
        Task<bool> UpdateProfile(string customerId, CustomerUpdateProfileRequest request);
        string GenerateJwtToken(Customer customer);

        // ===== THÊM MỚI =====
        Task ForgotPassword(string email);
        bool VerifyOtp(string email, string otp);
        Task ResetPassword(string email, string otp, string newPassword);
    }
}