using CommunityToolkit.Mvvm.ComponentModel;
using hotel_scheduler.Models.Entities;
using DAL.Entities;

namespace hotel_scheduler.ViewModels
{
    public partial class ChargeCategoryItemViewModel: ObservableObject
    {
        public ChargeCategoryDTO OriginalDTO { get; private set; }
        [ObservableProperty] public partial bool IsEditing { get; set; } = false;
        [ObservableProperty] public partial string EditName { get; set; } = "";
        [ObservableProperty] public partial string EditDescription { get; set; } = "";
        [ObservableProperty] public partial decimal EditPrice { get; set; }
        [ObservableProperty] public partial ChargeType EditType { get; set; }
        public bool IsPreCreated => OriginalDTO.IsPreCreated;

        public ChargeCategoryItemViewModel(ChargeCategoryDTO dto)
        {
            OriginalDTO = dto;
            EditName = dto.Name;
            EditDescription = dto.Description;
            EditPrice = dto.DefaultPricePerNight;
            EditType = dto.Type;
        }

        public void CommitEdits()
        {
            OriginalDTO.Name = EditName;
            OriginalDTO.Description = EditDescription;
            OriginalDTO.DefaultPricePerNight = EditPrice;
            OriginalDTO.Type = EditType;
        }

        public void RevertEdits()
        {
            EditName = OriginalDTO.Name;
            EditDescription = OriginalDTO.Description;
            EditPrice = OriginalDTO.DefaultPricePerNight;
            EditType = OriginalDTO.Type;
        }
    }
}
