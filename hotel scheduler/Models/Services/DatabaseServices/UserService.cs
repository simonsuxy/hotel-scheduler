using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
    // Handles all database operations for Users.

    public class UserService
    {
        private readonly HotelSchedulerDbContext _db;
        public UserService(HotelSchedulerDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _db.Users
                .ToListAsync();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return null;
            return new UserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            };
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
            => await _db.Users.AnyAsync(u => u.Username == username.Trim().ToLower());

        public async Task CreateUserAsync(string username, string password, UserRole role)
        {
            if (await IsUsernameTakenAsync(username))
                throw new InvalidOperationException($"Username '{username}' is already taken.");
            var user = new User
            {
                Username = username.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(int id, string username, UserRole role)
        {
            var user = await _db.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found");
            user.Username = username.Trim();
            user.Role = role;
            await _db.SaveChangesAsync();
        }

        // Changes old password to a new password only if user inputed the correct old password
        // If they don't, throws error InvalidOperationException
        public async Task ResetUserPasswordAsync(int id, string newPassword, string oldPassword)
        {
            var user = await _db.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found");
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _db.SaveChangesAsync();
        }

        public async Task DeactivateUserAsync(int id)
        {
            var user = await _db.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found");
            user?.IsActive = false;
            await _db.SaveChangesAsync();
        }

        public async Task ActivateUserAsync(int id)
        {
            var user = await _db.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found");
            user?.IsActive = true;
            await _db.SaveChangesAsync();
        }
        public async Task SetUserRoleAsync(int id, UserRole role)
        {
            var user = await _db.Users.FindAsync(id)
                ?? throw new InvalidOperationException("User not found.");
            user.Role = role;
            await _db.SaveChangesAsync();
        }
    }
}
