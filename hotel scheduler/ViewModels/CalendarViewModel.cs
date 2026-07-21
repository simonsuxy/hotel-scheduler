using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;

namespace hotel_scheduler.ViewModels
{
    public partial class CalendarViewModel(ReservationService reservationService, RoomService roomService, ReservationViewModel reservationViewModel) : ObservableObject
    {
        private readonly ReservationService _reservationService = reservationService;
        private readonly RoomService _roomService = roomService;
        private readonly ReservationViewModel _reservationViewModel = reservationViewModel;

        public ObservableCollection<RoomDTO> Rooms { get; private set; } = [];
        public ObservableCollection<ReservationDTO> Reservations { get; private set; } = [];
        [ObservableProperty]
        public partial DateTime StartDate { get; set; } = DateTime.Today;
        [ObservableProperty]
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
            _reservationViewModel.OnSaved = async () => await LoadReservationsAsync();
            await ShowPopupAsync();
        }

        private async Task ShowPopupAsync()
        {
            var popup = new Views.ReservationPopup(_reservationViewModel);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }
    }
}
