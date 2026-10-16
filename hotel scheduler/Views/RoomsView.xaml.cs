using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class RoomsView : ContentView
    {
        private readonly RoomsViewModel _viewModel;

        public RoomsView(RoomsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }
    }
}