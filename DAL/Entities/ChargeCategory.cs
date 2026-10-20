namespace DAL.Entities
{
    public class ChargeCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal DefaultPricePerNight { get; set; }
        public ChargeType Type { get; set; } = ChargeType.PerNight;
        public bool IsPreCreated { get; set; } = false;
        public List<ReservationCharge> ReservationCharges { get; set; } = [];
    }
}
