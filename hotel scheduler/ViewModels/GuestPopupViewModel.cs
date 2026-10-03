using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;

namespace hotel_scheduler.ViewModels
{
    public partial class GuestPopupViewModel(GuestService guestService): ObservableObject
    {
        private readonly GuestService _guestService = guestService;

        public Action<GuestDTO>? OnGuestCreated { get; set; }
        public Action? CloseAction { get; set; }
        public Action? OnSaved { get; set; }

        private int? _editingGuestId = null;

        [ObservableProperty]
        public partial bool IsEditing { get; set; } = false;
        [ObservableProperty]
        public partial string FirstName { get; set; } = "";

        [ObservableProperty]
        public partial string LastName { get; set; } = "";

        [ObservableProperty]
        public partial string Phone { get; set; } = "";

        [ObservableProperty]
        public partial string Email { get; set; } = "";

        [ObservableProperty]
        public partial string IdNumber { get; set; } = "";

        [ObservableProperty]
        public partial string Address { get; set; } = "";

        [ObservableProperty]
        public partial string Notes{ get; set; } = "";
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasError { get; set; } = false;
        public List<ReservationDTO> Reservations { get; private set; } = [];

        public bool HasNoReservations => IsEditing && !Reservations.Any();
        public bool HasReservations => IsEditing && Reservations.Any();

        public void InitializeGuestPopup(string prefilledName = "")
        {
            ResetForm();
            IsEditing = false;
            _editingGuestId = null;
            if(!string.IsNullOrWhiteSpace(prefilledName))
            {
                var parts = prefilledName.Trim().Split('.',2);
                FirstName = parts[0];
                LastName = parts.Length > 1 ? parts[1] : "";
            }
        }

        public void InitializeEditGuestPopup(GuestDTO guest)
        {
            ResetForm();
            IsEditing = true;
            _editingGuestId = guest.Id;
            FirstName = guest.FirstName;
            LastName = guest.LastName;
            Phone = guest.Phone ?? "";
            Email = guest.Email ?? "";
            IdNumber = guest.IdNumber ?? "";
            Address = guest.Address ?? "";
            Notes = guest.Notes;
            Reservations = guest.Reservations;
            OnPropertyChanged(nameof(HasNoReservations));
            OnPropertyChanged(nameof(HasReservations));
        }


        [RelayCommand]
        private async Task SaveAsync()
        {
            HasError = false;
            ErrorMessage = "";
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                ErrorMessage = "First name is required";
                HasError = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "Last name is required";
                HasError = true;
                return;
            }
            try
            {
                // Edit page
                if (IsEditing && _editingGuestId.HasValue)
                {
                    await _guestService.UpdateAsync(
                        _editingGuestId.Value, FirstName.Trim(), LastName.Trim(),
                        string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                        string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                        string.IsNullOrWhiteSpace(IdNumber) ? null : IdNumber.Trim(),
                        string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                        Notes.Trim());
                    OnSaved?.Invoke();
                    CloseAction?.Invoke();
                }
                // Create page
                else
                {
                    var guest = await _guestService.CreateAsync(
                        FirstName.Trim(), LastName.Trim(),
                        string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                        string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                        string.IsNullOrWhiteSpace(IdNumber) ? null : IdNumber.Trim(),
                        string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                        Notes.Trim());
                    OnGuestCreated?.Invoke(guest);
                    CloseAction?.Invoke();
                }
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
            if (_editingGuestId == null) return;
            HasError = false;
            ErrorMessage = "";
            try
            {
                await _guestService.DeleteAsync(_editingGuestId.Value);
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
        private void Close()
        {
            CloseAction?.Invoke();
        }

        private void ResetForm()
        {
            FirstName = "";
            LastName = "";
            Phone = "";
            Email = "";
            IdNumber = "";
            Address = "";
            Notes = "";
            ErrorMessage = "";
            Reservations = [];
            HasError = false;
            OnPropertyChanged(nameof(HasNoReservations));
            OnPropertyChanged(nameof(HasReservations));
        }

    }
}
