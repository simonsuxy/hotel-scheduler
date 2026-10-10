using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;
namespace hotel_scheduler.ViewModels
{
    public partial class FinanceViewModel(ReservationService reservationService, FinanceService financeService) : ObservableObject
    {
        private readonly ReservationService _reservationService = reservationService;
        private readonly FinanceService _financeService = financeService;

        public ObservableCollection<ReservationDTO> Reservations { get; } = [];
        public ObservableCollection<ChargeCategoryItemViewModel> Categories { get; } = [];
        public ObservableCollection<ChargeType> ChargeTypes { get; } = [ChargeType.PerNight, ChargeType.Flat];

        [ObservableProperty] public partial decimal TotalRevenue { get; set; }
        [ObservableProperty] public partial decimal CollectedIncome { get; set; }
        [ObservableProperty] public partial decimal PendingBalance { get; set; }
        [ObservableProperty] public partial decimal TotalExtrasRevenue { get; set; }
        [ObservableProperty] public partial string NewCategoryName { get; set; } = "";
        [ObservableProperty] public partial string NewCategoryPrice { get; set; } = "";
        [ObservableProperty] public partial string NewCategoryDescription { get; set; } = "";
        [ObservableProperty] public partial ChargeType NewCategoryType { get; set; } = ChargeType.PerNight;
        [ObservableProperty] public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty] public partial bool HasError { get; set; } = false;

        public async Task InitializeAsync()
        {
            await LoadCategoriesAsync();
            await LoadFinancialsAsync();


        }

        public async Task LoadCategoriesAsync()
        {
            var categories = await _financeService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var cat in categories)
            {
                Categories.Add(new ChargeCategoryItemViewModel(cat));
            }
        }

        public async Task LoadFinancialsAsync()
        {

            var reservations = await _reservationService.GetReservationsWithChargesAsync();


            Reservations.Clear();
            decimal total = 0m, paid = 0m, pending = 0m, extras = 0m;

            foreach (var r in reservations)
            {
                Reservations.Add(r);
                total += r.ComputedTotalPrice;
                extras += r.TotalChargesPrice;

                if (r.PaymentStatus == DAL.Entities.PaymentStatus.Paid)
                    paid += r.ComputedTotalPrice;
                else if (r.PaymentStatus == DAL.Entities.PaymentStatus.Unpaid)
                    pending += r.ComputedTotalPrice;
                else if (r.PaymentStatus == DAL.Entities.PaymentStatus.PartiallyPaid)
                {
                    paid += r.ComputedTotalPrice / 2;
                    pending += r.ComputedTotalPrice / 2;
                }
            }

            TotalRevenue = total;
            CollectedIncome = paid;
            PendingBalance = pending;
            TotalExtrasRevenue = extras;



        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName) || !decimal.TryParse(NewCategoryPrice, out decimal price))
                return;

            try
            {
                await _financeService.CreateCategoryAsync(NewCategoryName, price, NewCategoryType, NewCategoryDescription);

                NewCategoryName = "";
                NewCategoryPrice = "";
                NewCategoryDescription = "";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving category: {ex.Message}";
                HasError = true;
            }
            await LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(ChargeCategoryItemViewModel item)
        {
            if (item == null || item.IsPreCreated) return;
            try
            {
                await _financeService.DeleteCategoryAsync(item.OriginalDTO.Id);
                Categories.Remove(item);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
            
        }

        [RelayCommand]
        private void ToggleEditCategory(ChargeCategoryItemViewModel category)
        {
            if (category == null) return;
            if (category.IsEditing)
            {
                category.RevertEdits();
            }
            category.IsEditing = !category.IsEditing;
        }

        [RelayCommand]
        private async Task SaveEditedCategoryAsync(ChargeCategoryItemViewModel item)
        {
            if (item == null) return;
            HasError = false;
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(item.EditName))
            {
                ErrorMessage = "Category name is required.";
                HasError = true;
                return;
            }

            try
            {
                await _financeService.UpdateCategoryAsync(
                    item.OriginalDTO.Id,
                    item.EditName,
                    item.EditPrice,
                    item.EditType,
                    item.EditDescription);
                item.CommitEdits();
                item.IsEditing = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving category: {ex.Message}";
                HasError = true;
            }
        }
    }
}
