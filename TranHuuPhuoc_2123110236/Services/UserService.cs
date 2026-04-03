using TranHuuPhuoc_2123110236.Models;
using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;

namespace TranHuuPhuoc_2123110236.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await _context.User.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách người dùng: " + ex.Message);
            }
        }

        public async Task<User> GetUserById(string id)
        {
            try
            {
                var user = await _context.User.FindAsync(id);
                if (user == null)
                {
                    throw new Exception("Người dùng không tồn tại");
                }
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy người dùng: " + ex.Message);
            }
        }

        public async Task<User> CreateUser(User user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.UserId))
                {
                    throw new Exception("ID người dùng không được để trống");
                }

                if (string.IsNullOrWhiteSpace(user.FullName))
                {
                    throw new Exception("Tên đầy đủ không được để trống");
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    throw new Exception("Email không được để trống");
                }

                var existingUser = await _context.User.FindAsync(user.UserId);
                if (existingUser != null)
                {
                    throw new Exception("ID người dùng đã tồn tại");
                }

                var existingEmail = await _context.User
                    .FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingEmail != null)
                {
                    throw new Exception("Email đã tồn tại");
                }

                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo người dùng: " + ex.Message);
            }
        }

        public async Task<User> UpdateUser(string id, User user)
        {
            try
            {
                var existingUser = await _context.User.FindAsync(id);

                if (existingUser == null)
                {
                    throw new Exception("Người dùng không tồn tại");
                }

                if (string.IsNullOrWhiteSpace(user.FullName))
                {
                    throw new Exception("Tên đầy đủ không được để trống");
                }

                existingUser.FullName = user.FullName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Address = user.Address;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                existingUser.UpdatedAt = DateTime.Now;

                _context.User.Update(existingUser);
                await _context.SaveChangesAsync();

                return existingUser;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật người dùng: " + ex.Message);
            }
        }

        public async Task<bool> DeleteUser(string id)
        {
            try
            {
                var user = await _context.User.FindAsync(id);

                if (user == null)
                {
                    throw new Exception("Người dùng không tồn tại");
                }

                _context.User.Remove(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa người dùng: " + ex.Message);
            }
        }

        public async Task<List<User>> SearchUserByName(string name)
        {
            try
            {
                return await _context.User
                    .Where(u => u.FullName.Contains(name))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm người dùng: " + ex.Message);
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    throw new Exception("Email không tồn tại");
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm email: " + ex.Message);
            }
        }

        public async Task<int> GetUserCount()
        {
            try
            {
                return await _context.User.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đếm người dùng: " + ex.Message);
            }
        }
    }
}