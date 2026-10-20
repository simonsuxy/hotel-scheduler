using DAL.Entities;

namespace hotel_scheduler.Models.Entities
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAdmin => Role == UserRole.Admin;
    }
}
