using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface IEmployeeAuthService
    {
        Task<EmployeeLoginResponse> Login(EmployeeLoginRequest request);
        Task<EmployeeDto> Register(EmployeeRegisterRequest request);  // Chỉ Admin
        Task<bool> ChangePassword(string employeeId, EmployeeChangePasswordRequest request);
        Task<EmployeeDto> GetProfile(string employeeId);
        Task<bool> UpdateEmployeeRole(string employeeId, string newRole);  // Chỉ Admin
        Task<List<EmployeeDto>> GetAllEmployees();  // Chỉ Admin
        string GenerateJwtToken(Employee employee);
    }
}