using hotel_scheduler.Views;

namespace hotel_scheduler
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        protected override async void OnStart()
        {
            await Shell.Current.GoToAsync("//login");
            base.OnStart();
        }
    }
}