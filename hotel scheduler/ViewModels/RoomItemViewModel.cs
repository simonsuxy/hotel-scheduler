using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using DAL.Entities;
using hotel_scheduler.Models.Entities;

namespace hotel_scheduler.ViewModels
{
    public partial class RoomItemViewModel : ObservableObject
    {
        public RoomDTO OriginalDTO { get; }

        public int Id => OriginalDTO.Id;

        // View mode display properties
        public string Name => OriginalDTO.Name;
        public string Type => OriginalDTO.Type;
        public string Status => OriginalDTO.Status.ToString();
        public decimal PricePerNight => OriginalDTO.PricePerNight;
        public int Capacity => OriginalDTO.Capacity;
        public string Notes => OriginalDTO.Notes;

        [ObservableProperty] public partial bool IsEditing { get; set; } = false;
        public bool HasReservations => OriginalDTO.HasReservations;
        public bool HasNoReservations => !OriginalDTO.HasReservations;
        public bool CanDelete => !IsEditing && HasNoReservations;

        // Draft properties for on-page editing
        [ObservableProperty] public partial string EditName { get; set; } = "";
        [ObservableProperty] public partial string EditType { get; set; } = "Single";
        [ObservableProperty] public partial string EditStatus { get; set; } = "Active";
        [ObservableProperty] public partial string EditPriceText { get; set; } = "0.00";
        [ObservableProperty] public partial int EditCapacity { get; set; } = 1;
        [ObservableProperty] public partial string EditNotes { get; set; } = "";

        
        public RoomItemViewModel(RoomDTO dto)
        {
            OriginalDTO = dto;
            RevertEdits();
        }

        public bool TryGetParsedPrice(out decimal price)
        {
            price = 0m;
            if (string.IsNullOrWhiteSpace(EditPriceText))
                return false;

            string safeText = EditPriceText.Replace(',', '.');
            return decimal.TryParse(safeText, NumberStyles.Any, CultureInfo.InvariantCulture, out price);
        }

        public void CommitEdits(decimal price, RoomStatus status)
        {
            OriginalDTO.Name = EditName.Trim();
            OriginalDTO.Type = EditType;
            OriginalDTO.PricePerNight = price;
            OriginalDTO.Capacity = EditCapacity;
            OriginalDTO.Status = status;
            OriginalDTO.Notes = EditNotes?.Trim() ?? "";

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(PricePerNight));
            OnPropertyChanged(nameof(Capacity));
            OnPropertyChanged(nameof(Notes));

            RevertEdits();
        }

        public void RevertEdits()
        {
            EditName = OriginalDTO.Name;
            EditType = string.IsNullOrWhiteSpace(OriginalDTO.Type) ? "Single" : OriginalDTO.Type;
            EditStatus = OriginalDTO.Status.ToString();
            EditPriceText = OriginalDTO.PricePerNight.ToString("0.00", CultureInfo.InvariantCulture);
            EditCapacity = OriginalDTO.Capacity < 1 ? 1 : OriginalDTO.Capacity;
            EditNotes = OriginalDTO.Notes ?? "";
        }
    }
}