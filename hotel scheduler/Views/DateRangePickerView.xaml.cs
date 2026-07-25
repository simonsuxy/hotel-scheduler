using CommunityToolkit.Maui.Views;
using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class DateRangePickerPopup : Popup
    {
        private readonly DateRangePickerViewModel _viewModel;

        public DateRangePickerPopup(DateRangePickerViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            viewModel.CloseAction = async () =>
            {
                // Ensure CloseAsync is run on the main thread and awaited
                await MainThread.InvokeOnMainThreadAsync(async () => await CloseAsync());
            };
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await CloseAsync();
        }
    }
}