using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;


namespace hotel_scheduler.ViewModels
{
    public partial class UsersViewModel(UserService userService): ObservableObject
    {
        private readonly UserService _userService = userService;

        public ObservableCollection<UserItemViewModel> Users { get; } = [];

        [ObservableProperty] public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty] public partial bool HasError { get; set; } = false;

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                HasError = false;
                ErrorMessage = "";

                var users = await _userService.GetAllUsersAsync();

                Users.Clear();
                foreach (var u in users)
                    Users.Add(new UserItemViewModel(u));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load users";
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task ToggleRoleAsync(UserItemViewModel user)
        {
            if (user == null || user.IsFirstUser) return;

            try
            {
                var newRole = user.Role == UserRole.Admin ? UserRole.Staff : UserRole.Admin;
                await _userService.SetUserRoleAsync(user.Id, newRole);

                user.OriginalDTO.Role = newRole;
                user.UpdateFromDTO();
            }
            catch(Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task ToggleStatusAsync(UserItemViewModel user)
        {
            if (user == null || user.IsFirstUser) return;
            try
            {
                if (user.IsActive)
                    await _userService.DeactivateUserAsync(user.Id);
                else
                    await _userService.ActivateUserAsync(user.Id);
                user.OriginalDTO.IsActive = !user.IsActive;
                user.UpdateFromDTO();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
        }
    }
}
