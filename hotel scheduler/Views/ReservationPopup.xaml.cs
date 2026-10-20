using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class ReservationPopup : Popup
    {
        private readonly ReservationViewModel _viewModel;
        private readonly GuestPopupViewModel _guestPopupViewModel;
        private readonly IServiceProvider _serviceProvider;

        public ReservationPopup(ReservationViewModel viewModel, GuestPopupViewModel guestPopupViewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _guestPopupViewModel = guestPopupViewModel;
            _serviceProvider = serviceProvider;
            BindingContext = viewModel;

            viewModel.CloseAction = () => MainThread.BeginInvokeOnMainThread(() => _ = CloseAsync());
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            _ = CloseAsync();
        }

        private async void OnAddGuestClicked(object sender, EventArgs e)
        {
            _guestPopupViewModel.InitializeGuestPopup(_viewModel.GuestSearchQuery);
            _guestPopupViewModel.OnGuestCreated = (guest) =>
            {
                _viewModel.SelectGuestCommand.Execute(guest);
            };

            var popup = new GuestPopup(
                _guestPopupViewModel,
                _serviceProvider.GetRequiredService<ReservationViewModel>(),
                _serviceProvider);

            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }

        private void OnQuantityCompleted(object sender, EventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is ReservationChargeDTO charge)
            {
                _viewModel.UpdateChargeQuantityCommand.Execute(charge);
            }
        }
    }
}