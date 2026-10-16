using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;

namespace hotel_scheduler.ViewModels
{
    public partial class RoomsViewModel(RoomService roomService) : ObservableObject
    {
        private readonly RoomService _roomService = roomService;

        public ObservableCollection<RoomItemViewModel> Rooms { get; } = [];
        public ObservableCollection<string> RoomTypes { get; } = ["Single", "Double", "Suite"];
        public ObservableCollection<string> RoomStatuses { get; } = ["Active", "Maintenance", "Inactive"];

        // Form fields for creating a new room
        [ObservableProperty] public partial string NewRoomName { get; set; } = "";
        [ObservableProperty] public partial string NewRoomType { get; set; } = "Single";
        [ObservableProperty] public partial string NewRoomPrice { get; set; } = "";
        [ObservableProperty] public partial int NewRoomCapacity { get; set; } = 1;
        [ObservableProperty] public partial string NewRoomStatus { get; set; } = "Active";
        [ObservableProperty] public partial string NewRoomNotes { get; set; } = "";

        // Field for adding a new RoomType
        [ObservableProperty] public partial string CustomRoomTypeName { get; set; } = "";

        // Error handling
        [ObservableProperty] public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty] public partial bool HasError { get; set; } = false;

        public async Task InitializeAsync()
        {
            await LoadRoomsAsync();
        }

        public async Task LoadRoomsAsync()
        {
            var roomDtos = await _roomService.GetRoomsAsync();
            Rooms.Clear();

            foreach (var dto in roomDtos)
            {
                if (!RoomTypes.Contains(dto.Type) && !string.IsNullOrWhiteSpace(dto.Type))
                    RoomTypes.Add(dto.Type);

                Rooms.Add(new RoomItemViewModel(dto));
            }
        }

        [RelayCommand]
        private void AddRoomType()
        {
            HasError = false;
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(CustomRoomTypeName))
            {
                ErrorMessage = "Enter a valid room type name.";
                HasError = true;
                return;
            }

            var formattedType = CustomRoomTypeName.Trim();
            if (!RoomTypes.Contains(formattedType))
            {
                RoomTypes.Add(formattedType);
                NewRoomType = formattedType;
            }

            CustomRoomTypeName = "";
        }

        [RelayCommand]
        private async Task CreateRoomAsync()
        {
            HasError = false;
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(NewRoomName))
            {
                ErrorMessage = "Room name is required.";
                HasError = true;
                return;
            }

            string safePrice = NewRoomPrice?.Replace(',', '.') ?? "0";
            if (!decimal.TryParse(safePrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                ErrorMessage = "Please enter a valid price per night.";
                HasError = true;
                return;
            }

            if (NewRoomCapacity < 1)
            {
                ErrorMessage = "Capacity must be at least 1.";
                HasError = true;
                return;
            }

            Enum.TryParse<RoomStatus>(NewRoomStatus, true, out var statusEnum);

            try
            {
                await _roomService.CreateRoomAsync(
                    NewRoomName,
                    "",
                    NewRoomType,
                    NewRoomCapacity,
                    statusEnum,
                    price,  
                    NewRoomNotes);

                // Reset form
                NewRoomName = "";
                NewRoomPrice = "";
                NewRoomCapacity = 1;
                NewRoomNotes = "";
                NewRoomStatus = "Active";

                await LoadRoomsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
        }

        [RelayCommand]
        private void ToggleEditRoom(RoomItemViewModel room)
        {
            if (room == null) return;

            room.RevertEdits();

            if (!string.IsNullOrWhiteSpace(room.EditType) && !RoomTypes.Contains(room.EditType))
            {
                RoomTypes.Add(room.EditType);
            }

            room.IsEditing = !room.IsEditing;
        }

        [RelayCommand]
        private async Task SaveEditedRoomAsync(RoomItemViewModel room)
        {
            if (room == null) return;
            HasError = false;
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(room.EditName))
            {
                ErrorMessage = "Room name cannot be empty.";
                HasError = true;
                return;
            }

            if (!room.TryGetParsedPrice(out decimal price) || price < 0)
            {
                ErrorMessage = "Please enter a valid price per night.";
                HasError = true;
                return;
            }

            if (room.EditCapacity < 1)
            {
                ErrorMessage = "Capacity must be at least 1.";
                HasError = true;
                return;
            }

            Enum.TryParse<RoomStatus>(room.EditStatus, true, out var statusEnum);

            try
            {
                await _roomService.UpdateRoomAsync(
                    room.Id,
                    room.EditName,
                    "",
                    room.EditType,
                    room.EditCapacity,
                    statusEnum,
                    price,           
                    room.EditNotes);

                room.CommitEdits(price, statusEnum);
                room.IsEditing = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save: {ex.Message}";
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task DeleteRoomAsync(RoomItemViewModel room)
        {
            if (room == null || !room.HasNoReservations) return;
            HasError = false;
            ErrorMessage = "";

            try
            {
                await _roomService.DeleteRoomAsync(room.Id);
                Rooms.Remove(room);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete room: {ex.Message}";
                HasError = true;
            }
        }
    }
}