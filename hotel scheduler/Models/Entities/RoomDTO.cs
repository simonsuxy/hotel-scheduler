using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace hotel_scheduler.Models.Entities
{
    public class RoomDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public RoomType Type { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Active;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string Notes { get; set; } = "";
    }
}
