namespace DAL.Entities
{
    public class Guest
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
        public List<Reservation> Reservations { get; set; } = []; 
    }
}
