namespace TranHuuPhuoc_2123110236.DTOs
{
    // Register Request - Xóa EmployeeId, thêm Department & Salary
    public class EmployeeRegisterRequest
    {
        // ❌ Xóa: EmployeeId
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Department { get; set; }  // ✅ Thêm
        public decimal Salary { get; set; }     // ✅ Thêm
    }

    // Login Request
    public class EmployeeLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Employee DTO Response - Thêm Department & Salary
    public class EmployeeDto
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Department { get; set; }  // ✅ Thêm
        public decimal Salary { get; set; }     // ✅ Thêm
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Login Response
    public class EmployeeLoginResponse
    {
        public string Token { get; set; }
        public EmployeeDto Employee { get; set; }
        public string Message { get; set; }
    }

    // Change Password Request
    public class EmployeeChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // Update Employee Role Request
    public class UpdateRoleRequest
    {
        public string NewRole { get; set; }
    }

    // Update Employee Profile Request
    public class EmployeeUpdateProfileRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Department { get; set; }
        public decimal Salary { get; set; }
    }
}