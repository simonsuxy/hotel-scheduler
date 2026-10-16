using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "Single";
        public RoomStatus Status { get; set; } = RoomStatus.Active;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string Notes { get; set; } = "";
        public List<Reservation> Reservations { get; set; } = [];
    } 
}
