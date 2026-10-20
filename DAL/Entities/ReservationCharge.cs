namespace DAL.Entities
{
    public class ReservationCharge
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal PricePerNight { get; set; }
        public Reservation Reservation { get; set; } = null!;
        public ChargeCategory Category { get; set; } = null!;
    }
}
