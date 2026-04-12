namespace TranHuuPhuoc_2123110236.DTOs
{
    // Employee Register
    public class EmployeeRegisterRequest
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // Employee Login
    public class EmployeeLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Employee Response
    public class EmployeeDto
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }  // Admin, Staff
        public bool IsActive { get; set; }
    }

    // Employee Login Response
    public class EmployeeLoginResponse
    {
        public string Token { get; set; }
        public EmployeeDto Employee { get; set; }
        public string Message { get; set; }
    }

    // Change Password
    public class EmployeeChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // Update Employee Role Request
    public class UpdateRoleRequest
    {
        public string NewRole { get; set; }  // Admin, Staff
    }
}