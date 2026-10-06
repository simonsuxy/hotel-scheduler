using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace hotel_scheduler.Models.Entities
{
    public class ReservationDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public decimal RoomPricePerNight { get; set; }
        public string GuestName { get; set; } = "";
        public int GuestId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestCount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string Notes { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int Nights => (CheckOut - CheckIn).Days;
        public decimal TotalPrice { get; set; }
        public string Color { get; set; } = "#4C9BE8";

        public List<ReservationChargeDTO> Charges { get; set; } = [];

        public decimal RoomBasePrice =>  RoomPricePerNight * Nights;

        public decimal TotalChargesPrice => Charges.Sum(c => c.CalculateTotal(Nights));

        public decimal ComputedTotalPrice => RoomBasePrice + TotalChargesPrice;
    }
}
