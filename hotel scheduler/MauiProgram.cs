using CommunityToolkit.Maui;
using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Services;
using hotel_scheduler.Models.Services.DatabaseServices;
using hotel_scheduler.ViewModels;
using hotel_scheduler.Views;
using Microsoft.EntityFrameworkCore;
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
            builder.Services.AddTransient<FinanceService>();
            builder.Services.AddSingleton<AuthService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddSingleton<CalendarViewModel>();  // Changed to Singleton to preserve state across view recreations
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<ReservationViewModel>();
            builder.Services.AddTransient<GuestPopupViewModel>();
            builder.Services.AddTransient<DateRangePickerViewModel>();
            builder.Services.AddTransient<GuestViewModel>();
            builder.Services.AddTransient<FinanceViewModel>();



            // Views
            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<RegisterView>();
            builder.Services.AddTransient<CalendarView>();
            builder.Services.AddTransient<ReservationPopup>();
            builder.Services.AddTransient<GuestPopup>();
            builder.Services.AddTransient<DateRangePickerPopup>();
            builder.Services.AddTransient<GuestsView>();
            builder.Services.AddTransient<FinanceView>();
            builder.Services.AddTransient<MainView>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<HotelSchedulerDbContext>();

                // Automatically applies pending migrations and creates the DB if missing
                //db.Database.EnsureDeleted();
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