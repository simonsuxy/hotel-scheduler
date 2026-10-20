using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
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
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .ToListAsync();

            return [.. users.Select(MapToDTO)];
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : MapToDTO(user);
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
            => await _db.Users.AnyAsync(u => u.Username.ToLower() == username.Trim().ToLower());

        public async Task<UserDTO?> LoginAsync(string username,  string password)
        {
            var trimmedUsername = username.Trim().ToLower();
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == trimmedUsername && u.IsActive) ?? throw new InvalidOperationException("Incorrect username.");
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new InvalidOperationException("Incorrect password.");
            return MapToDTO(user);
        }

        public async Task<UserDTO?> RegisterAsync(string username, string password, UserRole role)
        {
            var trimmedUsername = username.Trim().ToLower();

            if (await IsUsernameTakenAsync(trimmedUsername))
            {
                throw new InvalidOperationException($"Username '{username.Trim()}' is already taken.");
            }

            // assigns admin automatically if database is empty
            bool isFirstUser = !await _db.Users.AnyAsync();
            var finalRole = isFirstUser ? UserRole.Admin : role;

            var user = new User
            {
                Username = trimmedUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = finalRole,
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
                ?? throw new InvalidOperationException("User not found.");

            var trimmedUsername = username.Trim().ToLower();
            if (await _db.Users.AnyAsync(u => u.Username.ToLower() == trimmedUsername && u.Id != id))
                throw new InvalidOperationException($"Username '{trimmedUsername}' is already taken.");

            user.Username = trimmedUsername;
            user.Role = role;
            await _db.SaveChangesAsync();
        }
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
