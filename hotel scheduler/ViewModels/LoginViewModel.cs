using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Services;
using hotel_scheduler.Views;

namespace hotel_scheduler.ViewModels
{
    public partial class LoginViewModel(AuthService authService, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly AuthService _authService = authService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [ObservableProperty]
        public partial string Username { get; set; } = "";
        [ObservableProperty]
        public partial string Password { get; set; } = "";
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasError { get; set; } = false;

        [RelayCommand]
        private async Task LoginAsync()
        {
            HasError = false;
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter your username and password.";
                HasError = true;
                return;
            }
            try
            {
                await _authService.LoginAsync(Username, Password);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current!.MainPage = _serviceProvider.GetRequiredService<MainView>();
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task GoToRegister()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current!.MainPage = _serviceProvider.GetRequiredService<RegisterView>();
            });
        }
    }
}
