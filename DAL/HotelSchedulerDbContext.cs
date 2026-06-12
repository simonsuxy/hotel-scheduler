using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class HotelSchedulerDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Reservations> Reservations => Set<Reservations>();
        public DbSet<Settings> Settings => Set<Settings>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=data.sqlite");
    }
}
