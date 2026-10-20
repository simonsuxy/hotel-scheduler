using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class SettingsView : ContentView
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }
    }
}