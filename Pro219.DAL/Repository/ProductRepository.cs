using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Pro219.DAL.Repository
{
    public class ProductRepository
    {
        private readonly ClothesDbContext _context;

        public ProductRepository()
        {
            _context = new ClothesDbContext();
        }

        public ProductRepository(ClothesDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDetailDto>> GetAllProducts(
            int? page = null,
            int? pageSize = null,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null,
            string? sortOrder = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Where(p => p.Delete == false
                        && p.Brand.Delete == false && p.Brand.Status == 1
                        && p.Category.Delete == false && p.Category.Status == 1);

                if (brandId.HasValue && brandId.Value > 0)
                {
                    query = query.Where(p => p.BrandId == brandId.Value);
                }

                if (colorId.HasValue && colorId.Value > 0)
                {
                    query = query.Where(p => p.ProductVariants.Any(pv => pv.Delete == false && pv.ColorId == colorId.Value));
                }

                if (sizeId.HasValue && sizeId.Value > 0)
                {
                    query = query.Where(p => p.ProductVariants.Any(pv => pv.Delete == false && pv.SizeId == sizeId.Value));
                }

                switch (sortOrder?.ToLower())
                {
                    case "name_asc":
                    case "az":
                        query = query.OrderBy(p => p.Name);
                        break;
                    case "name_desc":
                    case "za":
                        query = query.OrderByDescending(p => p.Name);
                        break;
                    case "price_asc":
                    case "price_increase":
                        query = query.OrderBy(p => p.BasePrice);
                        break;
                    case "price_desc":
                    case "price_decrease":
                        query = query.OrderByDescending(p => p.BasePrice);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                var productListQuery = query.Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    Description = p.Description,

                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    SaleName = p.Sale != null ? p.Sale.Name : null,
                    CreateAt = p.CreatedAt,


                    AvailableColors = p.ProductVariants
                            .Where(pv => pv.Delete == false)
                            .Select(pv => pv.Color)
                            .Distinct()
                            .Select(c => new ColorDto
                            {
                                Id = c.Id,
                                Name = c.Name,
                                HexCode = c.HexCode
                            })
                            .ToList(),

                    AvailableSizes = p.ProductVariants
                            .Where(pv => pv.Delete == false)
                            .Select(pv => pv.Size)
                            .Distinct()
                            .Select(s => new SizeDto
                            {
                                Id = s.Id,
                                Name = s.Name
                            })
                            .ToList(),
                    ImageUrls = p.ProductImages
                            .Where(pi => pi.Delete == false && pi.ProductVariantId == null)
                            .Select(pi => pi.ImageUrl)
                            .ToList(),

                    Variants = p.ProductVariants
                            .Where(pv => pv.Delete == false && (!colorId.HasValue || colorId.Value <= 0 || pv.ColorId == colorId.Value))
                            .Select(pv => new ProductVariantDto
                            {
                                Id = pv.Id,
                                SKU = pv.SKU,
                                Price = pv.Price,
                                StockQuantity = pv.StockQuantity,

                                ColorId = pv.Color.Id,
                                ColorName = pv.Color.Name,
                                ColorHexCode = pv.Color.HexCode,

                                SizeId = pv.Size.Id,
                                SizeName = pv.Size.Name,

                                VariantImageUrls = p.ProductImages
                                    .Where(pi => pi.Delete == false && pi.ProductVariantId == pv.Id)
                                    .Select(pi => pi.ImageUrl)
                                    .ToList()
                            }).ToList(),

                    ReviewCount = p.Reviews.Count(r => r.Status == 1),
                    AverageRating = p.Reviews.Any(r => r.Status == 1)
                                            ? (double?)p.Reviews.Where(r => r.Status == 1).Average(r => r.Overall)
                                            : null
                })
                    .AsQueryable();

                if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
                {
                    productListQuery = productListQuery
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
                }

                var productListDto = await productListQuery.ToListAsync();

                return productListDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ProductDetailDto>> GetAllProductsInCategory(
            int categoryId,
            int? page = null,
            int? pageSize = null,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null,
            string? sortOrder = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Where(p => p.Delete == false && p.CategoryId == categoryId
                        && p.Brand.Delete == false && p.Brand.Status == 1
                        && p.Category.Delete == false && p.Category.Status == 1);

                if (brandId.HasValue && brandId.Value > 0)
                {
                    query = query.Where(p => p.BrandId == brandId.Value);
                }

                if (colorId.HasValue && colorId.Value > 0)
                {
                    query = query.Where(p => p.ProductVariants.Any(pv => pv.Delete == false && pv.ColorId == colorId.Value));
                }

                if (sizeId.HasValue && sizeId.Value > 0)
                {
                    query = query.Where(p => p.ProductVariants.Any(pv => pv.Delete == false && pv.SizeId == sizeId.Value));
                }

                switch (sortOrder?.ToLower())
                {
                    case "name_asc":
                    case "az":
                        query = query.OrderBy(p => p.Name);
                        break;
                    case "name_desc":
                    case "za":
                        query = query.OrderByDescending(p => p.Name);
                        break;
                    case "price_asc":
                    case "price_increase":
                        query = query.OrderBy(p => p.BasePrice);
                        break;
                    case "price_desc":
                    case "price_decrease":
                        query = query.OrderByDescending(p => p.BasePrice);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                var productListQuery = query.Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    Description = p.Description,

                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    SaleName = p.Sale != null ? p.Sale.Name : null,
                    CreateAt = p.CreatedAt,


                    AvailableColors = p.ProductVariants
                            .Where(pv => pv.Delete == false)
                            .Select(pv => pv.Color)
                            .Distinct()
                            .Select(c => new ColorDto
                            {
                                Id = c.Id,
                                Name = c.Name,
                                HexCode = c.HexCode
                            })
                            .ToList(),

                    AvailableSizes = p.ProductVariants
                            .Where(pv => pv.Delete == false)
                            .Select(pv => pv.Size)
                            .Distinct()
                            .Select(s => new SizeDto
                            {
                                Id = s.Id,
                                Name = s.Name
                            })
                            .ToList(),
                    ImageUrls = p.ProductImages
                            .Where(pi => pi.Delete == false && pi.ProductVariantId == null)
                            .Select(pi => pi.ImageUrl)
                            .ToList(),

                    Variants = p.ProductVariants
                            .Where(pv => pv.Delete == false && (!colorId.HasValue || colorId.Value <= 0 || pv.ColorId == colorId.Value))
                            .Select(pv => new ProductVariantDto
                            {
                                Id = pv.Id,
                                SKU = pv.SKU,
                                Price = pv.Price,
                                StockQuantity = pv.StockQuantity,

                                ColorId = pv.Color.Id,
                                ColorName = pv.Color.Name,
                                ColorHexCode = pv.Color.HexCode,

                                SizeId = pv.Size.Id,
                                SizeName = pv.Size.Name,

                                VariantImageUrls = p.ProductImages
                                    .Where(pi => pi.Delete == false && pi.ProductVariantId == pv.Id)
                                    .Select(pi => pi.ImageUrl)
                                    .ToList()
                            }).ToList(),

                    ReviewCount = p.Reviews.Count(r => r.Status == 1),
                    AverageRating = p.Reviews.Any(r => r.Status == 1)
                                            ? (double?)p.Reviews.Where(r => r.Status == 1).Average(r => r.Overall)
                                            : null
                })
                    .AsQueryable();

                if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
                {
                    productListQuery = productListQuery
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
                }

                var productListDto = await productListQuery.ToListAsync();

                return productListDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ProductDetailDto>> GetAllProductByKeyWord(
            string keyWord,
            int? page = null,
            int? pageSize = null,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null,
            string? sortOrder = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Where(p => p.Delete == false && p.Name.ToLower().Contains(keyWord.ToLower())
                        && p.Brand.Delete == false && p.Brand.Status == 1
                        && p.Category.Delete == false && p.Category.Status == 1);

                if (brandId.HasValue && brandId.Value > 0)
                {
                    query = query.Where(p => p.BrandId == brandId.Value);
                }

                if (colorId.HasValue && colorId.Value > 0)
                {
                    query = query.Where(p => p.ProductVariants.Any(pv => pv.Delete == false && pv.ColorId == colorId.Value));
                }

                if (sizeId.HasValue && sizeId.Value > 0)
                {
                    query = query.Where(p => p.ProductVariants.Any(pv => pv.Delete == false && pv.SizeId == sizeId.Value));
                }

                switch (sortOrder?.ToLower())
                {
                    case "name_asc":
                    case "az":
                        query = query.OrderBy(p => p.Name);
                        break;
                    case "name_desc":
                    case "za":
                        query = query.OrderByDescending(p => p.Name);
                        break;
                    case "price_asc":
                    case "price_increase":
                        query = query.OrderBy(p => p.BasePrice);
                        break;
                    case "price_desc":
                    case "price_decrease":
                        query = query.OrderByDescending(p => p.BasePrice);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                var productListQuery = query.Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    Description = p.Description,

                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    SaleName = p.Sale != null ? p.Sale.Name : null,
                    CreateAt = p.CreatedAt,


                    AvailableColors = p.ProductVariants
                            .Where(pv => pv.Delete == false)
                            .Select(pv => pv.Color)
                            .Distinct()
                            .Select(c => new ColorDto
                            {
                                Id = c.Id,
                                Name = c.Name,
                                HexCode = c.HexCode
                            })
                            .ToList(),

                    AvailableSizes = p.ProductVariants
                            .Where(pv => pv.Delete == false)
                            .Select(pv => pv.Size)
                            .Distinct()
                            .Select(s => new SizeDto
                            {
                                Id = s.Id,
                                Name = s.Name
                            })
                            .ToList(),
                    ImageUrls = p.ProductImages
                            .Where(pi => pi.Delete == false && pi.ProductVariantId == null)
                            .Select(pi => pi.ImageUrl)
                            .ToList(),

                    Variants = p.ProductVariants
                            .Where(pv => pv.Delete == false && (!colorId.HasValue || colorId.Value <= 0 || pv.ColorId == colorId.Value))
                            .Select(pv => new ProductVariantDto
                            {
                                Id = pv.Id,
                                SKU = pv.SKU,
                                Price = pv.Price,
                                StockQuantity = pv.StockQuantity,

                                ColorId = pv.Color.Id,
                                ColorName = pv.Color.Name,
                                ColorHexCode = pv.Color.HexCode,

                                SizeId = pv.Size.Id,
                                SizeName = pv.Size.Name,

                                VariantImageUrls = p.ProductImages
                                    .Where(pi => pi.Delete == false && pi.ProductVariantId == pv.Id)
                                    .Select(pi => pi.ImageUrl)
                                    .ToList()
                            }).ToList(),

                    ReviewCount = p.Reviews.Count(r => r.Status == 1),
                    AverageRating = p.Reviews.Any(r => r.Status == 1)
                                            ? (double?)p.Reviews.Where(r => r.Status == 1).Average(r => r.Overall)
                                            : null
                })
                    .AsQueryable();

                if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
                {
                    productListQuery = productListQuery
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
                }

                var productListDto = await productListQuery.ToListAsync();
                return productListDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Product>> GetAllProducts(string? keyword = null, int? categoryId = null, int? brandId = null)
        {
            try
            {
                var query = _context.Products
                            .Include(p => p.Brand)
                            .Include(p => p.Category)
                            .Where(p => p.Delete == false
                                && p.Brand.Delete == false && p.Brand.Status == 1
                                && p.Category.Delete == false && p.Category.Status == 1);

                if(!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(keyword.ToLower()));
                }
                if(categoryId != null && categoryId > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }
                if (brandId != null && brandId > 0)
                {
                    query = query.Where(p => p.BrandId == brandId);
                }
                var products = await query.Select(p => new Product
                    {
                        Id = p.Id,
                        CategoryId = p.CategoryId,
                        BrandId = p.BrandId,
                        SaleId = p.SaleId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        CreatedAt = p.CreatedAt,
                        Status = p.Status,
                        Delete = p.Delete,
                        UpdateAt = p.UpdateAt,
                        DeleteAt = p.DeleteAt,
                        UpdateBy = p.UpdateBy,
                        Brand = p.Brand != null ? new Brand
                        {
                            Id = p.Brand.Id,
                            Name = p.Brand.Name,
                            Description = p.Brand.Description,
                            Status = p.Brand.Status,
                            Delete = p.Brand.Delete,
                            CreateAt = p.Brand.CreateAt,
                            UpdateAt = p.Brand.UpdateAt,
                            DeleteAt = p.Brand.DeleteAt,
                            UpdateBy = p.Brand.UpdateBy
                        } : null,
                        Category = p.Category != null ? new Category
                        {
                            Id = p.Category.Id,
                            ParentCategoryId = p.Category.ParentCategoryId,
                            Name = p.Category.Name,
                            Description = p.Category.Description,
                            Status = p.Category.Status,
                            Delete = p.Category.Delete,
                            CreateAt = p.Category.CreateAt,
                            UpdateAt = p.Category.UpdateAt,
                            DeleteAt = p.Category.DeleteAt,
                            UpdateBy = p.Category.UpdateBy
                        } : null
                    })
                    .ToListAsync();
                return products;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Product> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id
                        && p.Delete == false
                        && p.Brand.Delete == false && p.Brand.Status == 1
                        && p.Category.Delete == false && p.Category.Status == 1)
                    .Select(p => new Product
                    {
                        Id = p.Id,
                        CategoryId = p.CategoryId,
                        BrandId = p.BrandId,
                        SaleId = p.SaleId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        CreatedAt = p.CreatedAt,
                        Status = p.Status,
                        Delete = p.Delete,
                        UpdateAt = p.UpdateAt,
                        DeleteAt = p.DeleteAt,
                        UpdateBy = p.UpdateBy,
                        Brand = p.Brand != null ? new Brand
                        {
                            Id = p.Brand.Id,
                            Name = p.Brand.Name,
                            Description = p.Brand.Description,
                            Status = p.Brand.Status,
                            Delete = p.Brand.Delete,
                            CreateAt = p.Brand.CreateAt,
                            UpdateAt = p.Brand.UpdateAt,
                            DeleteAt = p.Brand.DeleteAt,
                            UpdateBy = p.Brand.UpdateBy
                        } : null,
                        Category = p.Category != null ? new Category
                        {
                            Id = p.Category.Id,
                            ParentCategoryId = p.Category.ParentCategoryId,
                            Name = p.Category.Name,
                            Description = p.Category.Description,
                            Status = p.Category.Status,
                            Delete = p.Category.Delete,
                            CreateAt = p.Category.CreateAt,
                            UpdateAt = p.Category.UpdateAt,
                            DeleteAt = p.Category.DeleteAt,
                            UpdateBy = p.Category.UpdateBy
                        } : null
                    })
                    .FirstOrDefaultAsync();
                return product;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Product> AddProduct(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.Now;
                product.Delete = false;
                var addedProduct = _context.Products.Add(product).Entity;
                await _context.SaveChangesAsync();
                return addedProduct;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(product.Id);

                if (existingProduct == null || existingProduct.Delete == true) return null;

                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.SaleId = product.SaleId;
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.BasePrice = product.BasePrice;
                existingProduct.Status = product.Status;
                existingProduct.UpdateBy = product.UpdateBy;
                existingProduct.UpdateAt = DateTime.Now;

                var updatedProduct = _context.Products.Update(existingProduct).Entity;
                await _context.SaveChangesAsync();
                return updatedProduct;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Product> DeleteProduct(int id, string? updateBy = null)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null) return null;
                var hasActiveOrder = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.ProductVariant)
                    .Where(oi => oi.ProductVariant.ProductId == id
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

                product.Delete = true;
                product.DeleteAt = DateTime.Now;
                product.UpdateAt = DateTime.Now;
                if (updateBy != null)
                {
                    product.UpdateBy = updateBy;
                }


                var updatedProduct = _context.Products.Update(product).Entity;
                await _context.SaveChangesAsync();
                return updatedProduct;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<List<Product>> GetTopProductAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.Delete == false && p.Status == 1
                    && p.Brand.Delete == false && p.Brand.Status == 1
                    && p.Category.Delete == false && p.Category.Status == 1)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(p => new Product
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    SaleId = p.SaleId,
                    Name = p.Name,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    CreatedAt = p.CreatedAt,
                    Status = p.Status,
                    Delete = p.Delete,
                    UpdateAt = p.UpdateAt,
                    DeleteAt = p.DeleteAt,
                    UpdateBy = p.UpdateBy,
                    Brand = p.Brand != null ? new Brand
                    {
                        Id = p.Brand.Id,
                        Name = p.Brand.Name,
                        Description = p.Brand.Description,
                        Status = p.Brand.Status,
                        Delete = p.Brand.Delete,
                        CreateAt = p.Brand.CreateAt,
                        UpdateAt = p.Brand.UpdateAt,
                        DeleteAt = p.Brand.DeleteAt,
                        UpdateBy = p.Brand.UpdateBy
                    } : null,
                    Category = p.Category != null ? new Category
                    {
                        Id = p.Category.Id,
                        ParentCategoryId = p.Category.ParentCategoryId,
                        Name = p.Category.Name,
                        Description = p.Category.Description,
                        Status = p.Category.Status,
                        Delete = p.Category.Delete,
                        CreateAt = p.Category.CreateAt,
                        UpdateAt = p.Category.UpdateAt,
                        DeleteAt = p.Category.DeleteAt,
                        UpdateBy = p.Category.UpdateBy
                    } : null,
                    ProductImages = p.ProductImages
                        .Where(pi => pi.Delete != true)
                        .Select(pi => new ProductImage
                        {
                            Id = pi.Id,
                            ProductId = pi.ProductId,
                            ProductVariantId = pi.ProductVariantId,
                            ImageUrl = pi.ImageUrl,
                            IsMain = pi.IsMain,
                            Delete = pi.Delete,
                            CreateAt = pi.CreateAt,
                            UpdateAt = pi.UpdateAt,
                            DeleteAt = pi.DeleteAt,
                            Status = pi.Status,
                            UpdateBy = pi.UpdateBy
                        }).ToList(),
                    ProductVariants = p.ProductVariants
                        .Where(pv => pv.Delete != true)
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
                            Color = pv.Color != null ? new DAL.Models.Color
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
                            } : null
                        }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<Product>> GetTopBestSellerAsync()
        {
            var topProductIds = await _context.OrderItems
                .Where(oi => oi.ProductVariant != null)
                .GroupBy(oi => oi.ProductVariant.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(8)
                .Select(x => x.ProductId)
                .ToListAsync();

            if (!topProductIds.Any())
                return new List<Product>();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => topProductIds.Contains(p.Id)
                    && p.Delete == false
                    && p.Brand.Delete == false && p.Brand.Status == 1
                    && p.Category.Delete == false && p.Category.Status == 1)
                .Select(p => new Product
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    SaleId = p.SaleId,
                    Name = p.Name,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    CreatedAt = p.CreatedAt,
                    Status = p.Status,
                    Delete = p.Delete,
                    UpdateAt = p.UpdateAt,
                    DeleteAt = p.DeleteAt,
                    UpdateBy = p.UpdateBy,
                    Brand = p.Brand != null ? new Brand
                    {
                        Id = p.Brand.Id,
                        Name = p.Brand.Name,
                        Description = p.Brand.Description,
                        Status = p.Brand.Status,
                        Delete = p.Brand.Delete,
                        CreateAt = p.Brand.CreateAt,
                        UpdateAt = p.Brand.UpdateAt,
                        DeleteAt = p.Brand.DeleteAt,
                        UpdateBy = p.Brand.UpdateBy
                    } : null,
                    Category = p.Category != null ? new Category
                    {
                        Id = p.Category.Id,
                        ParentCategoryId = p.Category.ParentCategoryId,
                        Name = p.Category.Name,
                        Description = p.Category.Description,
                        Status = p.Category.Status,
                        Delete = p.Category.Delete,
                        CreateAt = p.Category.CreateAt,
                        UpdateAt = p.Category.UpdateAt,
                        DeleteAt = p.Category.DeleteAt,
                        UpdateBy = p.Category.UpdateBy
                    } : null,
                    ProductImages = p.ProductImages
                        .Where(pi => pi.Delete != true)
                        .Select(pi => new ProductImage
                        {
                            Id = pi.Id,
                            ProductId = pi.ProductId,
                            ProductVariantId = pi.ProductVariantId,
                            ImageUrl = pi.ImageUrl,
                            IsMain = pi.IsMain,
                            Delete = pi.Delete,
                            CreateAt = pi.CreateAt,
                            UpdateAt = pi.UpdateAt,
                            DeleteAt = pi.DeleteAt,
                            Status = pi.Status,
                            UpdateBy = pi.UpdateBy
                        }).ToList(),
                    ProductVariants = p.ProductVariants
                        .Where(pv => pv.Delete != true)
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
                            Color = pv.Color != null ? new DAL.Models.Color
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
                            } : null
                        }).ToList()
                })
                .ToListAsync();

            return products;
        }

        public async Task<Product> GetProductDetailAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id
                        && p.Delete == false
                        && p.Brand.Delete == false && p.Brand.Status == 1
                        && p.Category.Delete == false && p.Category.Status == 1)
                    .Select(p => new Product
                    {
                        Id = p.Id,
                        CategoryId = p.CategoryId,
                        BrandId = p.BrandId,
                        SaleId = p.SaleId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        CreatedAt = p.CreatedAt,
                        Status = p.Status,
                        Delete = p.Delete,
                        UpdateAt = p.UpdateAt,
                        DeleteAt = p.DeleteAt,
                        UpdateBy = p.UpdateBy,
                        Brand = p.Brand != null ? new Brand
                        {
                            Id = p.Brand.Id,
                            Name = p.Brand.Name,
                            Description = p.Brand.Description,
                            Status = p.Brand.Status,
                            Delete = p.Brand.Delete,
                            CreateAt = p.Brand.CreateAt,
                            UpdateAt = p.Brand.UpdateAt,
                            DeleteAt = p.Brand.DeleteAt,
                            UpdateBy = p.Brand.UpdateBy
                        } : null,
                        Category = p.Category != null ? new Category
                        {
                            Id = p.Category.Id,
                            ParentCategoryId = p.Category.ParentCategoryId,
                            Name = p.Category.Name,
                            Description = p.Category.Description,
                            Status = p.Category.Status,
                            Delete = p.Category.Delete,
                            CreateAt = p.Category.CreateAt,
                            UpdateAt = p.Category.UpdateAt,
                            DeleteAt = p.Category.DeleteAt,
                            UpdateBy = p.Category.UpdateBy
                        } : null,
                        ProductImages = p.ProductImages
                            .Where(pi => pi.Delete != true)
                            .Select(pi => new ProductImage
                            {
                                Id = pi.Id,
                                ProductId = pi.ProductId,
                                ProductVariantId = pi.ProductVariantId,
                                ImageUrl = pi.ImageUrl,
                                IsMain = pi.IsMain,
                                Delete = pi.Delete,
                                CreateAt = pi.CreateAt,
                                UpdateAt = pi.UpdateAt,
                                DeleteAt = pi.DeleteAt,
                                Status = pi.Status,
                                UpdateBy = pi.UpdateBy
                            }).ToList(),
                        ProductVariants = p.ProductVariants
                            .Where(v => v.Delete != true && v.IsActive == true)
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
                                Color = pv.Color != null ? new DAL.Models.Color
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
                                Size = pv.Size != null ? new DAL.Models.Size
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
                            }).ToList()
                    })
                    .FirstOrDefaultAsync();

                return product;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ProductDetailDto>> GetAllAndDetailOptimized()
        {
            var productListDto = await _context.Products
                // 1. Lọc sản phẩm
                .Where(p => p.Delete == false && p.Status == 1
                    && p.Brand.Delete == false && p.Brand.Status == 1
                    && p.Category.Delete == false && p.Category.Status == 1)

                // 2. Sử dụng Select để ÁNH XẠ sang DTO
                .Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    Description = p.Description,

                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    SaleName = p.Sale != null ? p.Sale.Name : null,
                    CreateAt = p.CreatedAt,

                    // 🌟 TỔNG HỢP MÀU DUY NHẤT (trả về ColorDto) 🌟
                    AvailableColors = p.ProductVariants
                        .Where(pv => pv.Delete == false)
                        .Select(pv => pv.Color) // Chọn đối tượng Color
                        .Distinct() // Lọc đối tượng Color duy nhất (EF Core sẽ làm việc này dựa trên ID)
                        .Select(c => new ColorDto // Ánh xạ sang ColorDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            HexCode = c.HexCode
                        })
                        .ToList(),

                    // 🌟 TỔNG HỢP KÍCH THƯỚC DUY NHẤT (trả về SizeDto) 🌟
                    AvailableSizes = p.ProductVariants
                        .Where(pv => pv.Delete == false)
                        .Select(pv => pv.Size) // Chọn đối tượng Size
                        .Distinct() // Lọc đối tượng Size duy nhất
                        .Select(s => new SizeDto // Ánh xạ sang SizeDto
                        {
                            Id = s.Id,
                            Name = s.Name
                        })
                        .ToList(),

                    // Xử lý HÌNH ẢNH CHUNG
                    ImageUrls = p.ProductImages
                        .Where(pi => pi.Delete == false && pi.ProductVariantId == null)
                        .Select(pi => pi.ImageUrl)
                        .ToList(),

                    // Xử lý BIẾN THỂ (Cập nhật ánh xạ Color/Size chi tiết)
                    Variants = p.ProductVariants
                        .Where(pv => pv.Delete == false)
                        .Select(pv => new ProductVariantDto
                        {
                            Id = pv.Id,
                            SKU = pv.SKU,
                            Price = pv.Price,
                            StockQuantity = pv.StockQuantity,

                            // CẬP NHẬT THUỘC TÍNH MÀU SẮC
                            ColorId = pv.Color.Id,
                            ColorName = pv.Color.Name,
                            ColorHexCode = pv.Color.HexCode,

                            // CẬP NHẬT THUỘC TÍNH KÍCH THƯỚC
                            SizeId = pv.Size.Id,
                            SizeName = pv.Size.Name,

                            VariantImageUrls = p.ProductImages
                                .Where(pi => pi.Delete == false && pi.ProductVariantId == pv.Id)
                                .Select(pi => pi.ImageUrl)
                                .ToList()
                        }).ToList(),

                    // Thống kê đánh giá
                    ReviewCount = p.Reviews.Count(r => r.Status == 1),
                    AverageRating = p.Reviews.Any(r => r.Status == 1)
                                        ? (double?)p.Reviews.Where(r => r.Status == 1).Average(r => r.Overall)
                                        : null
                })
                .ToListAsync();

            return productListDto;
        }

        public async Task<ProductDetailDto> GetDetail(int id)
        {
            var productListDto = await _context.Products
                // 1. Lọc sản phẩm
                .Where(p => p.Delete == false && p.Status == 1 && p.Id == id
                    && p.Brand.Delete == false && p.Brand.Status == 1
                    && p.Category.Delete == false && p.Category.Status == 1)

                // 2. Sử dụng Select để ÁNH XẠ sang DTO
                .Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    Description = p.Description,

                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    SaleName = p.Sale != null ? p.Sale.Name : null,
                    CreateAt = p.CreatedAt,

                    // 🌟 TỔNG HỢP MÀU DUY NHẤT (trả về ColorDto) 🌟
                    AvailableColors = p.ProductVariants
                        .Where(pv => pv.Delete == false)
                        .Select(pv => pv.Color) // Chọn đối tượng Color
                        .Distinct() // Lọc đối tượng Color duy nhất (EF Core sẽ làm việc này dựa trên ID)
                        .Select(c => new ColorDto // Ánh xạ sang ColorDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            HexCode = c.HexCode
                        })
                        .ToList(),

                    // 🌟 TỔNG HỢP KÍCH THƯỚC DUY NHẤT (trả về SizeDto) 🌟
                    AvailableSizes = p.ProductVariants
                        .Where(pv => pv.Delete == false)
                        .Select(pv => pv.Size) // Chọn đối tượng Size
                        .Distinct() // Lọc đối tượng Size duy nhất
                        .Select(s => new SizeDto // Ánh xạ sang SizeDto
                        {
                            Id = s.Id,
                            Name = s.Name
                        })
                        .ToList(),

                    // Xử lý HÌNH ẢNH CHUNG
                    ImageUrls = p.ProductImages
                        .Where(pi => pi.Delete == false && pi.ProductVariantId == null)
                        .Select(pi => pi.ImageUrl)
                        .ToList(),

                    // Xử lý BIẾN THỂ (Cập nhật ánh xạ Color/Size chi tiết)
                    Variants = p.ProductVariants
                        .Where(pv => pv.Delete == false)
                        .Select(pv => new ProductVariantDto
                        {
                            Id = pv.Id,
                            SKU = pv.SKU,
                            Price = pv.Price,
                            StockQuantity = pv.StockQuantity,

                            // CẬP NHẬT THUỘC TÍNH MÀU SẮC
                            ColorId = pv.Color.Id,
                            ColorName = pv.Color.Name,
                            ColorHexCode = pv.Color.HexCode,

                            // CẬP NHẬT THUỘC TÍNH KÍCH THƯỚC
                            SizeId = pv.Size.Id,
                            SizeName = pv.Size.Name,

                            VariantImageUrls = p.ProductImages
                                .Where(pi => pi.Delete == false && pi.ProductVariantId == pv.Id)
                                .Select(pi => pi.ImageUrl)
                                .ToList()
                        }).ToList(),

                    // Thống kê đánh giá
                    ReviewCount = p.Reviews.Count(r => r.Status == 1),
                    AverageRating = p.Reviews.Any(r => r.Status == 1)
                                        ? (double?)p.Reviews.Where(r => r.Status == 1).Average(r => r.Overall)
                                        : null
                }).FirstOrDefaultAsync();

            return productListDto;
        }

        public async Task<List<SearchCombineProductDto>> SearchCombineProduct(
            string? keyword = null,
            int? page = null,
            int? pageSize = null,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null,
            string? sortOrder = null,
            bool? getDeleted = null)
        {
            try
            {
                var query = _context.ProductVariants
                    .AsNoTracking()
                    .Include(pv => pv.Product)
                        .ThenInclude(p => p.Brand)
                    .Include(pv => pv.Product)
                        .ThenInclude(p => p.Category)
                    .Include(pv => pv.Color)
                    .Include(pv => pv.Size)
                    .AsQueryable();

                
                if (getDeleted.HasValue)
                {
                    if (getDeleted.Value == false)
                    {
                        
                        query = query.Where(pv => pv.Delete != true
                            && pv.Product.Delete != true && pv.Product.Status == 1
                            && pv.Product.Brand.Delete != true && pv.Product.Brand.Status == 1
                            && pv.Product.Category.Delete != true && pv.Product.Category.Status == 1
                            && (pv.ColorId == null || (pv.Color.Delete != true && pv.Color.Status == 1))
                            && (pv.SizeId == null || (pv.Size.Delete != true && pv.Size.Status == 1)));
                    }
                   
                }

                if(keyword!=null && keyword.Trim().Length > 0)
                {
                    var loweredKeyword = keyword.Trim().ToLower();
                    query = query.Where(pv => pv.Product.Name.ToLower().Contains(loweredKeyword)) ;
                }
               
                if (brandId.HasValue && brandId.Value > 0)
                {
                    query = query.Where(pv => pv.Product.BrandId == brandId.Value);
                }

                if (colorId.HasValue && colorId.Value > 0)
                {
                    query = query.Where(pv => pv.ColorId == colorId.Value);
                }

                if (sizeId.HasValue && sizeId.Value > 0)
                {
                    query = query.Where(pv => pv.SizeId == sizeId.Value);
                }

                // Apply sorting
                switch (sortOrder?.ToLower())
                {
                    case "name_asc":
                    case "az":
                        query = query.OrderBy(pv => pv.Product.Name);
                        break;
                    case "name_desc":
                    case "za":
                        query = query.OrderByDescending(pv => pv.Product.Name);
                        break;
                    case "price_asc":
                    case "price_increase":
                        query = query.OrderBy(pv => pv.Price);
                        break;
                    case "price_desc":
                    case "price_decrease":
                        query = query.OrderByDescending(pv => pv.Price);
                        break;
                    default:
                        query = query.OrderByDescending(pv => pv.Product.CreatedAt);
                        break;
                }

                // Select to DTO
                var resultQuery = query.Select(pv => new SearchCombineProductDto
                {
                    ProductId = pv.ProductId,
                    ProductVariantId = pv.Id,
                    ProductName = pv.Product.Name,
                    ColorId = pv.ColorId,
                    ColorName = pv.Color != null ? pv.Color.Name : null,
                    SizeId = pv.SizeId,
                    SizeName = pv.Size != null ? pv.Size.Name : null,
                    BrandId = pv.Product.BrandId,
                    BrandName = pv.Product.Brand.Name,
                    Quantity = pv.StockQuantity,
                    UnitPrice = pv.Price
                })
                .AsQueryable();

                // Apply pagination
                if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
                {
                    resultQuery = resultQuery
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
                }

                var result = await resultQuery.ToListAsync();
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class SearchCombineProductDto
        {
            public int ProductId { get; set; }
            public int ProductVariantId { get; set; }
            public string ProductName { get; set; }
            public int? ColorId { get; set; }
            public string? ColorName { get; set; }
            public int? SizeId { get; set; }
            public string? SizeName { get; set; }
            public int BrandId { get; set; }
            public string BrandName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        public class ProductDetailDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal BasePrice { get; set; }
            public string Description { get; set; }
            public string CategoryName { get; set; }
            public string BrandName { get; set; }
            public string SaleName { get; set; }
            public List<ProductVariantDto> Variants { get; set; }
            public List<string> ImageUrls { get; set; }
            public int ReviewCount { get; set; }
            public double? AverageRating { get; set; }
            public List<ColorDto> AvailableColors { get; set; }
            public List<SizeDto> AvailableSizes { get; set; }
            public DateTime CreateAt { get; set; }
        }
        public class ProductVariantDto
        {
            public int Id { get; set; }
            public string SKU { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string ColorName { get; set; }
            public string SizeName { get; set; }
            public List<string> VariantImageUrls { get; set; }
            public int ColorId { get; set; }
            public string ColorHexCode { get; set; }
            public int SizeId { get; set; }
        }

        public class ColorDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string HexCode { get; set; }
        }

        public class SizeDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
