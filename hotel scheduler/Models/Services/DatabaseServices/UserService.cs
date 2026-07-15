using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
    // Handles all database operations for Users.

    public class UserService(HotelSchedulerDbContext db)
    {
        private readonly HotelSchedulerDbContext _db = db;

        private static UserDTO MapToDTO(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _db.Users
                .ToListAsync();
            return [.. users.Select(MapToDTO)];
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return null;
            return MapToDTO(u);
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
            => await _db.Users.AnyAsync(u => u.Username == username.Trim().ToLower());

        public async Task<UserDTO?> LoginAsync(string username,  string password)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username.Trim().ToLower() && u.IsActive);
            if (user  == null)
                throw new InvalidOperationException("Incorrect username.");
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new InvalidOperationException("Incorrect password.");
            return MapToDTO(user);
        }

        public async Task<UserDTO?> RegisterAsync(string username, string password, UserRole role)
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
            return MapToDTO(user);
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
