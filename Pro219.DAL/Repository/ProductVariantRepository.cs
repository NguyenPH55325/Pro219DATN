using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class ProductVariantRepository
    {
        ClothesDbContext _context;

        public ProductVariantRepository()
        {
            _context = new ClothesDbContext();
        }

        public async Task<List<ProductVariant>> GetAllProductVariants()
        {
            try
            {
                var variants = await _context.ProductVariants
                    .Where(x => x.Delete != true)
                    .ToListAsync();
                return variants;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ProductVariant> GetProductVariantById(int id)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(id);
                if (variant != null && variant.Delete == true)
                    return null;
                return variant;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> GetProductVariantQuantityById(int id)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(id);
                if (variant == null || variant.Delete == true)
                    return 0;
                return variant.StockQuantity;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<List<ProductVariant>> GetProductVariantsByProductId(int productId)
        {
            try
            {
                var variants = await _context.ProductVariants
                    .Where(x => x.ProductId == productId && x.Delete != true)
                    .ToListAsync();
                return variants;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ProductVariant> AddProductVariant(ProductVariant variant)
        {
            try
            {
                variant.CreateAt = DateTime.Now;
                variant.Delete = false;
                var addedVariant = _context.ProductVariants.Add(variant).Entity;
                await _context.SaveChangesAsync();
                return addedVariant;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ProductVariant> UpdateProductVariant(ProductVariant variant)
        {
            try
            {
                var existingVariant = await _context.ProductVariants.FindAsync(variant.Id);

                if (existingVariant == null || existingVariant.Delete == true) return null;

                existingVariant.ProductId = variant.ProductId;
                existingVariant.ColorId = variant.ColorId;
                existingVariant.SizeId = variant.SizeId;
                existingVariant.SKU = variant.SKU;
                existingVariant.StockQuantity = variant.StockQuantity;
                existingVariant.Price = variant.Price;
                existingVariant.ArrivalTime = variant.ArrivalTime;
                existingVariant.IsActive = variant.IsActive;
                existingVariant.Status = variant.Status;
                existingVariant.UpdateBy = variant.UpdateBy;
                existingVariant.UpdateAt = DateTime.Now;

                var updatedVariant = _context.ProductVariants.Update(existingVariant).Entity;
                await _context.SaveChangesAsync();
                return updatedVariant;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ProductVariant> DeleteProductVariant(int id,string? updateBy = null)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(id);

                if (variant == null) return null;

                variant.Delete = true;
                variant.DeleteAt = DateTime.Now;
                variant.UpdateAt = DateTime.Now;
                if (updateBy != null)
                {
                    variant.UpdateBy = updateBy;
                }
               

                var updatedVariant = _context.ProductVariants.Update(variant).Entity;
                await _context.SaveChangesAsync();
                return updatedVariant;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IncreaseProductVariantQuantity(int id, int count)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(id);

                if (variant == null || variant.Delete == true) return false;

                variant.StockQuantity += count;
                variant.UpdateAt = DateTime.Now;

                _context.ProductVariants.Update(variant);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DecreaseProductVariantQuantity(int id, int count)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(id);

                if (variant == null || variant.Delete == true) return false;

                if (variant.StockQuantity - count < 0) return false;

                variant.StockQuantity -= count;
                variant.UpdateAt = DateTime.Now;

                _context.ProductVariants.Update(variant);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

