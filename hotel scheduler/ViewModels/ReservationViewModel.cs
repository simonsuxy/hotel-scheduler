using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;

namespace hotel_scheduler.ViewModels
{
    public partial class ReservationViewModel(ReservationService reservationService, GuestService guestService, AuthService authService): ObservableObject
    {
        private readonly ReservationService _reservationService = reservationService;
        private readonly GuestService _guestService = guestService;
        private readonly AuthService _authService = authService;

        public Action? OnSaved { get; set; }
        private int? _editingReservationId = null;

        [ObservableProperty]
        public partial bool IsEditing { get; set; } = false;

        // Guest search

        [ObservableProperty]
        public partial string GuestSearchQuery { get; set; } = "";
        [ObservableProperty]
        public partial GuestDTO? SelectedGuest { get; set; } = null;
        [ObservableProperty]
        public partial ObservableCollection<GuestDTO> GuestSearchResults { get; set; } = [];

        public bool HasGuestResults => GuestSearchResults.Count > 0 && SelectedGuest == null;
        public bool HasSelectedGuest => SelectedGuest != null;

        [ObservableProperty]
        public partial bool IsColorDropdownOpen { get; set; } = false;
        public Action? CloseAction { get; set; }

        // Form fields
        public ObservableCollection<RoomDTO> Rooms { get; } = [];

        [ObservableProperty]
        public partial RoomDTO? SelectedRoom { get; set; } = null;
        [ObservableProperty]
        public partial DateTime CheckIn { get; set; } = DateTime.Today;
        [ObservableProperty]
        public partial DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);
        [ObservableProperty]
        public partial string CheckInTime { get; set; } = "PM";
        [ObservableProperty]
        public partial string CheckOutTime { get; set; } = "AM";
        [ObservableProperty]
        public partial int GuestCount { get; set; } = 1;
        [ObservableProperty]
        public partial string SelectedColor { get; set; } = "#4C9BE8";
        [ObservableProperty]
        public partial ReservationColor? SelectedColorOption { get; set; } 
        [ObservableProperty]
        public partial PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        [ObservableProperty]
        public partial string Notes { get; set; } = "";
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasError { get; set; } = false;



        public ObservableCollection<string> TimeOptions { get; } = ["AM", "PM"];

        public string PaymentStatusColor => PaymentStatus switch
        {
            PaymentStatus.Unpaid => "#E53935",
            PaymentStatus.PartiallyPaid => "#FB8C00",
            PaymentStatus.Paid => "#43A047",
            _ => "#E53935"
        };
        public string PaymentStatusLabel => PaymentStatus switch
        {
            PaymentStatus.Unpaid => "Unpaid",
            PaymentStatus.PartiallyPaid => "Partially paid",
            PaymentStatus.Paid => "Paid",
            _ => "Unpaid"
        };

        public ObservableCollection<ReservationColor> ReservationColorsOptions { get; } =
            [
                new ReservationColor("Blue",   "#4C9BE8"),
                new ReservationColor("Green",  "#56C596"),
                new ReservationColor("Orange", "#F4A261"),
                new ReservationColor("Coral",  "#E76F51"),
                new ReservationColor("Purple", "#A78BFA"),
                new ReservationColor("Pink",   "#F472B6"),
                new ReservationColor("Sky",    "#38BDF8"),
                new ReservationColor("Amber",  "#FBBF24")
            ];

        [RelayCommand]
        private void SelectColor(ReservationColor color)
        {
            SelectedColorOption = color;
            SelectedColor = color.Hex;
            IsColorDropdownOpen = false;
        }

        [RelayCommand]
        private void ToggleColorDropdown()
        {
            IsColorDropdownOpen = !IsColorDropdownOpen;
        }

        public void InitializeNewReservationPopup(RoomDTO? chosenRoom = null, DateTime? chosenTime = null)
        {
            ResetPopup();
            IsEditing = false;
            _editingReservationId = null;
            if (chosenRoom != null)
                SelectedRoom = Rooms.FirstOrDefault(r => r.Id == chosenRoom.Id);
            if (chosenTime != null)
            {
                CheckIn = chosenTime.Value.Date;
                CheckInTime = chosenTime.Value.Hour >= 12 ? "PM" : "AM";
                CheckOut = chosenTime.Value.Date.AddDays(2);
                CheckOutTime = "AM";
            }
        }

