using hotel_scheduler.ViewModels;


namespace hotel_scheduler.Views
{
    public partial class GuestsView : ContentView
    {
        private readonly GuestViewModel _viewModel;

        public GuestsView(GuestViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }
    }
}