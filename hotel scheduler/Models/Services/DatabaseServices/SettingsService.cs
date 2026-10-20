using DAL;
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
                HotelName = settings.HotelName,
                Address = settings.Address,
                Phone = settings.Phone,
                Currency = settings.Currency,
                DefaultCalendarDays = settings.DefaultCalendarDays,
                DefaultCheckInTime = settings.DefaultCheckInTime,
                DefaultCheckOutTime = settings.DefaultCheckOutTime,
            };
        }

        public async Task UpdateSettingsAsync(SettingsDTO dto)
        {
            var settings = await _db.Settings.SingleAsync();
            settings.HotelName = dto.HotelName.Trim();
            settings.Address = dto.Address?.Trim();
            settings.Phone = dto.Phone?.Trim();
            settings.Currency = dto.Currency.Trim();
            settings.DefaultCalendarDays = dto.DefaultCalendarDays;
            settings.DefaultCheckInTime = dto.DefaultCheckInTime;
            settings.DefaultCheckOutTime = dto.DefaultCheckOutTime;
            await _db.SaveChangesAsync();
        }
    }
}
