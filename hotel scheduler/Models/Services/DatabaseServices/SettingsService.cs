using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
    public class SettingsService(HotelSchedulerDbContext db)
    {
        private readonly HotelSchedulerDbContext _db = db;

        public async Task<SettingsDTO> GetSettingsAsync()
        {
            var settings = await _db.Settings.SingleAsync();
            return new SettingsDTO
            {
                Id = settings.Id,
                HotelName = settings.HotelName ?? "Hotel",
                Address = settings.Address ?? "",
                Phone = settings.Phone ?? "",
                Email = settings.Email ?? "",
                Currency = settings.Currency ?? "EUR",
                DefaultCalendarDays = settings.DefaultCalendarDays,
                DefaultCheckInTime = settings.DefaultCheckInTime,
                DefaultCheckOutTime = settings.DefaultCheckOutTime,
            };
        }

        public async Task UpdateSettingsAsync(SettingsDTO dto)
        {
            var settings = await _db.Settings.SingleOrDefaultAsync();
            if (settings == null)
            {
                settings = new Settings { Id = 1 };
                _db.Settings.Add(settings);
            }
            settings.HotelName = dto.HotelName.Trim() ?? "Hotel";
            settings.Address = dto.Address?.Trim() ?? "";
            settings.Phone = dto.Phone?.Trim() ?? "";
            settings.Currency = dto.Currency.Trim() ?? "EUR";
            settings.Email = dto.Email?.Trim() ?? "";
            settings.DefaultCalendarDays = dto.DefaultCalendarDays;
            settings.DefaultCheckInTime = dto.DefaultCheckInTime;
            settings.DefaultCheckOutTime = dto.DefaultCheckOutTime;
            await _db.SaveChangesAsync();

        }
    }
}
