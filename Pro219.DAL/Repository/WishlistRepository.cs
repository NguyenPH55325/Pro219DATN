using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class WishlistRepository
    {
        ClothesDbContext _context;

        public WishlistRepository()
        {
            _context = new ClothesDbContext();
        }

        public async Task<List<Wishlist>> GetAllWishlists()
        {
            try
            {
                var wishlists = await _context.Wishlists
                    .Where(x => x.Delete != true)
                    .ToListAsync();
                return wishlists;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Wishlist> GetWishlistById(int id)
        {
            try
            {
                var wishlist = await _context.Wishlists.FindAsync(id);
                if (wishlist != null && wishlist.Delete == true)
                    return null;
                return wishlist;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Wishlist>> GetWishlistsByCustomerId(int customerId)
        {
            try
            {
                var wishlists = await _context.Wishlists
                    .Where(x => x.CustomerId == customerId && x.Delete != true)
                    .ToListAsync();
                return wishlists;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Wishlist> GetWishlistByCustomerAndProduct(int customerId, int productId)
        {
            try
            {
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(x => x.CustomerId == customerId 
                        && x.ProductId == productId 
                        && x.Delete != true);
                return wishlist;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Wishlist> AddWishlist(Wishlist wishlist)
        {
            try
            {
                wishlist.CreateAt = DateTime.Now;
                wishlist.Delete = false;
                var addedWishlist = _context.Wishlists.Add(wishlist).Entity;
                await _context.SaveChangesAsync();
                return addedWishlist;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Wishlist> UpdateWishlist(Wishlist wishlist)
        {
            try
            {
                var existingWishlist = await _context.Wishlists.FindAsync(wishlist.Id);

                if (existingWishlist == null || existingWishlist.Delete == true) return null;

                existingWishlist.CustomerId = wishlist.CustomerId;
                existingWishlist.ProductVariantId = wishlist.ProductVariantId;
                existingWishlist.Status = wishlist.Status;
                existingWishlist.UpdateBy = wishlist.UpdateBy;
                existingWishlist.UpdateAt = DateTime.Now;

                var updatedWishlist = _context.Wishlists.Update(existingWishlist).Entity;
                await _context.SaveChangesAsync();
                return updatedWishlist;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Wishlist> DeleteWishlist(int id, string updateBy = null)
        {
            try
            {
                var wishlist = await _context.Wishlists.FindAsync(id);

                if (wishlist == null) return null;

                wishlist.Delete = true;
                wishlist.DeleteAt = DateTime.Now;
                wishlist.UpdateAt = DateTime.Now;
                if (!string.IsNullOrEmpty(updateBy))
                {
                    wishlist.UpdateBy = updateBy;
                }

                var updatedWishlist = _context.Wishlists.Update(wishlist).Entity;
                await _context.SaveChangesAsync();
                return updatedWishlist;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

