using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class BrandRepository
    {
        ClothesDbContext _context;

        public BrandRepository()
        {
            _context = new ClothesDbContext();
        }

        public async Task<List<Brand>> GetAllBrands(string keyword)
        {
            try
            {
                if (keyword == null)
                {
                    var brands = await _context.Brands
                        .Where(x => x.Delete != true)
                        .ToListAsync();
                    return brands;
                }
                else
                {
                    var brands = await _context.Brands
                        .Where(x => x.Delete != true && x.Name.Contains(keyword))
                        .ToListAsync();
                    return brands;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Brand> GetBrandById(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand != null && brand.Delete == true)
                    return null;
                return brand;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Brand> AddBrand(Brand brand)
        {
            try
            {
                brand.CreateAt = DateTime.Now;
                brand.Delete = false;
                var sameNameBrand = await _context.Brands
                    .Where(b => b.Name == brand.Name && b.Delete != true)
                    .FirstOrDefaultAsync();
                if (sameNameBrand != null)
                {
                    return null;
                }    
                var addedBrand = _context.Brands.Add(brand).Entity;
                await _context.SaveChangesAsync();
                return addedBrand;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Brand> UpdateBrand(Brand brand)
        {
            try
            {
                var existingBrand = await _context.Brands.FindAsync(brand.Id);

                var sameNameBrand = await _context.Brands
                    .Where(b => b.Name == brand.Name && b.Id != brand.Id && b.Delete != true)
                    .FirstOrDefaultAsync();
                if (existingBrand == null || existingBrand.Delete == true || sameNameBrand!=null) return null;

                existingBrand.Name = brand.Name;
                existingBrand.Description = brand.Description;
                existingBrand.Status = brand.Status;
                existingBrand.UpdateBy = brand.UpdateBy;
                existingBrand.UpdateAt = DateTime.Now;

                var updatedBrand = _context.Brands.Update(existingBrand).Entity;
                await _context.SaveChangesAsync();
                return updatedBrand;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Brand> DeleteBrand(int id, string? updateBy = null)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);

                if (brand == null) return null;

                var hasActiveOrder = await _context.OrderItems
                 .Include(oi => oi.Order)
                 .Include(oi => oi.ProductVariant)
                 .Where(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null && oi.ProductVariant.Product.BrandId == id
                     && oi.Order.Status.HasValue
                     && (oi.Order.Status.Value == 0
                         || oi.Order.Status.Value == 1
                         || oi.Order.Status.Value == 2
                         || oi.Order.Status.Value == 4
                         || oi.Order.Status.Value == 5))
                 .AnyAsync();

                if (hasActiveOrder)
                {
                    return null;
                }

                brand.Delete = true;
                brand.DeleteAt = DateTime.Now;
                brand.UpdateAt = DateTime.Now;
                if (updateBy != null)
                {
                    brand.UpdateBy = updateBy;
                }
               
                var updatedBrand = _context.Brands.Update(brand).Entity;
                await _context.SaveChangesAsync();
                return updatedBrand;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

