using CommunityToolkit.Maui.Views;
using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class GuestPopup : Popup
    {
        private readonly GuestPopupViewModel _viewModel;

        public GuestPopup(GuestPopupViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            viewModel.CloseAction = () =>
                MainThread.BeginInvokeOnMainThread(() => _ = this.CloseAsync());
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            _ = this.CloseAsync();
        }
    }
}