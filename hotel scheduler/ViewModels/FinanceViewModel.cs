using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;
using System.Globalization;
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
            try
            {
                HasError = false;
                ErrorMessage = "";

                await LoadCategoriesAsync();
                await LoadFinancialsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to initialize finance data: {ex.Message}";
                HasError = true;
            }
        }

        public async Task LoadCategoriesAsync()
        {
            var categories = await _financeService.GetCategoriesAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Categories.Clear();
                foreach (var cat in categories)
                {
                    Categories.Add(new ChargeCategoryItemViewModel(cat));
                }
            });
        }

        public async Task LoadFinancialsAsync()
        {
            var reservations = await _reservationService.GetReservationsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Reservations.Clear();
                decimal total = 0m, paid = 0m, pending = 0m, extras = 0m;

                foreach (var r in reservations)
                {
                    Reservations.Add(r);
                    total += r.ComputedTotalPrice;
                    extras += r.TotalChargesPrice;

                    if (r.PaymentStatus == PaymentStatus.Paid)
                    {
                        paid += r.ComputedTotalPrice;
                    }
                    else if (r.PaymentStatus == PaymentStatus.Unpaid)
                    {
                        pending += r.ComputedTotalPrice;
                    }
                    else if (r.PaymentStatus == PaymentStatus.PartiallyPaid)
                    {
                        paid += r.ComputedTotalPrice / 2;
                        pending += r.ComputedTotalPrice / 2;
                    }
                }

                TotalRevenue = total;
                CollectedIncome = paid;
                PendingBalance = pending;
                TotalExtrasRevenue = extras;
            });
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            HasError = false;
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                ErrorMessage = "Category name is required.";
                HasError = true;
                return;
            }
            string safePriceText = NewCategoryPrice?.Replace(',', '.') ?? "0";
            if (!decimal.TryParse(safePriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                ErrorMessage = "Please enter a valid price.";
                HasError = true;
                return;
            }
            try
            {
                await _financeService.CreateCategoryAsync(NewCategoryName, price, NewCategoryType, NewCategoryDescription);

                NewCategoryName = "";
                NewCategoryPrice = "";
                NewCategoryDescription = "";
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving category: {ex.Message}";
                HasError = true;
            }
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
