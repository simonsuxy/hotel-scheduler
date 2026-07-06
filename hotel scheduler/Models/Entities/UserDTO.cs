using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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
