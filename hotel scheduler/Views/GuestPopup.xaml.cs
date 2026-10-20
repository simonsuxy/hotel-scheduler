using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using hotel_scheduler.Models.Services.DatabaseServices;
using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class GuestPopup : Popup
    {
        private readonly GuestPopupViewModel _viewModel;
        private readonly ReservationViewModel _reservationViewModel;
        private readonly IServiceProvider _serviceProvider;

        public GuestPopup(GuestPopupViewModel viewModel,
                          ReservationViewModel reservationViewModel,
                          IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _reservationViewModel = reservationViewModel;
            _serviceProvider = serviceProvider;
            BindingContext = viewModel;

            viewModel.CloseAction = () =>
                MainThread.BeginInvokeOnMainThread(() => _ = this.CloseAsync());

            viewModel.OnReservationTapped = async (reservation) =>
            {
                var roomService = _serviceProvider.GetRequiredService<RoomService>();
                var guestService = _serviceProvider.GetRequiredService<GuestService>();
                var allRooms = await roomService.GetAllActiveRoomAsync();

                if (reservation.GuestId == 0 && viewModel.EditingGuestId.HasValue)
                {
                    reservation.GuestId = viewModel.EditingGuestId.Value;
                }

                _reservationViewModel.SetRooms(allRooms);
                await _reservationViewModel.InitializeEditReservationPopup(reservation);

                _reservationViewModel.OnSaved = async () =>
                {
                    viewModel.OnSaved?.Invoke();
                    var updatedGuests = await guestService.GetAllGuestsAsync();
                    var updatedGuest = updatedGuests.FirstOrDefault(g => g.Id == viewModel.EditingGuestId);
                    if (updatedGuest != null)
                    {
                        viewModel.InitializeEditGuestPopup(updatedGuest);
                    }
                };

                var guestPopupViewModel = _serviceProvider.GetRequiredService<GuestPopupViewModel>();
                var popup = new ReservationPopup(
                    _reservationViewModel,
                    guestPopupViewModel,
                    _serviceProvider);

                await Application.Current!.MainPage!.ShowPopupAsync(popup);
            };
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            _ = this.CloseAsync();
        }
    }
}