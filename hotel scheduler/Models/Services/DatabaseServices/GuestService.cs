using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
    // Handles all database operations for Guests.
    public class GuestService(HotelSchedulerDbContext db)
    {
        private readonly HotelSchedulerDbContext _db = db;
        private static GuestDTO MapToDTO(Guest guest) => new()
        {
            Id = guest.Id,
            FirstName = guest.FirstName,
            LastName = guest.LastName,
            Email = guest.Email,
            Phone = guest.Phone,
            Address = guest.Address,
            Notes = guest.Notes,
            Reservations = guest.Reservations.Select(r => new ReservationDTO
            {
                Id = r.Id,
                RoomId = r.RoomId,
                GuestId = r.GuestId != 0 ? r.GuestId : guest.Id, 
                GuestCount = r.GuestCount,
                Color = r.Color,
                Notes = r.Notes,
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                PaymentStatus = r.PaymentStatus
            }).ToList()
        };
        
        public async Task<List<GuestDTO>> GetAllGuestsAsync()
        {
            var guests = await _db.Guests
                .Include(g => g.Reservations)
                .ThenInclude(r => r.Room)
                .OrderBy(g => g.LastName)
                .ThenBy(g => g.FirstName)
                .ToListAsync();
            return [.. guests.Select(MapToDTO)];
        }

        public async Task<GuestDTO?> GetGuestByIdAsync(int id)
        {
            var g = await _db.Guests
                .Include(g => g.Reservations)
                .ThenInclude(r => r.Room)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (g == null) return null;
            return MapToDTO(g);
        }
        // Searches guests by name, email, or phone
        public async Task<List<GuestDTO>> SearchAsync(string query)
        {
            var q = query.Trim().ToLower();

            var guests = await _db.Guests
                .Where(g => g.FirstName.ToLower().Contains(q)
                         || g.LastName.ToLower().Contains(q)
                         || (g.Email != null && g.Email.ToLower().Contains(q))
                         || (g.Phone != null && g.Phone.Contains(q)))
                .OrderBy(g => g.LastName)
                .ThenBy(g => g.FirstName)
                .ToListAsync();
            return [.. guests.Select(MapToDTO)];
        }

        public async Task<GuestDTO> CreateAsync(string firstName, string lastName,
                                                 string? email, string? phone,
                                                 string? idNumber, string? address,
                                                 string notes = "")
        {
            var guest = new Guest
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email?.Trim(),
                Phone = phone?.Trim(),
                IdNumber = idNumber?.Trim(),
                Address = address?.Trim(),
                Notes = notes.Trim()
            };

            _db.Guests.Add(guest);
            await _db.SaveChangesAsync();
            return MapToDTO(guest);
        }

        public async Task UpdateAsync(int id, string firstName, string lastName,
                                      string? email, string? phone,
                                      string? idNumber, string? address,
                                      string notes = "")
        {
            var guest = await _db.Guests.FindAsync(id)
                ?? throw new InvalidOperationException("Guest not found.");

            guest.FirstName = firstName.Trim();
            guest.LastName = lastName.Trim();
            guest.Email = email?.Trim();
            guest.Phone = phone?.Trim();
            guest.IdNumber = idNumber?.Trim();
            guest.Address = address?.Trim();
            guest.Notes = notes.Trim();
            await _db.SaveChangesAsync();
        }

        // Deletes a guest — only possible if they have no reservations attached
        // If they do, throws error InvalidOperationException
        public async Task DeleteAsync(int id)
        {
            var guest = await _db.Guests
                .Include(g => g.Reservations)
                .FirstOrDefaultAsync(g => g.Id == id)
                ?? throw new InvalidOperationException("Guest not found.");

            if (guest.Reservations.Count != 0)
                throw new InvalidOperationException(
                    "Cannot delete a guest with existing reservations. Remove the reservations first.");
            _db.Guests.Remove(guest);
            await _db.SaveChangesAsync();
        }
    }
}