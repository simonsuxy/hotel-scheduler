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
            var settings = await _db.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new Settings { Id = 1, HotelName = "Hotel", Currency = "EUR", DefaultCalendarDays = 14 };
                _db.Settings.Add(settings);
                await _db.SaveChangesAsync();
            }
            return new SettingsDTO
            {
                Id = settings.Id,
                HotelName = string.IsNullOrWhiteSpace(settings.HotelName) ? "Hotel" : settings.HotelName,
                Address = settings.Address ?? "",
                Phone = settings.Phone ?? "",
                Email = settings.Email ?? "",
                Currency = string.IsNullOrWhiteSpace(settings.Currency) ? "EUR" : settings.Currency,
                DefaultCalendarDays = settings.DefaultCalendarDays <= 0 ? 14 : settings.DefaultCalendarDays,
                DefaultCheckInTime = settings.DefaultCheckInTime == default ? new TimeSpan(14, 0, 0) : settings.DefaultCheckInTime,
                DefaultCheckOutTime = settings.DefaultCheckOutTime == default ? new TimeSpan(10, 0, 0) : settings.DefaultCheckOutTime
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
            settings.HotelName = string.IsNullOrWhiteSpace(dto.HotelName) ? "Hotel" : dto.HotelName.Trim();
            settings.Address = dto.Address?.Trim() ?? "";
            settings.Phone = dto.Phone?.Trim() ?? "";
            settings.Email = dto.Email?.Trim() ?? "";
            settings.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "EUR" : dto.Currency.Trim();
            settings.DefaultCalendarDays = dto.DefaultCalendarDays <= 0 ? 14 : dto.DefaultCalendarDays;
            settings.DefaultCheckInTime = dto.DefaultCheckInTime;
            settings.DefaultCheckOutTime = dto.DefaultCheckOutTime;

            await _db.SaveChangesAsync();

        }
    }
}