        public async Task InitializeEditReservationPopup(ReservationDTO reservation)
        {
            ResetPopup();
            IsEditing = true;
            _editingReservationId = reservation.Id;
            SelectedRoom = Rooms.FirstOrDefault(r => r.Id == reservation.RoomId);
            GuestCount = reservation.GuestCount;
            SelectedColorOption = ReservationColorsOptions
                .FirstOrDefault(c => c.Hex == reservation.Color)
                ?? ReservationColorsOptions.First();
            PaymentStatus = reservation.PaymentStatus;
            Notes = reservation.Notes;
            CheckInTime = reservation.CheckIn.Hour >= 12 ? "PM" : "AM";
            CheckOutTime = reservation.CheckOut.Hour >= 12 ? "PM" : "AM";

            // Force DatePicker to update on main thread after a brief delay
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Task.Delay(50);
                CheckIn = reservation.CheckIn.Date;
                CheckOut = reservation.CheckOut.Date;
            });

            var guest = await _guestService.GetGuestByIdAsync(reservation.GuestId);
            if (guest != null)
            {
                SelectedGuest = guest;
                OnPropertyChanged(nameof(HasSelectedGuest));
            }

            OnPropertyChanged(nameof(PaymentStatusColor));
            OnPropertyChanged(nameof(PaymentStatusLabel));
        }

        public void SetRooms(IEnumerable<RoomDTO> rooms)
        {
            Rooms.Clear();
            foreach(var room in rooms)
                Rooms.Add(room);
        }

        partial void OnGuestSearchQueryChanged(string value)
        {
            _ = SearchGuestAsync(value);
        }

        private async Task SearchGuestAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                GuestSearchResults.Clear();
                OnPropertyChanged(nameof(HasGuestResults));
                return;
            }
            var guests = await _guestService.SearchAsync(query);
            GuestSearchResults.Clear();
            foreach(var guest in guests)
                GuestSearchResults.Add(guest);
            OnPropertyChanged(nameof(HasGuestResults));
        }

        [RelayCommand]
        private void SelectGuest(GuestDTO guest)
        {
            SelectedGuest = guest;
            GuestSearchQuery = "";
            GuestSearchResults.Clear();
            OnPropertyChanged(nameof(HasGuestResults));
            OnPropertyChanged(nameof(HasSelectedGuest));
        }

        [RelayCommand]
        private void ClearGuest()
        {
            SelectedGuest = null;
            OnPropertyChanged(nameof(HasSelectedGuest));
        }

        [RelayCommand]
        private void CyclePaymentStatus()
        {
            PaymentStatus = PaymentStatus switch
            {
                PaymentStatus.Unpaid => PaymentStatus.PartiallyPaid,
                PaymentStatus.PartiallyPaid => PaymentStatus.Paid,
                PaymentStatus.Paid => PaymentStatus.Unpaid,
                _ => PaymentStatus.Unpaid
            };
            OnPropertyChanged(nameof(PaymentStatusColor));
            OnPropertyChanged(nameof(PaymentStatusLabel));
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            HasError = false;
            ErrorMessage = "";
            if (SelectedGuest == null)
            {
                ErrorMessage = "Select a guest.";
                HasError = true;
                return;
            }
            if (SelectedRoom == null)
            {
                ErrorMessage = "Select a room.";
                HasError = true;
                return;
            }
            var checkIn = CheckIn.Date.AddHours(CheckInTime == "PM" ? 14 : 8);
            var checkOut = CheckOut.Date.AddHours(CheckOutTime == "PM" ? 14 : 8);
            try
            {
                if (IsEditing && _editingReservationId.HasValue)
                {
                    await _reservationService.UpdateAsync(_editingReservationId.Value,
                                        SelectedRoom.Id, SelectedGuest.Id, checkIn, checkOut,
                                        GuestCount, Notes, SelectedColor);
                    await _reservationService.UpdatePaymenetStatus(_editingReservationId.Value, PaymentStatus);
                }
                else
                {
                    await _reservationService.CreateReservationAsync(SelectedRoom.Id, SelectedGuest.Id, checkIn, checkOut,
                                                GuestCount, Notes, SelectedColor, _authService.CurrentUser?.Id);
                }
                OnSaved?.Invoke();
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }         
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (_editingReservationId == null) return;

            try
            {
                await _reservationService.DeleteAsync(_editingReservationId.Value);
                OnSaved?.Invoke();
                CloseAction?.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
        }

        private void ResetPopup()
        {            
            SelectedGuest = null;
            SelectedRoom = null;
            GuestSearchQuery = "";
            GuestSearchResults.Clear();
            CheckIn = DateTime.Today;
            CheckOut = DateTime.Today.AddDays(1);
            CheckInTime = "PM";
            CheckOutTime = "AM";
            GuestCount = 1;
            SelectedColor = "#4C9BE8";
            SelectedColorOption = ReservationColorsOptions.FirstOrDefault(c => c.Hex == "#4C9BE8");
            PaymentStatus = PaymentStatus.Unpaid;
            Notes = "";
            ErrorMessage = "";
            HasError = false;
            _editingReservationId = null;
            OnPropertyChanged(nameof(HasGuestResults));
            OnPropertyChanged(nameof(HasSelectedGuest));
            OnPropertyChanged(nameof(PaymentStatusColor));
            OnPropertyChanged(nameof(PaymentStatusLabel));
        }

        [RelayCommand]
        private void Close()
        {
            CloseAction?.Invoke();
        }

    }
}
