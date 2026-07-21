using hotel_scheduler.Views;

namespace hotel_scheduler
{
    public partial class App : Application
    {
        public App(LoginView loginView)
        {
            InitializeComponent();
            MainPage = loginView;
        }
    }
}