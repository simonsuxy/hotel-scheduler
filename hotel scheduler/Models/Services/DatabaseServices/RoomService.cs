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
            Notes = room.Notes,
            HasReservations = room.Reservations.Count > 0
        };

        public async Task<List<RoomDTO>> GetRoomsAsync()
        {
            var rooms = await _db.Rooms
                .AsNoTracking()
                .Include(r => r.Reservations)
                .OrderBy(r => r.Name)
                .ToListAsync();
            return [.. rooms.Select(MapToDTO)];
        }

        public async Task<List<RoomDTO>> GetAllActiveRoomAsync()
        {
            var rooms = await _db.Rooms
                .AsNoTracking()
                .Include(r => r.Reservations)
                .Where(r => r.Status == RoomStatus.Active)
                .OrderBy(r => r.Name)
                .ToListAsync();
            return [.. rooms.Select(MapToDTO)];
        }

        public async Task<RoomDTO?> GetRoomByIdAsync(int id)
        {
            var room = await _db.Rooms
                .AsNoTracking()
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync(r => r.Id == id);

            return room == null ? null : MapToDTO(room);
        }

        public async Task<RoomDTO> CreateRoomAsync(string name, string description,
                                                    string type, int capacity, RoomStatus status,
                                                    decimal pricePerNight, string notes)
        {
            var trimmedName = name.Trim();
            if (await _db.Rooms.AnyAsync(r => r.Name.ToLower() == trimmedName.ToLower()))
            {
                throw new InvalidOperationException($"A room named '{trimmedName}' already exists.");
            }

            var room = new Room
            {
                Name = trimmedName,
                Description = description,
                Type = string.IsNullOrWhiteSpace(type) ? "Single" : type.Trim(),
                Status = status,
                Capacity = Math.Max(1, capacity),
                PricePerNight = pricePerNight,
                Notes = notes
            };
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return MapToDTO(room);
        }

        public async Task UpdateRoomAsync(int id, string name, string description,
                                            string type, int capacity,RoomStatus status,
                                            decimal pricePerNight, string notes)
        {
            var room = await _db.Rooms.FindAsync(id)
                ?? throw new InvalidOperationException("Room not found");
            var trimmedName = name.Trim();
            if (await _db.Rooms.AnyAsync(r => r.Name.ToLower() == trimmedName.ToLower() && r.Id != id))
                throw new InvalidOperationException($"A room named '{trimmedName}' already exists.");
            room.Name = trimmedName;
            room.Description = description;
            room.Type = string.IsNullOrWhiteSpace(type) ? "Single" : type.Trim();
            room.Capacity = Math.Max(1, capacity);
            room.PricePerNight = pricePerNight;
            room.Status = status;
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

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _db.Rooms
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new InvalidOperationException("Room not found.");

            if (room.Reservations.Count != 0)
                throw new InvalidOperationException("Cannot delete a room with existing reservations.");

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
        }
    }
}
