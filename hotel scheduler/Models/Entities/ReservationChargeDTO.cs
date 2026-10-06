using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace hotel_scheduler.Models.Entities
{
    public class ReservationChargeDTO
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public ChargeType ChargeType { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerNight { get; set; }

        public decimal CalculateTotal(int nights)
        {
            return ChargeType == ChargeType.Flat
                ? Quantity * PricePerNight
                : Quantity * PricePerNight * (nights > 0 ? nights : 1);
        }
    }
}
