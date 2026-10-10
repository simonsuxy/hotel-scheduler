using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class FinanceView : ContentView
    {
        private readonly FinanceViewModel _viewModel;

        public FinanceView(FinanceViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }
    }
}