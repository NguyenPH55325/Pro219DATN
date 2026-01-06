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

        public async Task<List<ProductVariant>> GetAllProductVariants(string role = null)
        {
            try
            {

                var query = _context.ProductVariants.AsNoTracking().AsQueryable();

                query = query.Where(x => x.Delete != true);

                if (role == "Customer" || role ==null)
                {
                    query = query.Where(x => x.Product.Status == 1 && x.Product.Delete != true
                                        && x.Product.Brand.Status == 1 && x.Product.Brand.Delete != true
                                        && x.Product.Category.Status == 1 && x.Product.Category.Delete != true
                                        && (x.Color.Delete != true && x.Color.Status == 1)
                                        && (x.Size.Delete != true && x.Size.Status == 1) && x.IsActive==true);
                }
                else if(role == "Admin" || role == "Manager" || role == "Staff")
                {
                    query = query.Where(x => (x.Color.Delete != true)
                                        && ( x.Size.Delete != true) && (x.Product.Delete !=true) && (x.Product.Brand.Delete!=true) && (x.Product.Category.Delete!=true));
                }



                var variants = await query
                       .Select(pv => new ProductVariant {
                       Id = pv.Id,
                       ProductId = pv.ProductId,
                       ColorId = pv.ColorId,
                       SizeId = pv.SizeId,
                       SKU = pv.SKU,
                       StockQuantity = pv.StockQuantity,
                       Price = pv.Price,
                       ArrivalTime = pv.ArrivalTime,
                       IsActive = pv.IsActive,
                       Delete = pv.Delete,
                       CreateAt = pv.CreateAt,
                       UpdateAt = pv.UpdateAt,
                       DeleteAt = pv.DeleteAt,
                       Status = pv.Status,
                       UpdateBy = pv.UpdateBy,
                       Product = pv.Product != null ? new Product
                       {
                           Id = pv.Product.Id,
                           CategoryId = pv.Product.CategoryId,
                           BrandId = pv.Product.BrandId,
                           SaleId = pv.Product.SaleId,
                           Name = pv.Product.Name,
                           Description = pv.Product.Description,
                           BasePrice = pv.Product.BasePrice,
                           CreatedAt = pv.Product.CreatedAt,
                           Status = pv.Product.Status,
                           Delete = pv.Product.Delete,
                           UpdateAt = pv.Product.UpdateAt,
                           DeleteAt = pv.Product.DeleteAt,
                           UpdateBy = pv.Product.UpdateBy,
                           Brand = pv.Product.Brand != null ? new Brand
                           {
                               Id = pv.Product.Brand.Id,
                               Name = pv.Product.Brand.Name,
                               Description = pv.Product.Brand.Description,
                               Status = pv.Product.Brand.Status,
                               Delete = pv.Product.Brand.Delete,
                               CreateAt = pv.Product.Brand.CreateAt,
                               UpdateAt = pv.Product.Brand.UpdateAt,
                               DeleteAt = pv.Product.Brand.DeleteAt,
                               UpdateBy = pv.Product.Brand.UpdateBy
                           } : null,
                           Category = pv.Product.Category != null ? new Category
                           {
                               Id = pv.Product.Category.Id,
                               ParentCategoryId = pv.Product.Category.ParentCategoryId,
                               Name = pv.Product.Category.Name,
                               Description = pv.Product.Category.Description,
                               Status = pv.Product.Category.Status,
                               Delete = pv.Product.Category.Delete,
                               CreateAt = pv.Product.Category.CreateAt,
                               UpdateAt = pv.Product.Category.UpdateAt,
                               DeleteAt = pv.Product.Category.DeleteAt,
                               UpdateBy = pv.Product.Category.UpdateBy
                           } : null
                       } : null,
                       Color = pv.Color != null ? new Color
                       {
                           Id = pv.Color.Id,
                           Name = pv.Color.Name,
                           HexCode = pv.Color.HexCode,
                           Delete = pv.Color.Delete,
                           CreateAt = pv.Color.CreateAt,
                           UpdateAt = pv.Color.UpdateAt,
                           DeleteAt = pv.Color.DeleteAt,
                           Status = pv.Color.Status,
                           UpdateBy = pv.Color.UpdateBy
                       } : null,
                       Size = pv.Size != null ? new Size
                       {
                           Id = pv.Size.Id,
                           Name = pv.Size.Name,
                           Delete = pv.Size.Delete,
                           CreateAt = pv.Size.CreateAt,
                           UpdateAt = pv.Size.UpdateAt,
                           DeleteAt = pv.Size.DeleteAt,
                           Status = pv.Size.Status,
                           UpdateBy = pv.Size.UpdateBy
                       } : null
                   })
                   .ToListAsync();
                return variants;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ProductVariant>> GetAllProductVariantsRaw()
        {
            try
            {
                var variants = await _context.ProductVariants
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
                var variant = await _context.ProductVariants
                    .AsNoTracking()
                    .Where(x => x.Id == id
                        && x.Delete != true
                        && x.Product.Delete != true && x.Product.Status == 1
                        && x.Product.Brand.Delete != true && x.Product.Brand.Status == 1
                        && x.Product.Category.Delete != true && x.Product.Category.Status == 1
                        && (x.ColorId == null || (x.Color.Delete != true && x.Color.Status == 1))
                        && (x.SizeId == null || (x.Size.Delete != true && x.Size.Status == 1)))
                    .Select(pv => new ProductVariant
                    {
                        Id = pv.Id,
                        ProductId = pv.ProductId,
                        ColorId = pv.ColorId,
                        SizeId = pv.SizeId,
                        SKU = pv.SKU,
                        StockQuantity = pv.StockQuantity,
                        Price = pv.Price,
                        ArrivalTime = pv.ArrivalTime,
                        IsActive = pv.IsActive,
                        Delete = pv.Delete,
                        CreateAt = pv.CreateAt,
                        UpdateAt = pv.UpdateAt,
                        DeleteAt = pv.DeleteAt,
                        Status = pv.Status,
                        UpdateBy = pv.UpdateBy,
                        Product = pv.Product != null ? new Product
                        {
                            Id = pv.Product.Id,
                            CategoryId = pv.Product.CategoryId,
                            BrandId = pv.Product.BrandId,
                            SaleId = pv.Product.SaleId,
                            Name = pv.Product.Name,
                            Description = pv.Product.Description,
                            BasePrice = pv.Product.BasePrice,
                            CreatedAt = pv.Product.CreatedAt,
                            Status = pv.Product.Status,
                            Delete = pv.Product.Delete,
                            UpdateAt = pv.Product.UpdateAt,
                            DeleteAt = pv.Product.DeleteAt,
                            UpdateBy = pv.Product.UpdateBy,
                            Brand = pv.Product.Brand != null ? new Brand
                            {
                                Id = pv.Product.Brand.Id,
                                Name = pv.Product.Brand.Name,
                                Description = pv.Product.Brand.Description,
                                Status = pv.Product.Brand.Status,
                                Delete = pv.Product.Brand.Delete,
                                CreateAt = pv.Product.Brand.CreateAt,
                                UpdateAt = pv.Product.Brand.UpdateAt,
                                DeleteAt = pv.Product.Brand.DeleteAt,
                                UpdateBy = pv.Product.Brand.UpdateBy
                            } : null,
                            Category = pv.Product.Category != null ? new Category
                            {
                                Id = pv.Product.Category.Id,
                                ParentCategoryId = pv.Product.Category.ParentCategoryId,
                                Name = pv.Product.Category.Name,
                                Description = pv.Product.Category.Description,
                                Status = pv.Product.Category.Status,
                                Delete = pv.Product.Category.Delete,
                                CreateAt = pv.Product.Category.CreateAt,
                                UpdateAt = pv.Product.Category.UpdateAt,
                                DeleteAt = pv.Product.Category.DeleteAt,
                                UpdateBy = pv.Product.Category.UpdateBy
                            } : null
                        } : null,
                        Color = pv.Color != null ? new Color
                        {
                            Id = pv.Color.Id,
                            Name = pv.Color.Name,
                            HexCode = pv.Color.HexCode,
                            Delete = pv.Color.Delete,
                            CreateAt = pv.Color.CreateAt,
                            UpdateAt = pv.Color.UpdateAt,
                            DeleteAt = pv.Color.DeleteAt,
                            Status = pv.Color.Status,
                            UpdateBy = pv.Color.UpdateBy
                        } : null,
                        Size = pv.Size != null ? new Size
                        {
                            Id = pv.Size.Id,
                            Name = pv.Size.Name,
                            Delete = pv.Size.Delete,
                            CreateAt = pv.Size.CreateAt,
                            UpdateAt = pv.Size.UpdateAt,
                            DeleteAt = pv.Size.DeleteAt,
                            Status = pv.Size.Status,
                            UpdateBy = pv.Size.UpdateBy
                        } : null
                    })
                    .FirstOrDefaultAsync();
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
                var variant = await _context.ProductVariants
                    .Include(pv => pv.Product)
                        .ThenInclude(p => p.Brand)
                    .Include(pv => pv.Product)
                        .ThenInclude(p => p.Category)
                    .Include(pv => pv.Color)
                    .Include(pv => pv.Size)
                    .Where(x => x.Id == id
                        && x.Delete != true
                        && x.Product.Delete != true && x.Product.Status == 1
                        && x.Product.Brand.Delete != true && x.Product.Brand.Status == 1
                        && x.Product.Category.Delete != true && x.Product.Category.Status == 1
                        && (x.ColorId == null || (x.Color.Delete != true && x.Color.Status == 1))
                        && (x.SizeId == null || (x.Size.Delete != true && x.Size.Status == 1)))
                    .Select(x => x.StockQuantity)
                    .FirstOrDefaultAsync();
                return variant;
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
                    .AsNoTracking()
                    .Where(x => x.ProductId == productId
                        && x.Delete != true
                        && x.Product.Delete != true
                        && x.Product.Brand.Delete != true && x.Product.Brand.Status == 1
                        && x.Product.Category.Delete != true && x.Product.Category.Status == 1
                        && (x.ColorId == null || (x.Color.Delete != true && x.Color.Status == 1))
                        && (x.SizeId == null || (x.Size.Delete != true && x.Size.Status == 1)))
                    .Select(pv => new ProductVariant
                    {
                        Id = pv.Id,
                        ProductId = pv.ProductId,
                        ColorId = pv.ColorId,
                        SizeId = pv.SizeId,
                        SKU = pv.SKU,
                        StockQuantity = pv.StockQuantity,
                        Price = pv.Price,
                        ArrivalTime = pv.ArrivalTime,
                        IsActive = pv.IsActive,
                        Delete = pv.Delete,
                        CreateAt = pv.CreateAt,
                        UpdateAt = pv.UpdateAt,
                        DeleteAt = pv.DeleteAt,
                        Status = pv.Status,
                        UpdateBy = pv.UpdateBy,
                        Product = pv.Product != null ? new Product
                        {
                            Id = pv.Product.Id,
                            CategoryId = pv.Product.CategoryId,
                            BrandId = pv.Product.BrandId,
                            SaleId = pv.Product.SaleId,
                            Name = pv.Product.Name,
                            Description = pv.Product.Description,
                            BasePrice = pv.Product.BasePrice,
                            CreatedAt = pv.Product.CreatedAt,
                            Status = pv.Product.Status,
                            Delete = pv.Product.Delete,
                            UpdateAt = pv.Product.UpdateAt,
                            DeleteAt = pv.Product.DeleteAt,
                            UpdateBy = pv.Product.UpdateBy,
                            Brand = pv.Product.Brand != null ? new Brand
                            {
                                Id = pv.Product.Brand.Id,
                                Name = pv.Product.Brand.Name,
                                Description = pv.Product.Brand.Description,
                                Status = pv.Product.Brand.Status,
                                Delete = pv.Product.Brand.Delete,
                                CreateAt = pv.Product.Brand.CreateAt,
                                UpdateAt = pv.Product.Brand.UpdateAt,
                                DeleteAt = pv.Product.Brand.DeleteAt,
                                UpdateBy = pv.Product.Brand.UpdateBy
                            } : null,
                            Category = pv.Product.Category != null ? new Category
                            {
                                Id = pv.Product.Category.Id,
                                ParentCategoryId = pv.Product.Category.ParentCategoryId,
                                Name = pv.Product.Category.Name,
                                Description = pv.Product.Category.Description,
                                Status = pv.Product.Category.Status,
                                Delete = pv.Product.Category.Delete,
                                CreateAt = pv.Product.Category.CreateAt,
                                UpdateAt = pv.Product.Category.UpdateAt,
                                DeleteAt = pv.Product.Category.DeleteAt,
                                UpdateBy = pv.Product.Category.UpdateBy
                            } : null
                        } : null,
                        Color = pv.Color != null ? new Color
                        {
                            Id = pv.Color.Id,
                            Name = pv.Color.Name,
                            HexCode = pv.Color.HexCode,
                            Delete = pv.Color.Delete,
                            CreateAt = pv.Color.CreateAt,
                            UpdateAt = pv.Color.UpdateAt,
                            DeleteAt = pv.Color.DeleteAt,
                            Status = pv.Color.Status,
                            UpdateBy = pv.Color.UpdateBy
                        } : null,
                        Size = pv.Size != null ? new Size
                        {
                            Id = pv.Size.Id,
                            Name = pv.Size.Name,
                            Delete = pv.Size.Delete,
                            CreateAt = pv.Size.CreateAt,
                            UpdateAt = pv.Size.UpdateAt,
                            DeleteAt = pv.Size.DeleteAt,
                            Status = pv.Size.Status,
                            UpdateBy = pv.Size.UpdateBy
                        } : null
                    })
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

        public async Task<ProductVariant> DeleteProductVariant(int id, string? updateBy = null)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(id);

                if (variant == null) return null;

                var hasActiveOrder = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.ProductVariant)
                    .Where(oi => oi.ProductVariant.Id == id
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

