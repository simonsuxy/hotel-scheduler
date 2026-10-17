using hotel_scheduler.ViewModels;

namespace hotel_scheduler.Views
{
    public partial class UsersView : ContentView
    {
        private readonly UsersViewModel _viewModel;

        public UsersView(UsersViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }
    }
}