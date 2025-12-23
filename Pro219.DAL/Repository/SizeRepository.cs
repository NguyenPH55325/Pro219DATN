using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class SizeRepository
    {
        ClothesDbContext _context;

        public SizeRepository()
        {
            _context = new ClothesDbContext();
        }

        public async Task<List<Size>> GetAllSizes(string keyword)
        {
            try
            {
                if (keyword == null)
                {

                    var sizes = await _context.Sizes
                        .Where(x => x.Delete != true)
                        .ToListAsync();
                    return sizes;
                }
                else
                {
                    var sizes = await _context.Sizes
                        .Where(x => x.Delete != true && x.Name.Contains(keyword))
                        .ToListAsync();
                    return sizes;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Size> GetSizeById(int id)
        {
            try
            {
                var size = await _context.Sizes.FindAsync(id);
                if (size != null && size.Delete == true)
                    return null;
                return size;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Size> AddSize(Size size)
        {
            try
            {
                size.CreateAt = DateTime.Now;
                size.Delete = false;
                var addedSize = _context.Sizes.Add(size).Entity;
                await _context.SaveChangesAsync();
                return addedSize;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Size> UpdateSize(Size size)
        {
            try
            {
                var existingSize = await _context.Sizes.FindAsync(size.Id);

                if (existingSize == null || existingSize.Delete == true) return null;

                existingSize.Name = size.Name;
                existingSize.Status = size.Status;
                existingSize.UpdateBy = size.UpdateBy;
                existingSize.UpdateAt = DateTime.Now;

                var updatedSize = _context.Sizes.Update(existingSize).Entity;
                await _context.SaveChangesAsync();
                return updatedSize;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Size> DeleteSize(int id, string updateBy = null)
        {
            try
            {
                var size = await _context.Sizes.FindAsync(id);

                if (size == null) return null;

                size.Delete = true;
                size.DeleteAt = DateTime.Now;
                size.UpdateAt = DateTime.Now;
                if (!string.IsNullOrEmpty(updateBy))
                {
                    size.UpdateBy = updateBy;
                }

                var updatedSize = _context.Sizes.Update(size).Entity;
                await _context.SaveChangesAsync();
                return updatedSize;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

