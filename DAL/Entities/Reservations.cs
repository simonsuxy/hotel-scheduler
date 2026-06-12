using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Reservations
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int GuestId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestCount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string Notes { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public Room Room { get; set; } = null!;
        public Guest Guest { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public int Nights => (CheckOut - CheckIn).Days;
    }
}
