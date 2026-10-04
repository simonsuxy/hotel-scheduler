using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using hotel_scheduler.Views;
using System.Collections.ObjectModel;

namespace hotel_scheduler.ViewModels
{
    public partial class CalendarViewModel(ReservationService reservationService, RoomService roomService, 
            ReservationViewModel reservationViewModel,DateRangePickerViewModel dateRangePickerViewModel ,IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly ReservationService _reservationService = reservationService;
        private readonly RoomService _roomService = roomService;
        private readonly ReservationViewModel _reservationViewModel = reservationViewModel;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly DateRangePickerViewModel _dateRangePickerViewModel = dateRangePickerViewModel;

        public ObservableCollection<RoomDTO> Rooms { get; private set; } = [];
        public ObservableCollection<ReservationDTO> Reservations { get; private set; } = [];
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DateRangleLabel))]
        [NotifyPropertyChangedFor(nameof(VisibleDates))]
        public partial DateTime StartDate { get; set; } = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DateRangleLabel))]
        [NotifyPropertyChangedFor(nameof(VisibleDates))]
        public partial DateTime EndDate { get; set; } = DateTime.Today.AddDays(6);
        [ObservableProperty]
        public partial bool IsNewReservationPopupOpen { get; set; } = false;
        [ObservableProperty]
        public partial bool IsEditReservationPopupOpen { get; set; } = false;
        [ObservableProperty]
        public partial ReservationDTO? SelectedReservation { get; set; } = null;
        [ObservableProperty]
        public partial DateTime PrefilledDate { get; set; } = DateTime.Today;
        [ObservableProperty]
        public partial RoomDTO? PrefilledRoom { get; set; } = null;
        [ObservableProperty]
        public partial int DayCount { get; set; } = 7;

        private bool _suppressDayCountChanged = false;

        partial void OnDayCountChanged(int value)
        {
            if (_suppressDayCountChanged) return;

            EndDate = StartDate.AddDays(value - 1);
            // Don't call LoadReservationsAsync here - let individual commands handle it
            // to avoid redundant database calls when multiple properties change at once
        }

        [RelayCommand]
        private async Task ShiftForwardAsync()
        {
            StartDate = StartDate.AddDays(1);
            EndDate = EndDate.AddDays(1);
            await LoadReservationsAsync();
        }

        [RelayCommand]
        private async Task ShiftBackwardAsync()
        {
            StartDate = StartDate.AddDays(-1);
            EndDate = EndDate.AddDays(-1);
            await LoadReservationsAsync();
        }

        [RelayCommand]
        private async Task IncreaseDaysAsync()
        {
            if (DayCount >= 28) return;
            DayCount++;
        }
        [RelayCommand]
        private async Task DecreaseDaysAsync()
        {
            if (DayCount <= 5) return;
            DayCount--;
        }

        public string DateRangleLabel => $"{StartDate:MMM d} | {EndDate:MMM d}";

        public List<DateTime> VisibleDates
        {
            get
            {
                var dates = new List<DateTime>();
                for(var day = StartDate; day <= EndDate; day = day.AddDays(1))
                    dates.Add(day);
                return dates;
            }
        }

        public async Task InitializeAsync()
        {
            EndDate = StartDate.AddDays(DayCount - 1);
            await LoadRoomsAsync();
            await LoadReservationsAsync();
        }

        private async Task LoadRoomsAsync()
        {
            var rooms = await _roomService.GetAllActiveRoomAsync();
            Rooms.Clear();
            foreach(var room in rooms)
                Rooms.Add(room);
        }
        private async Task LoadReservationsAsync()
        {
            var reservations = await _reservationService.GetReservationsByDateAsync(StartDate,  EndDate.AddDays(1));
            Reservations.Clear();
            foreach(var  reservation in reservations)
                Reservations.Add(reservation);
            OnPropertyChanged(nameof(DateRangleLabel));
            OnPropertyChanged(nameof(VisibleDates));
            OnPropertyChanged(nameof(Reservations));

        }

        [RelayCommand]
        private async Task SetDateRangeAsync(object param)
        {
            if (param is not (DateTime start, DateTime end)) return;
            StartDate = start;
            EndDate = end;
            await LoadReservationsAsync();
        }

        [RelayCommand]
        private async Task OpenNewReservationAsync()
        {
            _reservationViewModel.SetRooms(Rooms);
            _reservationViewModel.InitializeNewReservationPopup();
            _reservationViewModel.OnSaved = async () => await LoadReservationsAsync();
            await ShowPopupAsync();
        }

        [RelayCommand]
        private async Task OpenNewReservationFromCellAsync(object param)
        {
            if (param is not ValueTuple<RoomDTO, DateTime> tuple) return;
            var (room, dateTime) = tuple;

            _reservationViewModel.SetRooms(Rooms);
            _reservationViewModel.InitializeNewReservationPopup(room, dateTime);
            _reservationViewModel.OnSaved = async () => await LoadReservationsAsync();
            await ShowPopupAsync();
        }

        [RelayCommand]
        private async Task OpenEditReservationAsync(ReservationDTO reservation)
        {
            _reservationViewModel.SetRooms(Rooms);
            await _reservationViewModel.InitializeEditReservationPopup(reservation);
            _reservationViewModel.OnSaved = async () =>
            {
                await LoadReservationsAsync();
            };
            await ShowPopupAsync();
        }

        private async Task ShowPopupAsync()
        {
            var guestPopupViewModel = _serviceProvider.GetRequiredService<GuestPopupViewModel>();
            var popup = new ReservationPopup(_reservationViewModel, guestPopupViewModel, _serviceProvider);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }

        [RelayCommand]
        private async Task OpenDatePickerAsync()
        {
            _dateRangePickerViewModel.Initialize(StartDate, EndDate);

            bool rangeChanged = false;

            _dateRangePickerViewModel.OnRangeSelected = (start, end) =>
            {
                // Temporarily suppress OnDayCountChanged to prevent it from overwriting EndDate
                _suppressDayCountChanged = true;
                try
                {
                    StartDate = start;
                    EndDate = end;
                    // Also update DayCount while suppression is active to keep it in sync
                    DayCount = (int)(end - start).TotalDays + 1;
                }
                finally
                {
                    _suppressDayCountChanged = false;
                }

                rangeChanged = true;
            };

            var popup = new DateRangePickerPopup(_dateRangePickerViewModel);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);

            // Reload reservations ONCE after the popup is completely closed
            if (rangeChanged)
            {
                await LoadReservationsAsync();
            }
        }
    }
}
