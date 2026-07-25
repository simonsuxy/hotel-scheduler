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

        public void InitializeGuestPopup(string prefilledName = "")
        {
            ResetForm();
            if(!string.IsNullOrWhiteSpace(prefilledName))
            {
                var parts = prefilledName.Trim().Split('.',2);
                FirstName = parts[0];
                LastName = parts.Length > 1 ? parts[1] : "";
            }
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
                var guest = await _guestService.CreateAsync(FirstName, LastName,
                                        Email, Phone, IdNumber, Address, Notes);
                OnGuestCreated?.Invoke(guest);
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
            HasError = false;
        }

    }
}
