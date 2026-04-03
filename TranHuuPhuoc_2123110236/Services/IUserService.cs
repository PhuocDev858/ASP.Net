using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<User> GetUserById(string id);
        Task<User> CreateUser(User user);
        Task<User> UpdateUser(string id, User user);
        Task<bool> DeleteUser(string id);
        Task<List<User>> SearchUserByName(string name);
        Task<User> GetUserByEmail(string email);
        Task<int> GetUserCount();
    }
}