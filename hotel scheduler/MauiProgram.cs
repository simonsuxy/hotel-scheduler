using CommunityToolkit.Maui;
using DAL;
using hotel_scheduler.Models.Services;
using hotel_scheduler.Models.Services.DatabaseServices;
using hotel_scheduler.ViewModels;
using hotel_scheduler.Views;
using Microsoft.Extensions.Logging;

namespace hotel_scheduler
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();

            builder.Services.AddDbContext<HotelSchedulerDbContext>();
            builder.Services.AddTransient<UserService>();
            builder.Services.AddTransient<GuestService>();
            builder.Services.AddTransient<RoomService>();
            builder.Services.AddTransient<SettingsService>();
            builder.Services.AddSingleton<AuthService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();

            // Views
            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<RegisterView>();
            builder.Services.AddTransient<MainView>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();

            // Creates data.sqlite automatically on first launch if it doesn't exist
            // No migrations needed during development
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<HotelSchedulerDbContext>();
                db.Database.EnsureCreated();
            }

            return app;
        }
    }
}