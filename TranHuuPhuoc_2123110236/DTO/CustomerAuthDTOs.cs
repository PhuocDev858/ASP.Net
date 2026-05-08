namespace TranHuuPhuoc_2123110236.DTOs
{
    // Customer Register
    public class CustomerRegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // Customer Login
    public class CustomerLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Customer Response
    public class CustomerDto
    {
        public string CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public bool IsActive { get; set; }
    }

    // Customer Login Response
    public class CustomerLoginResponse
    {
        public string Token { get; set; }
        public CustomerDto Customer { get; set; }
        public string Message { get; set; }
    }

    // Change Password
    public class CustomerChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // Update Profile
    public class CustomerUpdateProfileRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    // ===== THÊM MỚI CHO QUÊN MẬT KHẨU =====

    // Bước 1: Gửi OTP
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = "";
    }

    // Bước 2: Xác nhận OTP
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = "";
        public string Otp { get; set; } = "";
    }

    // Bước 3: Đặt lại mật khẩu
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = "";
        public string Otp { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}