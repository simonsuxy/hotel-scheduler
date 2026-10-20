using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
    public class ReservationService(HotelSchedulerDbContext db)
    {
        private readonly HotelSchedulerDbContext _db = db;
        private static ReservationDTO MapToDTO(Reservation reservation)
        {
            int nights = Math.Max(1, reservation.Nights);
            decimal roomPrice = reservation.Room?.PricePerNight ?? 0m;

            var chargeDTOs = reservation.Charges?.Select(c => new ReservationChargeDTO
            {
                Id = c.Id,
                ReservationId = c.ReservationId,
                CategoryId = c.CategoryId,
                CategoryName = c.Category?.Name ?? "",
                ChargeType = c.Category?.Type ?? ChargeType.PerNight,
                Quantity = c.Quantity,
                PricePerNight = c.PricePerNight
            }).ToList() ?? [];

            return new ReservationDTO
            {
                Id = reservation.Id,
                RoomId = reservation.RoomId,
                RoomName = reservation.Room?.Name ?? "",
                RoomPricePerNight = roomPrice,
                GuestName = reservation.Guest?.FullName ?? "",
                GuestId = reservation.GuestId,
                CheckIn = reservation.CheckIn,
                CheckOut = reservation.CheckOut,
                GuestCount = reservation.GuestCount,
                PaymentStatus = reservation.PaymentStatus,
                Notes = reservation.Notes,
                CreatedAt = reservation.CreatedAt,
                Color = reservation.Color,
                Charges = chargeDTOs,
                TotalPrice = (roomPrice * nights) + chargeDTOs.Sum(c => c.CalculateTotal(nights))
            };
        }

        public async Task<List<ReservationDTO>> GetReservationsAsync()
        {
            var reservations = await _db.Reservations
                .AsNoTracking()
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .Include(r => r.Charges)
                    .ThenInclude(c => c.Category)
                .OrderBy(r => r.CheckIn)
                .ToListAsync();

            return [.. reservations.Select(MapToDTO)];
        }

        public async Task<List<ReservationDTO>> GetReservationsByDateAsync(DateTime startDate, DateTime endDate)
        {
            var overlappingReservations = await _db.Reservations
                .AsNoTracking()
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .Include(r => r.Charges)
                    .ThenInclude(c => c.Category)
                .Where(r => r.CheckIn < endDate && r.CheckOut > startDate)
                .OrderBy(r => r.CheckIn)
                .ToListAsync();

            return [.. overlappingReservations.Select(MapToDTO)];
        }

        public async Task<int> CreateReservationAsync(int roomId, int guestId, DateTime checkIn, DateTime checkOut,
                                                   int guestCount, string notes, string color, int? createdById)
        {
            await ValidateNoOverlapAsync(roomId, checkIn, checkOut);

            var reservation = new Reservation
            {
                RoomId = roomId,
                GuestId = guestId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                GuestCount = Math.Max(1, guestCount),
                Notes = notes.Trim(),
                Color = string.IsNullOrWhiteSpace(color) ? "#4C9BE8" : color,
                CreatedByUserId = createdById,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            return reservation.Id;
        }

        public async Task UpdateAsync(int reservationId, int roomId, int guestId, DateTime checkIn, DateTime checkOut,
                                       int guestCount, string notes, string color)
        {
            var reservation = await _db.Reservations.FindAsync(reservationId)
                ?? throw new InvalidOperationException("Reservation not found.");

            await ValidateNoOverlapAsync(roomId, checkIn, checkOut, excludeReservationId: reservationId);

            reservation.RoomId = roomId;
            reservation.GuestId = guestId;
            reservation.CheckIn = checkIn;
            reservation.CheckOut = checkOut;
            reservation.GuestCount = Math.Max(1, guestCount);
            reservation.Notes = notes;
            reservation.Color = string.IsNullOrWhiteSpace(color) ? "#4C9BE8" : color;
            await _db.SaveChangesAsync();
        }

        private async Task ValidateNoOverlapAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
        {
            bool hasOverlap = await _db.Reservations.AnyAsync(r =>
                r.RoomId == roomId &&
                (!excludeReservationId.HasValue || r.Id != excludeReservationId.Value) &&
                checkIn < r.CheckOut &&
                checkOut > r.CheckIn);

            if (hasOverlap)
            {
                throw new InvalidOperationException("This room is already reserved for the selected date and time.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id)
                ?? throw new InvalidOperationException("Reservation not found");
            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync();
        }

        public async Task UpdatePaymenetStatus(int id, PaymentStatus paymentStatus)
        {
            var reservation = await _db.Reservations.FindAsync(id)
                ?? throw new InvalidOperationException("Reservation not found.");
            reservation.PaymentStatus = paymentStatus;
            await _db.SaveChangesAsync();
        }
    }
}
