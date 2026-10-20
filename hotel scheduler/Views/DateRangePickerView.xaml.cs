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

            _viewModel.CloseAction = async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await CloseAsync());
            };
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await CloseAsync();
        }
    }
}