using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Services;

namespace hotel_scheduler.ViewModels
{
    public partial class MainViewModel(AuthService authService, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly AuthService _authService = authService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [ObservableProperty]
        public partial string HotelName { get; set; } = "Hotel name";

        [ObservableProperty]
        public partial View CurrentView { get; set; } = new ContentView();

        [ObservableProperty]
        public partial string ActivePage { get; set; } = "Calendar";

        public bool IsAdmin => _authService.IsAdmin;

        private bool _initialized = false;

        public void Initialize()
        {
            // Only navigate to Calendar on first launch
            // Subsequent calls should not reset the page
            if (_initialized) return;
            _initialized = true;
            NavigateTo("Calendar");
        }

        [RelayCommand]
        private void NavigateTo(string page)
        {
            ActivePage = page;
            CurrentView = page switch
            {
                "Calendar" => _serviceProvider.GetRequiredService<Views.CalendarView>(),
                "Guests" => _serviceProvider.GetRequiredService<Views.GuestsView>(),
                "Income" => _serviceProvider.GetRequiredService<Views.FinanceView>(),
                "Rooms" => _serviceProvider.GetRequiredService<Views.RoomsView>(),
                "Users" => _serviceProvider.GetRequiredService<Views.UsersView>(),
                _ => new ContentView()
            };
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            Application.Current!.MainPage = _serviceProvider.GetRequiredService<Views.LoginView>();
        }
    }
}