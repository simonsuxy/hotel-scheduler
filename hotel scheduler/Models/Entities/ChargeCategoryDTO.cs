using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace hotel_scheduler.Models.Entities
{
    public class ChargeCategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal DefaultPricePerNight { get; set; }
        public ChargeType Type { get; set; }
        public bool IsPreCreated { get; set; }
    }
}
