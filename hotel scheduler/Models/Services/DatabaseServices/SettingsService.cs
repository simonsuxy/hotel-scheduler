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
                DefaultCalendarDays = settings.DefaultCalendarDays
            };
        }

        public async Task UpdateSettingsAsync(string hotelName, string address,
                                    string phone, string currency, int defaultCalendarDays)
        {
            var settings = await _db.Settings.SingleAsync();
            settings.HotelName = hotelName.Trim();
            settings.Address = address.Trim();
            settings.Phone = phone.Trim();
            settings.Currency = currency.Trim();
            settings.DefaultCalendarDays = defaultCalendarDays;
            await _db.SaveChangesAsync();
        }
    }
}
