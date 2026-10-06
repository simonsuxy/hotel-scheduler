using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class HotelSchedulerDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Settings> Settings => Set<Settings>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=data.sqlite");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reservation>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Reservations)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.ManageToken)
                .IsUnique();

            modelBuilder.Entity<Room>()
                .Property(r => r.PricePerNight)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Settings>()
                .HasData(new Settings { Id = 1 });

            modelBuilder.Entity<ReservationCharge>()
                .HasOne(rc => rc.Reservation)
                .WithMany(r => r.Charges)
                .HasForeignKey(rc => rc.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReservationCharge>()
                .HasOne(rc => rc.Category)
                .WithMany(c => c.ReservationCharges)
                .HasForeignKey(rc => rc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChargeCategory>()
                .Property(c => c.DefaultPricePerNight)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ReservationCharge>()
                .Property(rc => rc.PricePerNight)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ChargeCategory>().HasData(
                new ChargeCategory { Id = 1, Name = "Adults", Description = "Charge per adult guest", DefaultPricePerNight = 20m, Type = ChargeType.PerNight, IsPreCreated = true },
                new ChargeCategory { Id = 2, Name = "Kids", Description = "Charge per child guest", DefaultPricePerNight = 10m, Type = ChargeType.PerNight, IsPreCreated = true },
                new ChargeCategory { Id = 3, Name = "Pets", Description = "Charge per pet", DefaultPricePerNight = 15m, Type = ChargeType.PerNight, IsPreCreated = true }
            );
        } 
    }
}
