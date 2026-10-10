using DAL;
using DAL.Entities;
using hotel_scheduler.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace hotel_scheduler.Models.Services.DatabaseServices
{
    public class FinanceService(HotelSchedulerDbContext db)
    {
        private readonly HotelSchedulerDbContext _db = db;

        private static ChargeCategoryDTO MapToCategoryDTO(ChargeCategory category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DefaultPricePerNight = category.DefaultPricePerNight,
            Type = category.Type,
            IsPreCreated = category.IsPreCreated
        };

        private static ReservationChargeDTO MapToChargeDTO(ReservationCharge charge) => new()
        {
            Id = charge.Id,
            ReservationId = charge.ReservationId,
            CategoryId = charge.CategoryId,
            CategoryName = charge.Category?.Name ?? "",
            CategoryDescription = charge.Category?.Description ?? "",
            ChargeType = charge.Category?.Type ?? ChargeType.PerNight,
            Quantity = charge.Quantity,
            PricePerNight = charge.PricePerNight
        };
        public async Task<List<ChargeCategoryDTO>> GetCategoriesAsync()
        {
            var categories = await _db.ChargeCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return [.. categories.Select(MapToCategoryDTO)];
        }

        public async Task<ChargeCategoryDTO> CreateCategoryAsync(
            string name,
            decimal defaultPricePerNight,
            ChargeType type,
            string description = "")
        {
            var trimmedName = name.Trim();
            if (await _db.ChargeCategories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower()))
                throw new InvalidOperationException($"Category '{trimmedName}' already exists.");

            var category = new ChargeCategory
            {
                Name = trimmedName,
                Description = description.Trim(),
                DefaultPricePerNight = defaultPricePerNight,
                Type = type,
                IsPreCreated = false
            };

            _db.ChargeCategories.Add(category);
            await _db.SaveChangesAsync();

            return MapToCategoryDTO(category);
        }

        public async Task UpdateCategoryAsync(
            int id,
            string name,
            decimal defaultPricePerNight,
            ChargeType type,
            string description = "")
        {
            var category = await _db.ChargeCategories.FindAsync(id)
                ?? throw new InvalidOperationException("Category not found.");

            var trimmedName = name.Trim();
            if (await _db.ChargeCategories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower() && c.Id != id))
                throw new InvalidOperationException($"Category '{trimmedName}' already exists.");

            category.Name = trimmedName;
            category.DefaultPricePerNight = defaultPricePerNight;
            category.Type = type;
            category.Description = description.Trim();

            await _db.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _db.ChargeCategories.FindAsync(id)
                ?? throw new InvalidOperationException("Category not found.");

            if (category.IsPreCreated)
                throw new InvalidOperationException("Pre-created categories cannot be deleted.");

            bool isUsed = await _db.ReservationCharges.AnyAsync(rc => rc.CategoryId == id);
            if (isUsed)
                throw new InvalidOperationException("Cannot delete a category currently linked to reservation charges.");

            _db.ChargeCategories.Remove(category);
            await _db.SaveChangesAsync();
        }

        public async Task<List<ReservationChargeDTO>> GetChargesForReservationAsync(int reservationId)
        {
            var charges = await _db.ReservationCharges
                .Include(rc => rc.Category)
                .Where(rc => rc.ReservationId == reservationId)
                .ToListAsync();

            return [.. charges.Select(MapToChargeDTO)];
        }

        public async Task AddChargeToReservationAsync(int reservationId, int categoryId, int quantity, decimal? customPrice = null)
        {
            var category = await _db.ChargeCategories.FindAsync(categoryId)
                ?? throw new InvalidOperationException("Category not found.");

            var charge = new ReservationCharge
            {
                ReservationId = reservationId,
                CategoryId = categoryId,
                Quantity = Math.Max(1, quantity),
                PricePerNight = customPrice ?? category.DefaultPricePerNight
            };

            _db.ReservationCharges.Add(charge);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateReservationChargeAsync(int chargeId, int quantity, decimal pricePerNight)
        {
            var charge = await _db.ReservationCharges.FindAsync(chargeId)
                ?? throw new InvalidOperationException("Charge item not found.");

            charge.Quantity = Math.Max(1, quantity);
            charge.PricePerNight = pricePerNight;

            await _db.SaveChangesAsync();
        }

        public async Task RemoveChargeFromReservationAsync(int chargeId)
        {
            var charge = await _db.ReservationCharges.FindAsync(chargeId)
                ?? throw new InvalidOperationException("Charge item not found.");

            _db.ReservationCharges.Remove(charge);
            await _db.SaveChangesAsync();
        }
    }
}