using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    .Where(p => p.Delete == false);

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
                    .Where(p => p.Delete == false && p.CategoryId == categoryId);

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
                    .Where(p => p.Delete == false && p.Name.ToLower().Contains(keyWord.ToLower()));
                
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

        public async Task<List<Product>> GetAllProducts()
        {
            try
            {
                var products = await _context.Products
                    .Where(x => x.Delete != true)
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
                var product = await _context.Products.FindAsync(id);
                if (product != null && product.Delete == true)
                    return null;
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
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Where(p => p.Delete == false && p.Status == 1)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
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
                .Where(p => topProductIds.Contains(p.Id))
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .ToListAsync();

            return products;
        }

        public async Task<Product> GetProductDetailAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)

                    .Include(p => p.ProductVariants.Where(v => v.Delete != true && v.IsActive == true))
                        .ThenInclude(v => v.Color)

                    .Include(p => p.ProductVariants.Where(v => v.Delete != true && v.IsActive == true))
                        .ThenInclude(v => v.Size)

                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product != null && product.Delete == true)
                    return null;

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
                .Where(p => p.Delete == false && p.Status == 1)

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
                .Where(p => p.Delete == false && p.Status == 1 && p.Id == id)

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
