using System;
using System.Collections.Generic;
using System.Text;

namespace hotel_scheduler.Models.Entities
{
    public class GuestDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? IdNumber { get; set; }
        public string? Address { get; set; }
        public string Notes { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}";
        public List<ReservationDTO> Reservations { get; set; } = [];

        public string StatusLabel
        {
            get
            {
                var now = DateTime.Now;
                if (Reservations.Any(r => r.CheckIn <= now && r.CheckOut >= now))
                    return "Current";
                if (Reservations.Any(r => r.CheckIn > now))
                    return "Future";
                if (Reservations.Any(r => r.CheckOut < now))
                    return "Past";
                return "None";
            }
            private set { }
        }
        public string StatusColor => StatusLabel switch
        {
            "Current" => "#43A047",  // green
            "Future" => "#4C4EE7",  // blue
            "Past" => "#FB8C00",  // orange
            _ => "#CCCCCC"   // gray
        };


    }
}
