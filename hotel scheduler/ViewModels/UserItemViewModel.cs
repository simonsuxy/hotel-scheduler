using CommunityToolkit.Mvvm.ComponentModel;
using DAL.Entities;
using hotel_scheduler.Models.Entities;

namespace hotel_scheduler.ViewModels
{
    public partial class UserItemViewModel: ObservableObject
    {
        public UserDTO OriginalDTO { get; }

        public int Id => OriginalDTO.Id;
        public string Username => OriginalDTO.Username;
        public DateTime CreatedAt => OriginalDTO.CreatedAt;
        public bool IsFirstUser => OriginalDTO.Id == 1;
        public bool IsNotFirstUser => !IsFirstUser;
        [ObservableProperty] public partial UserRole Role { get; set; }
        [ObservableProperty] public partial bool IsActive { get; set; }

        public string RoleLabel => Role == UserRole.Admin ? "Admin" : "Staff";
        public string StatusLabel => IsActive ? "Active" : "Deactivated";
        public string StatusColor => IsActive ? "#43A047" : "#E53935";

        public string ToggleRoleActionText => Role == UserRole.Admin ? "Demote" : "Make Admin";
        public string ToggleStatusActionText => IsActive ? "Deactivate" : "Activate";
        public string ToggleStatusActionColor => IsActive ? "#E53935" : "#43A047";

        public UserItemViewModel(UserDTO dto)
        {
            OriginalDTO = dto;
            Role = dto.Role;
            IsActive = dto.IsActive;
        }

        public void UpdateFromDTO()
        {
            Role = OriginalDTO.Role;
            IsActive = OriginalDTO.IsActive;

            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(StatusLabel));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(ToggleRoleActionText));
            OnPropertyChanged(nameof(ToggleStatusActionText));
            OnPropertyChanged(nameof(ToggleStatusActionColor));
        }



    }
}
