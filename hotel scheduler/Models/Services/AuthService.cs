using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;

namespace hotel_scheduler.Models.Services
{
    public class AuthService
    {
        private readonly IServiceProvider _serviceProvider;
        public UserDTO? CurrentUser { get; private set; }
        public bool IsLoogedIn => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
        public AuthService(IServiceProvider service)
        {
            _serviceProvider = service;
        }
        public async Task LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Entry all infromation");
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var user = await userService.LoginAsync(username, password);
            if (user == null)
            CurrentUser = user;
        }

        public async Task RegisterAsync(string username, string password1, string password2)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password1) || string.IsNullOrWhiteSpace(password2))
                throw new InvalidOperationException("Entry all infromation");
            if (password1 != password2)
                throw new InvalidOperationException("Not matching passwords");
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var user = await userService.RegisterAsync(username, password1, UserRole.Staff);
            CurrentUser = user;
        }
        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
