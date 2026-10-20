using System;
using System.Collections.Generic;
using System.Text;

namespace hotel_scheduler.Models.Entities
{
    public class SettingsDTO
    {
        public int Id { get; set; } = 1;
        public string HotelName { get; set; } = "Hotel";
        public string? Address { get; set; }
        public string? Phone { get; set; } 
        public string Currency { get; set; } = "EUR";
        public int DefaultCalendarDays { get; set; } = 14;
        public TimeSpan DefaultCheckInTime { get; set; } = new TimeSpan(14, 0, 0);
        public TimeSpan DefaultCheckOutTime { get; set; } = new TimeSpan(10, 0, 0);
    }
}
