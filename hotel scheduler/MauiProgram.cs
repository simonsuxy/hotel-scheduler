using CommunityToolkit.Maui;
using DAL;
using DAL.Entities;
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
            }).UseMauiCommunityToolkit(options =>
            {
                options.SetShouldEnableSnackbarOnWindows(false);
            });

            builder.Services.AddDbContext<HotelSchedulerDbContext>();
            builder.Services.AddTransient<UserService>();
            builder.Services.AddTransient<GuestService>();
            builder.Services.AddTransient<RoomService>();
            builder.Services.AddTransient<SettingsService>();
            builder.Services.AddTransient<ReservationService>();
            builder.Services.AddSingleton<AuthService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<CalendarViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<ReservationViewModel>();


            // Views
            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<RegisterView>();
            builder.Services.AddTransient<CalendarView>();
            builder.Services.AddTransient<ReservationPopup>();
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
                if (!db.Rooms.Any())
                {
                    db.Rooms.AddRange(
                        new Room { Name = "101", Description = "Standard single", Type = RoomType.Single, Capacity = 1, PricePerNight = 60m, Status = RoomStatus.Active },
                        new Room { Name = "102", Description = "Standard double", Type = RoomType.Double, Capacity = 2, PricePerNight = 90m, Status = RoomStatus.Active },
                        new Room { Name = "201", Description = "Suite", Type = RoomType.Suite, Capacity = 3, PricePerNight = 150m, Status = RoomStatus.Active },
                        new Room { Name = "202", Description = "Standard double", Type = RoomType.Double, Capacity = 2, PricePerNight = 90m, Status = RoomStatus.Active }
                    );
                    db.SaveChanges();
                }
            }

            return app;
        }
    }
}