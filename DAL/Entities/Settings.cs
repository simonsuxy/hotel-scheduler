using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Settings
    {
        public int Id { get; set; } = 1;
        public string HotelName { get; set; } = "Hotel";
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Currency { get; set; } = "EUR";
        public int DefaultCalendarDays { get; set; } = 14;
        public int CancellationPolicyDays { get; set; } = 2;
        
    }
}
