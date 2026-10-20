using DAL.Entities;

namespace hotel_scheduler.Models.Entities
{
    public class RoomDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "Single";
        public RoomStatus Status { get; set; } = RoomStatus.Active;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string Notes { get; set; } = "";
        public bool HasReservations { get; set; }
    }
}
