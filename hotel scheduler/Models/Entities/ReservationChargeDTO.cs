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
        public string CategoryDescription { get; set; } = "";
        public ChargeType ChargeType { get; set; }
        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set => _quantity = value < 1 ? 1 : value;
        }
        public decimal PricePerNight { get; set; }
        public decimal CalculateTotal(int nights)
        {
            return ChargeType == ChargeType.Flat
                ? Quantity * PricePerNight
                : Quantity * PricePerNight * (nights > 0 ? nights : 1);
        }
    }
}
