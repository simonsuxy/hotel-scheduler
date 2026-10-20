using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;

namespace hotel_scheduler.Models.Services
{
    public class AuthService(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public UserDTO? CurrentUser { get; private set; }
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
        public async Task LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Entry all infromation");

            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();

            var user = await userService.LoginAsync(username, password);
            if (user != null)
                CurrentUser = user;
        }

        public async Task RegisterAsync(string username, string password1, string password2)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password1) || string.IsNullOrWhiteSpace(password2))
                throw new InvalidOperationException("Please fill in all information.");

            if (password1 != password2)
                throw new InvalidOperationException("Passwords do not match.");

            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();

            // This will now throw an exception if the username exists
            var user = await userService.RegisterAsync(username, password1, UserRole.Staff);

            CurrentUser = user;
        }
        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
