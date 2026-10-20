using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Extensions;
using hotel_scheduler.Views;



namespace hotel_scheduler.ViewModels
{
    public partial class ReservationViewModel(ReservationService reservationService, GuestService guestService,
        AuthService authService, GuestPopupViewModel guestPopupViewModel,
        FinanceService financeService, SettingsService settingsService): ObservableObject
    {
        private readonly ReservationService _reservationService = reservationService;
        private readonly GuestService _guestService = guestService;
        private readonly AuthService _authService = authService;
        private readonly GuestPopupViewModel _guestPopupViewModel = guestPopupViewModel;
        private readonly FinanceService _financeService = financeService;
        private readonly SettingsService _settingsService = settingsService;

        public Action? OnSaved { get; set; }
        public Action? CloseAction { get; set; }
        private int? _editingReservationId = null;

        [ObservableProperty]
        public partial bool IsEditing { get; set; } = false;

        //guest search
        [ObservableProperty]
        public partial string GuestSearchQuery { get; set; } = "";
        [ObservableProperty]
        public partial GuestDTO? SelectedGuest { get; set; } = null;
        public ObservableCollection<GuestDTO> GuestSearchResults { get; } = [];

        public bool HasGuestResults => GuestSearchResults.Count > 0 && SelectedGuest == null;
        public bool HasSelectedGuest => SelectedGuest != null;

        [ObservableProperty]
        public partial bool IsColorDropdownOpen { get; set; } = false;

        // forms
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

        //financial
        [ObservableProperty] public partial ChargeCategoryDTO? SelectedCategoryToAdd { get; set; }
        [ObservableProperty] public partial decimal RoomBasePrice { get; set; }
        [ObservableProperty] public partial decimal TotalExtrasPrice { get; set; }
        [ObservableProperty] public partial decimal ComputedTotalPrice { get; set; }
        public bool HasCharges => ReservationCharges.Count > 0;
        public ObservableCollection<ChargeCategoryDTO> AvailableCategories { get; } = [];
        public ObservableCollection<ReservationChargeDTO> ReservationCharges { get; } = [];

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
        partial void OnCheckInChanged(DateTime value) => RecalculateTotals();
        partial void OnCheckOutChanged(DateTime value) => RecalculateTotals();
        partial void OnSelectedRoomChanged(RoomDTO? value) => RecalculateTotals();

        public void SetRooms(IEnumerable<RoomDTO> rooms)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Rooms.Clear();
                foreach (var room in rooms)
                {
                    Rooms.Add(room);
                }
            });
        }
        public async Task InitializeNewReservationPopup(RoomDTO? chosenRoom = null, DateTime? chosenTime = null)
        {
            ResetPopup();
            IsEditing = false;
            _editingReservationId = null;

            if (chosenRoom != null)
                SelectedRoom = Rooms.FirstOrDefault(r => r.Id == chosenRoom.Id) ?? chosenRoom;

            var settings = await _settingsService.GetSettingsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (chosenTime != null)
                {
                    CheckIn = chosenTime.Value.Date;
                    CheckInTime = chosenTime.Value.Hour == 0
                        ? (settings.DefaultCheckInTime.Hours >= 12 ? "PM" : "AM")
                        : (chosenTime.Value.Hour >= 12 ? "PM" : "AM");

                    CheckOut = chosenTime.Value.Date.AddDays(1);
                    CheckOutTime = settings.DefaultCheckOutTime.Hours >= 12 ? "PM" : "AM";
                }
                else
                {
                    CheckIn = DateTime.Today;
                    CheckInTime = settings.DefaultCheckInTime.Hours >= 12 ? "PM" : "AM";

                    CheckOut = DateTime.Today.AddDays(1);
                    CheckOutTime = settings.DefaultCheckOutTime.Hours >= 12 ? "PM" : "AM";
                }

                ReservationCharges.Clear();
                RecalculateTotals();
            });
            await LoadCategoriesAsync();
        }

        public async Task InitializeEditReservationPopup(ReservationDTO reservation)
        {
            ResetPopup();
            IsEditing = true;
            _editingReservationId = reservation.Id;

            SelectedRoom = Rooms.FirstOrDefault(r => r.Id == reservation.RoomId) ?? new RoomDTO
            {
                Id = reservation.RoomId,
                Name = reservation.RoomName,
                PricePerNight = reservation.RoomPricePerNight
            };
            GuestCount = reservation.GuestCount;
            SelectedColor = string.IsNullOrWhiteSpace(reservation.Color) ? "#4C9BE8" : reservation.Color;
            SelectedColorOption = ReservationColorsOptions
                .FirstOrDefault(c => c.Hex == SelectedColor)
                ?? ReservationColorsOptions.First();
            PaymentStatus = reservation.PaymentStatus;
            Notes = reservation.Notes ?? "";
            CheckInTime = reservation.CheckIn.Hour >= 14 ? "PM" : "AM";
            CheckOutTime = reservation.CheckOut.Hour >= 18 ? "PM" : "AM";

            MainThread.BeginInvokeOnMainThread(() =>
            {
                CheckIn = reservation.CheckIn.Date;
                CheckOut = reservation.CheckOut.Date;
            });

            var guest = await _guestService.GetGuestByIdAsync(reservation.GuestId);
            if (guest != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SelectedGuest = guest;
                    GuestSearchQuery = "";
                    GuestSearchResults.Clear();
                    OnPropertyChanged(nameof(HasSelectedGuest));
                    OnPropertyChanged(nameof(HasGuestResults));
                });
            }

            OnPropertyChanged(nameof(PaymentStatusColor));
            OnPropertyChanged(nameof(PaymentStatusLabel));

            await LoadCategoriesAsync();

            var charges = await _financeService.GetChargesForReservationAsync(reservation.Id);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ReservationCharges.Clear();
                foreach (var charge in charges)
                {
                    ReservationCharges.Add(charge);
                }
                RecalculateTotals();
            });
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                GuestSearchResults.Clear();
                foreach (var guest in guests)
                {
                    GuestSearchResults.Add(guest);
                }
                OnPropertyChanged(nameof(HasGuestResults));
            });
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
            var settings = await _settingsService.GetSettingsAsync();

            int inHour = CheckInTime == "PM" ? Math.Max(12, settings.DefaultCheckInTime.Hours) : 8;
            int outHour = CheckOutTime == "AM" ? Math.Min(11, settings.DefaultCheckOutTime.Hours) : 20;

            var checkIn = CheckIn.Date.AddHours(inHour);
            var checkOut = CheckOut.Date.AddHours(outHour);

            if (checkOut <= checkIn)
            {
                ErrorMessage = "Check-out time must be after check-in time.";
                HasError = true;
                return;
            }

            try
            {
                int reservationId;
                if (IsEditing && _editingReservationId.HasValue)
                {
                    reservationId = _editingReservationId.Value;
                    await _reservationService.UpdateAsync(_editingReservationId.Value,
                                        SelectedRoom.Id, SelectedGuest.Id, checkIn, checkOut,
                                        GuestCount, Notes, SelectedColor);
                    await _reservationService.UpdatePaymenetStatus(_editingReservationId.Value, PaymentStatus);
                    var oldCharges = await _financeService.GetChargesForReservationAsync(reservationId);
                    foreach (var c in oldCharges) 
                        await _financeService.RemoveChargeFromReservationAsync(c.Id);
                }
                else
                {
                    reservationId = await _reservationService.CreateReservationAsync(SelectedRoom.Id, SelectedGuest.Id, checkIn, checkOut,
                                                GuestCount, Notes, SelectedColor, _authService.CurrentUser?.Id);
                }
                foreach (var charge in ReservationCharges)
                {
                    await _financeService.AddChargeToReservationAsync(reservationId, charge.CategoryId, charge.Quantity, charge.PricePerNight);
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
            MainThread.BeginInvokeOnMainThread(() =>
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
            });
        }

        [RelayCommand]
        private async Task OpenGuestPopupAsync()
        {
            _guestPopupViewModel.InitializeGuestPopup(GuestSearchQuery);
            _guestPopupViewModel.OnGuestCreated = (guest) =>
            {
                SelectedGuest = guest;
                OnPropertyChanged(nameof(HasSelectedGuest));
                GuestSearchQuery = "";
                GuestSearchResults.Clear();
            };
            var popup = new GuestPopup(_guestPopupViewModel, null, null);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }

        private void RecalculateTotals()
        {
            int nights = Math.Max(1, (CheckOut.Date - CheckIn.Date).Days);
            RoomBasePrice = (SelectedRoom?.PricePerNight ?? 0m) * nights;
            TotalExtrasPrice = ReservationCharges.Sum(c => c.CalculateTotal(nights));
            ComputedTotalPrice = RoomBasePrice + TotalExtrasPrice;
            OnPropertyChanged(nameof(HasCharges));
        }

        partial void OnSelectedCategoryToAddChanged(ChargeCategoryDTO? value)
        {
            if (value == null) return;

            // If the user clicked the placeholder option, reset picker to placeholder
            if (value.Id == -1)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    SelectedCategoryToAdd = AvailableCategories.FirstOrDefault(c => c.Id == -1));
                return;
            }

            // Normal item selected: add it to reservation charges
            ReservationCharges.Add(new ReservationChargeDTO
            {
                Id = 0,
                CategoryId = value.Id,
                CategoryName = value.Name,
                CategoryDescription = value.Description,
                ChargeType = value.Type,
                Quantity = 1,
                PricePerNight = value.DefaultPricePerNight
            });

            RecalculateTotals();

            MainThread.BeginInvokeOnMainThread(() =>
                SelectedCategoryToAdd = AvailableCategories.FirstOrDefault(c => c.Id == -1));
        }

        [RelayCommand]
        private void RemoveCharge(ReservationChargeDTO charge)
        {
            if (charge != null)
            {
                ReservationCharges.Remove(charge);
                RecalculateTotals();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _financeService.GetCategoriesAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvailableCategories.Clear();
                AvailableCategories.Add(new ChargeCategoryDTO
                {
                    Id = -1,
                    Name = "Add new category..."
                });

                foreach (var c in categories)
                {
                    AvailableCategories.Add(c);
                }

                SelectedCategoryToAdd = AvailableCategories.First();
            });
        }

        [RelayCommand]
        private void UpdateChargeQuantity(ReservationChargeDTO charge)
        {
            if (charge == null) return;
            charge.Quantity = charge.Quantity;

            RecalculateTotals();
            OnPropertyChanged(nameof(ReservationCharges));
        }

        [RelayCommand]
        private void Close()
        {
            CloseAction?.Invoke();
        }

    }
}
