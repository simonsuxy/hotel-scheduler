using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace hotel_scheduler.Models.Services.DatabaseServices
{
    public class RoomService(HotelSchedulerDbContext db)
    {
        private readonly HotelSchedulerDbContext _db = db;

        private static RoomDTO MapToDTO(Room room) => new()
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            Type = room.Type,
            Status = room.Status,
            PricePerNight = room.PricePerNight,
            Capacity = room.Capacity,
            Notes = room.Notes
        };

        public async Task<List<RoomDTO>> GetRoomsAsync()
        {
            var rooms = await _db.Rooms
                .OrderBy(r => r.Name)
                .ToListAsync();
            return [.. rooms.Select(MapToDTO)];
        }

        public async Task<List<RoomDTO>> GetAllActiveRoomAsync()
        {
            var rooms = await _db.Rooms
                .Where(r => r.Status == RoomStatus.Active)
                .OrderBy(r => r.Name)
                .ToListAsync();
            return [.. rooms.Select(MapToDTO)];
        }

        public async Task<RoomDTO?> GetRoomByIdAsync(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return null;
            return MapToDTO(room);
        }

        public async Task<RoomDTO> CreateRoomAsync(string name, string description,
                                                    RoomType type, int capacity,
                                                    decimal pricePerNight, string notes)
        {
            if (await _db.Rooms.AnyAsync(r => r.Name.ToLower() == name.Trim().ToLower()))
                throw new InvalidOperationException($"A room named {name} already exists");
            var room = new Room
            {
                Name = name,
                Description = description,
                Type = type,
                Status = RoomStatus.Active,
                Capacity = capacity,
                PricePerNight = pricePerNight,
                Notes = notes
            };
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return MapToDTO(room);
        }

        public async Task UpdateRoomAsync(int id, string name, string description,
                                            RoomType type, int capacity,
                                            decimal pricePerNight, string notes)
        {
            var room = await _db.Rooms.FindAsync(id)
                ?? throw new InvalidOperationException("Room not found");
            if (await _db.Rooms.AnyAsync(r => r.Name.ToLower() == name.Trim().ToLower()
                                           && r.Id != id))
                throw new InvalidOperationException($"A room named '{name}' already exists.");
            room.Name = name;
            room.Description = description;
            room.Type = type;
            room.Capacity = capacity;
            room.PricePerNight = pricePerNight;
            room.Notes = notes;
            await _db.SaveChangesAsync();
        }

        public async Task SetRoomStatusAsync(int id, RoomStatus status)
        {
            var room = await _db.Rooms.FindAsync(id)
                ?? throw new InvalidOperationException("Room not found");
            room.Status = status;
            await _db.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var room = await _db.Rooms
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new InvalidOperationException("Room not found.");

            // Warn if room has future reservations still attached
            var hasFutureReservations = room.Reservations
                .Any(r => r.CheckOut > DateTime.UtcNow);

            if (hasFutureReservations)
                throw new InvalidOperationException(
                    "Cannot deactivate a room with future reservations. Reassign or cancel them first.");
            room.Status = RoomStatus.Inactive;
            await _db.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            var room = await _db.Rooms.FindAsync(id)
                ?? throw new InvalidOperationException("Room not found.");

            room.Status = RoomStatus.Active;
            await _db.SaveChangesAsync();
        }

    }
}
