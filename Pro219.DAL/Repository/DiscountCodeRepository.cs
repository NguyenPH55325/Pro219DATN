using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class DiscountCodeRepository
    {
        ClothesDbContext _context;

        public DiscountCodeRepository()
        {
            _context = new ClothesDbContext();
        }

        public async Task<List<DiscountCode>> GetAllDiscountCodes(string? code = null, string? discountType = null, byte? type = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.DiscountCodes
                    .Where(x => x.Delete != true)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(code))
                {
                    query = query.Where(x => x.Code.Contains(code));
                }

                if (!string.IsNullOrWhiteSpace(discountType))
                {
                    query = query.Where(x => x.DiscountType.Contains(discountType));
                }

                if (type.HasValue)
                {
                    query = query.Where(x => x.Type == type);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(x => x.StartDate.Date >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(x => x.EndDate.Date <= endDate.Value.Date);
                }

                var discountCodes = await query.ToListAsync();
                return discountCodes;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //public async Task<decimal> ApplyDiscountCodeValue(string code, int userId, decimal totalAmount){

        //    try{

        //        if(userId == null) return 0;
        //        var discountCode = await GetDiscountCodeByCode(code);
        //        int userTimeUsed = await _context.Orders.Where(x => x.CustomerId == userId && x.DiscountId == discountCode.DiscountId).CountAsync();
        //        if(discountCode.IsReusable == true && userTimeUsed >= discountCode.MaxUsage) return 0;
        //        if(discountCode == null) return 0;
        //        if(discountCode.MinOrderValue != null && totalAmount < discountCode.MinOrderValue) return 0;
        //        if(discountCode.MaxDiscountAmount != null && discountCode.Value > discountCode.MaxDiscountAmount) return 0;
        //        if(discountCode.MaxUsage != null && discountCode.UsageCount >= discountCode.MaxUsage) return 0;
        //        if(discountCode.IsActive == false) return 0;
        //        if(discountCode.StartDate > DateTime.Now) return 0;
        //        if(discountCode.EndDate < DateTime.Now) return 0;
        //        if(discountCode.IsReusable == false && userTimeUsed >= 1) return 0;
        //        if(discountCode.Status == 1) // Percent
        //        {
        //            return totalAmount * (discountCode.Value / 100);
        //        }
        //        else if(discountCode.Status == 2) // Fixed Amount
        //        {
        //            return totalAmount - discountCode.Value;
        //        }
        //        return 0;   
        //    }
        //    catch (Exception)
        //    {
        //        return 0;
        //    }
        //}


        public async Task<int> GetUserTimeUsed(string code, int userId)
        {
            try
            {
                var discountCode = await GetDiscountCodeByCode(code);
                return await _context.Orders.Where(x => x.CustomerId == userId && x.DiscountId == discountCode.DiscountId && x.Status!=3).CountAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<DiscountCode> GetDiscountCodeById(int id)
        {
            try
            {
                var discountCode = await _context.DiscountCodes.FindAsync(id);
                if (discountCode != null && discountCode.Delete == false)
                    return discountCode;
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

               
        public async Task<DiscountCode> GetDiscountCodeByCode(string code)
        {
            try
            {
                var discountCode = await _context.DiscountCodes
                    .FirstOrDefaultAsync(x => x.Code == code && x.Delete != true);
                return discountCode;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DiscountCode> AddDiscountCode(DiscountCode discountCode)
        {
            try
            {
                discountCode.CreateAt = DateTime.Now;
                discountCode.Delete = false;
                var addedDiscountCode = _context.DiscountCodes.Add(discountCode).Entity;
                await _context.SaveChangesAsync();
                return addedDiscountCode;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DiscountCode> UpdateDiscountCode(DiscountCode discountCode)
        {
            try
            {
                var existingDiscountCode = await _context.DiscountCodes.FindAsync(discountCode.DiscountId);

                if (existingDiscountCode == null || existingDiscountCode.Delete == true) return null;

                existingDiscountCode.Code = discountCode.Code;
                existingDiscountCode.DiscountType = discountCode.DiscountType;
                existingDiscountCode.Value = discountCode.Value;
                existingDiscountCode.MinOrderValue = discountCode.MinOrderValue;
                existingDiscountCode.StartDate = discountCode.StartDate;
                existingDiscountCode.EndDate = discountCode.EndDate;
                existingDiscountCode.IsReusable = discountCode.IsReusable;
                existingDiscountCode.IsActive = discountCode.IsActive;
                existingDiscountCode.Status = discountCode.Status;
                existingDiscountCode.UpdateBy = discountCode.UpdateBy;
                existingDiscountCode.UpdateBy = discountCode.UpdateBy;
                existingDiscountCode.UpdateAt = DateTime.Now;
                existingDiscountCode.Type = discountCode.Type;

                var updatedDiscountCode = _context.DiscountCodes.Update(existingDiscountCode).Entity;
                await _context.SaveChangesAsync();
                return updatedDiscountCode;
            }
            catch (Exception)
            {
                return null;
            }
        }

        

        public async Task<DiscountCode> DeleteDiscountCode(int id, string? updateBy = null)
        {
            try
            {
                var discountCode = await _context.DiscountCodes.FindAsync(id);

                if (discountCode == null) return null;

                discountCode.Delete = true;
                discountCode.DeleteAt = DateTime.Now;
                discountCode.UpdateAt = DateTime.Now;
                if (updateBy!=null)
                {
                    discountCode.UpdateBy = updateBy;
                }
              

                var updatedDiscountCode = _context.DiscountCodes.Update(discountCode).Entity;
                await _context.SaveChangesAsync();
                return updatedDiscountCode;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

