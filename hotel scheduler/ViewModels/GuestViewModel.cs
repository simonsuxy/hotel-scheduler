using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;

namespace hotel_scheduler.ViewModels
{
    public partial class GuestViewModel(GuestService guestService, GuestPopupViewModel guestPopupViewModel, ReservationViewModel reservationViewModel, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly GuestService _guestService = guestService;
        private readonly GuestPopupViewModel _guestPopupViewModel = guestPopupViewModel;
        private readonly ReservationViewModel _reservationViewModel = reservationViewModel;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        private List<GuestDTO> _allGuests = [];
        public ObservableCollection<GuestDTO> FilteredGuests { get; } = [];

        [ObservableProperty]
        public partial string SearchQuery { get; set; } = "";

        [ObservableProperty]
        public partial bool ShowCurrent { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowFuture { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowPast { get; set; } = true;

        public bool NoFiltersSelected => !ShowCurrent && !ShowFuture && !ShowPast;

        public async Task InitializeAsync()
        {
            await LoadGuestsAsync();
        }

        public async Task LoadGuestsAsync()
        {
            _allGuests.Clear();
            _allGuests = await _guestService.GetAllGuestsAsync();
            ApplyFilters();
        }

        partial void OnSearchQueryChanged(string value) => ApplyFilters();
        partial void OnShowCurrentChanged(bool value) => ApplyFilters();
        partial void OnShowFutureChanged(bool value) => ApplyFilters();
        partial void OnShowPastChanged(bool value) => ApplyFilters();


        // Priority 1 -> currently, 2 -> Future, 3 -> Past/None reservations
        private static int GetGuestPriority(GuestDTO guest, DateTime now)
        {
            if (guest.Reservations.Any(r => r.CheckIn <= now && r.CheckOut >= now))
                return 1;
            if (guest.Reservations.Any(r => r.CheckIn > now))
                return 2;
            return 3;
        }

        private void ApplyFilters()
        {
            OnPropertyChanged(nameof(NoFiltersSelected));

            var now = DateTime.Now;
            var query = SearchQuery.Trim().ToLower();

            var filtered = _allGuests.Where(g =>
            {
                if (!string.IsNullOrWhiteSpace(query) && !g.FullName.ToLower().Contains(query))
                    return false;

                var isCurrent = g.Reservations.Any(r => r.CheckIn <= now && r.CheckOut >= now);
                var isFuture = g.Reservations.Any(r => r.CheckIn > now);
                var isPast = g.Reservations.Any(r => r.CheckOut < now);
                var hasNoReservations = !g.Reservations.Any();

                return (ShowCurrent && isCurrent) ||
                       (ShowFuture && isFuture) ||
                       (ShowPast && isPast) ||
                       hasNoReservations;
            })
            .OrderBy(g => GetGuestPriority(g, now))
            .ThenBy(g => g.FullName)
            .ToList();

            FilteredGuests.Clear();
            foreach (var g in filtered)
                FilteredGuests.Add(g);
        }

        [RelayCommand]
        private void ToggleCurrent()
        {
            ShowCurrent = !ShowCurrent;
            OnPropertyChanged(nameof(NoFiltersSelected));
        }

        [RelayCommand]
        private void ToggleFuture()
        {
            ShowFuture = !ShowFuture;
            OnPropertyChanged(nameof(NoFiltersSelected));
        }

        [RelayCommand]
        private void TogglePast()
        {
            ShowPast = !ShowPast;
            OnPropertyChanged(nameof(NoFiltersSelected));
        }

        // Opens GuestPopup in create mode
        [RelayCommand]
        private async Task OpenNewGuestAsync()
        {
            _guestPopupViewModel.InitializeGuestPopup();
            _guestPopupViewModel.OnGuestCreated = async (guest) =>
                await LoadGuestsAsync();
            var popup = new Views.GuestPopup(_guestPopupViewModel, _reservationViewModel, _serviceProvider);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }

        // Opens GuestPopup in edit mode
        [RelayCommand]
        private async Task OpenEditGuestAsync(GuestDTO guest)
        {
            _guestPopupViewModel.InitializeEditGuestPopup(guest);
            _guestPopupViewModel.OnSaved = async () => await LoadGuestsAsync();
            var popup = new Views.GuestPopup(_guestPopupViewModel, _reservationViewModel, _serviceProvider);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }
    }
}