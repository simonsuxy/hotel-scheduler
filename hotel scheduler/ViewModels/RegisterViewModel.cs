using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Services;
using hotel_scheduler.Views;
using Microsoft.Win32;

namespace hotel_scheduler.ViewModels
{
    public partial class RegisterViewModel(AuthService authservice, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly AuthService _authService = authservice;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        [ObservableProperty]
        public partial string Username { get; set; } = "";
        [ObservableProperty]
        public partial string Password { get; set; } = "";
        [ObservableProperty]
        public partial string ConfirmPassword { get; set; } = "";
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasError { get; set; } = false;

        [RelayCommand]
        private async Task RegisterAsync()
        {
            HasError = false;
            ErrorMessage = "";
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Please enter your username and password.";
                HasError = true;
                return;
            }
            try
            {
                await _authService.RegisterAsync(Username.Trim(), Password, ConfirmPassword);
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
        private async Task GoToLogin()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current!.MainPage = _serviceProvider.GetRequiredService<LoginView>();
            });
        }
    }
}
