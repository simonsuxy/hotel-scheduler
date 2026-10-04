using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
    public class ReservationService
    {
        private readonly HotelSchedulerDbContext _db;

        public ReservationService(HotelSchedulerDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        private static ReservationDTO MapToDTO(Reservation reservation) => new()
        {
            Id = reservation.Id,
            RoomId = reservation.RoomId,
            RoomName = reservation.Room?.Name,
            GuestName = reservation.Guest?.FullName,
            GuestId = reservation.GuestId,
            CheckIn = reservation.CheckIn,
            CheckOut = reservation.CheckOut,
            GuestCount = reservation.GuestCount,
            PaymentStatus = reservation.PaymentStatus,
            Notes = reservation.Notes,
            CreatedAt = reservation.CreatedAt,
            TotalPrice = 0,
            Color = reservation.Color
        };

        public async Task<List<ReservationDTO>> GetReservationsAsync()
        {
            var reservations = await _db.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .OrderBy(r => r.CheckIn)
                .ToListAsync();

            return reservations.Select(MapToDTO).ToList();
        }

        public async Task<List<ReservationDTO>> GetReservationsByDateAsync(DateTime startDate, DateTime endDate)
        {
            var overlappingReservations = await _db.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .Where(r => r.CheckIn < endDate && r.CheckOut > startDate)
                .OrderBy(r => r.CheckIn)
                .ToListAsync();

            return overlappingReservations.Select(MapToDTO).ToList();
        }

        private async Task<bool> IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
        {
            return !await _db.Reservations
                .Where(r => r.RoomId == roomId
                     && r.CheckIn < checkOut
                     && r.CheckOut > checkIn
                     // Exclude the current reservation being edited
                     && (excludeReservationId == null || r.Id != excludeReservationId))
                .AnyAsync();
        }

        public async Task CreateReservationAsync(int roomId, int guestId, DateTime checkIn, DateTime checkOut,
                                                   int guestCount, string notes, string color, int? createdById)
        {
            await ValidateNoOverlapAsync(roomId, checkIn, checkOut);

            var reservation = new Reservation
            {
                RoomId = roomId,
                GuestId = guestId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                GuestCount = guestCount,
                Notes = notes,
                Color = color,
                CreatedByUserId = createdById,
                PaymentStatus = PaymentStatus.Unpaid
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
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
            reservation.GuestCount = guestCount;
            reservation.Notes = notes;
            reservation.Color = color;

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
