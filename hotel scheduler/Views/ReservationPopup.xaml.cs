using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class ReservationPopup : Popup
    {
        private readonly ReservationViewModel _viewModel;
        private readonly GuestPopupViewModel _guestPopupViewModel;
        private readonly IServiceProvider _serviceProvider;

        // Provide a local Close() helper so the ViewModel can request the
        // popup to close. Some CommunityToolkit versions expose a Close/Dismiss
        // implementation on the base Popup type differently; using a small
        // reflection-backed shim here makes the call robust across versions.

        public ReservationPopup(ReservationViewModel viewModel, GuestPopupViewModel guestPopupViewmodel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _guestPopupViewModel = guestPopupViewmodel;
            _serviceProvider = serviceProvider;
            BindingContext = viewModel;

            // Close command wired here since Popup.Close() lives in code-behind
            viewModel.CloseAction = () => MainThread.BeginInvokeOnMainThread(() => CloseAsync());
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            CloseAsync();
        }

        private async void OnAddGuestClicked(object sender, EventArgs e)
        {
            _guestPopupViewModel.InitializeGuestPopup(_viewModel.GuestSearchQuery);
            _guestPopupViewModel.OnGuestCreated = (guest) =>
            {
                _viewModel.SelectGuestCommand.Execute(guest);
            };

            var popup = new GuestPopup(_guestPopupViewModel,
                _serviceProvider.GetRequiredService<ReservationViewModel>(),
                _serviceProvider);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }



    }
}