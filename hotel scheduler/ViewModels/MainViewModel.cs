using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Services;
using hotel_scheduler.Models.Services.DatabaseServices;

namespace hotel_scheduler.ViewModels
{
    public partial class MainViewModel(AuthService authService, IServiceProvider serviceProvider,
        SettingsService settingsService) : ObservableObject
    {
        private readonly AuthService _authService = authService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly SettingsService _settingsService = settingsService;

        [ObservableProperty]
        public partial string HotelName { get; set; } = "Hotel name";

        [ObservableProperty]
        public partial View CurrentView { get; set; } = new ContentView();

        [ObservableProperty]
        public partial string ActivePage { get; set; } = "Calendar";

        public bool IsAdmin => _authService.IsAdmin;
        private bool _initialized = false;

        public async Task InitializeAsync()
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                if (!string.IsNullOrWhiteSpace(settings.HotelName))
                {
                    HotelName = settings.HotelName;
                }
            }
            catch
            {
                HotelName = "Hotel";
            }

            // Only navigate to Calendar on first launch
            if (!_initialized)
            {
                _initialized = true;
                NavigateTo("Calendar");
            }
        }

        [RelayCommand]
        private void NavigateTo(string page)
        {
            if (string.IsNullOrWhiteSpace(page)) return;

            ActivePage = page;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentView = page switch
                {
                    "Calendar" => _serviceProvider.GetRequiredService<Views.CalendarView>(),
                    "Guests" => _serviceProvider.GetRequiredService<Views.GuestsView>(),
                    "Income" => _serviceProvider.GetRequiredService<Views.FinanceView>(),
                    "Rooms" => _serviceProvider.GetRequiredService<Views.RoomsView>(),
                    "Users" => _serviceProvider.GetRequiredService<Views.UsersView>(),
                    "Settings" => _serviceProvider.GetRequiredService<Views.SettingsView>(),
                    _ => new ContentView()
                };
            });
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current!.MainPage = _serviceProvider.GetRequiredService<Views.LoginView>();
            });
        }
    }
}