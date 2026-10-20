using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace hotel_scheduler.ViewModels
{
    public partial class DateRangePickerViewModel : ObservableObject
    {
        public Action<DateTime, DateTime>? OnRangeSelected { get; set; }
        public Func<Task>? CloseAction { get; set; }

        [ObservableProperty]
        public partial DateTime SelectedStartDate { get; set; } = DateTime.Today;

        [ObservableProperty]
        public partial DateTime SelectedEndDate { get; set; } = DateTime.Today.AddDays(6);

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";

        [ObservableProperty]
        public partial bool HasError { get; set; } = false;

        public int SelectedDayCount => (SelectedEndDate - SelectedStartDate).Days + 1;
        public string DayCountLabel => $"{SelectedDayCount} days selected";
        public string DayCountColor => SelectedDayCount is >= 5 and <= 28
            ? "#43A047"  // Green — valid
            : "#E53935"; // Red — out of range

        public void Initialize(DateTime currentStart, DateTime currentEnd)
        {
            SelectedStartDate = currentStart;
            SelectedEndDate = currentEnd;
            ErrorMessage = "";
            HasError = false;
            RefreshLabels();
        }

        partial void OnSelectedStartDateChanged(DateTime value) => RefreshLabels();
        partial void OnSelectedEndDateChanged(DateTime value) => RefreshLabels();

        private void RefreshLabels()
        {
            OnPropertyChanged(nameof(SelectedDayCount));
            OnPropertyChanged(nameof(DayCountLabel));
            OnPropertyChanged(nameof(DayCountColor));
        }

        [RelayCommand]
        private async Task ConfirmAsync()
        {
            HasError = false;
            ErrorMessage = "";

            if (SelectedEndDate <= SelectedStartDate)
            {
                ErrorMessage = "End date must be after start date.";
                HasError = true;
                return;
            }

            if (SelectedDayCount < 5)
            {
                ErrorMessage = "Minimum range is 5 days.";
                HasError = true;
                return;
            }

            if (SelectedDayCount > 28)
            {
                ErrorMessage = "Maximum range is 28 days.";
                HasError = true;
                return;
            }

            OnRangeSelected?.Invoke(SelectedStartDate, SelectedEndDate);
            if (CloseAction != null)
                await CloseAction.Invoke();
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            if (CloseAction != null)
                await CloseAction.Invoke();
        }
    }
}