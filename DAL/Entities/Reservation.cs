using System.Security.Cryptography;

namespace DAL.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int GuestId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestCount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string Notes { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public Room Room { get; set; } = null!;
        public Guest Guest { get; set; } = null!;
        public User? CreatedByUser { get; set; }
        public int? CreatedByUserId { get; set; }
        public int Nights => (CheckOut - CheckIn).Days;
        public string Color { get; set; } = "#4C9BE8";

        //Token for future API
        public string ManageToken { get; set; }
            = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
