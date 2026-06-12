using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.Staff;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
