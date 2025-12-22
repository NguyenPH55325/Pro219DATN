using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Models;

namespace Pro219.DAL.Context
{
    public class ClothesDbContext : DbContext
    {
        public ClothesDbContext()
        {
            
        }

        public ClothesDbContext(DbContextOptions<ClothesDbContext> options) : base(options)
        {
             
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<DiscountCode> DiscountCodes { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=ClothesStore;TrustServerCertificate=True;User Id=sa; Password=123456");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            
          
            // Customer relationships
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Addresses)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Carts)
                .WithOne(cart => cart.Customer)
                .HasForeignKey(cart => cart.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Reviews)
                .WithOne(r => r.Customer)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Address relationships
            modelBuilder.Entity<Address>()
                .HasMany(a => a.Orders)
                .WithOne(o => o.ShippingAddress)
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart relationships
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem relationships
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany(pv => pv.CartItems)
                .HasForeignKey(ci => ci.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // DiscountCode relationships
            modelBuilder.Entity<DiscountCode>()
                .HasMany(dc => dc.Orders)
                .WithOne(o => o.DiscountCode)
                .HasForeignKey(o => o.DiscountId)
                .OnDelete(DeleteBehavior.SetNull);

            // PaymentMethod relationships
            modelBuilder.Entity<PaymentMethod>()
                .HasMany(pm => pm.Orders)
                .WithOne(o => o.PaymentMethod)
                .HasForeignKey(o => o.PaymentMethodId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany(pv => pv.OrderItems)
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category relationships (self-referencing)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Brand relationships
            modelBuilder.Entity<Brand>()
                .HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sale relationships
            modelBuilder.Entity<Sale>()
                .HasMany(s => s.Products)
                .WithOne(p => p.Sale)
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Product relationships
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductImages)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductVariants)
                .WithOne(pv => pv.Product)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductImage relationships
            // Note: Changed to Restrict to avoid multiple cascade paths
            // ProductImage -> Product is Cascade, so images will be deleted when Product is deleted
            // ProductImage -> ProductVariant is Restrict to avoid cascade cycle
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.ProductVariant)
                .WithMany(pv => pv.ProductImages)
                .HasForeignKey(pi => pi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductVariant relationships
            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Color)
                .WithMany(c => c.ProductVariants)
                .HasForeignKey(pv => pv.ColorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Size)
                .WithMany(s => s.ProductVariants)
                .HasForeignKey(pv => pv.SizeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductVariant>()
                .HasMany(pv => pv.InventoryLogs)
                .WithOne(il => il.ProductVariant)
                .HasForeignKey(il => il.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wishlist relationships
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Customer)
                .WithMany(c => c.Wishlists)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.ProductVariant)
                .WithMany(pv => pv.Wishlists)
                .HasForeignKey(w => w.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better performance
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<DiscountCode>()
                .HasIndex(dc => dc.Code)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.SKU)
                .IsUnique();

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2024, 1, 1, 10, 0, 0);

            // Users
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, UserName = "admin", PasswordHash = "26dc318942685872cf79c5eb96c9bb13", Role = "Admin", CreateAt = seedDate, Status = 1 },
                new User { UserID = 2, UserName = "manager", PasswordHash = "26dc318942685872cf79c5eb96c9bb13", Role = "Manager", CreateAt = seedDate, Status =1 }
            );

            // Colors
            modelBuilder.Entity<Color>().HasData(
                new Color { Id = 1, Name = "Đỏ", HexCode = "#FF0000", Delete = false, CreateAt = seedDate, Status = 1 },
                new Color { Id = 2, Name = "Xanh dương", HexCode = "#0000FF", Delete = false, CreateAt = seedDate, Status = 1 },
                new Color { Id = 3, Name = "Đen", HexCode = "#000000", Delete = false, CreateAt = seedDate, Status = 1 },
                new Color { Id = 4, Name = "Trắng", HexCode = "#FFFFFF", Delete = false, CreateAt = seedDate, Status = 1 },
                new Color { Id = 5, Name = "Xanh lá", HexCode = "#00FF00", Delete = false, CreateAt = seedDate, Status = 1 },
                new Color { Id = 6, Name = "Xám", HexCode = "#808080", Delete = false, CreateAt = seedDate, Status = 1 },
                new Color { Id = 7, Name = "Hồng", HexCode = "#FFC0CB", Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // Sizes
            modelBuilder.Entity<Size>().HasData(
                new Size { Id = 1, Name = "S", Delete = false, CreateAt = seedDate, Status = 1 },
                new Size { Id = 2, Name = "M", Delete = false, CreateAt = seedDate, Status = 1 },
                new Size { Id = 3, Name = "L", Delete = false, CreateAt = seedDate, Status = 1 },
                new Size { Id = 4, Name = "XL", Delete = false, CreateAt = seedDate, Status = 1 },
                new Size { Id = 5, Name = "XXL", Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // Brands
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Nike", Description = "Thương hiệu thể thao hàng đầu thế giới", Status = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Brand { Id = 2, Name = "Adidas", Description = "Thương hiệu thời trang thể thao nổi tiếng", Status = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Brand { Id = 3, Name = "Puma", Description = "Thương hiệu thời trang thể thao đẳng cấp", Status = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Brand { Id = 4, Name = "Uniqlo", Description = "Thương hiệu thời trang Nhật Bản", Status = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate }
            );

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Nam", Description = "Thời trang nam", Status = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Category { Id = 2, Name = "Nữ", Description = "Thời trang nữ", Status = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Category { Id = 3, Name = "Áo thun", Description = "Áo thun nam nữ", Status = 1, ParentCategoryId = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Category { Id = 4, Name = "Quần jean", Description = "Quần jean nam nữ", Status = 1, ParentCategoryId = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Category { Id = 5, Name = "Áo sơ mi", Description = "Áo sơ mi công sở", Status = 1, ParentCategoryId = 1, UpdateBy = "admin", Delete = false, CreateAt = seedDate },
                new Category { Id = 6, Name = "Váy", Description = "Váy nữ", Status = 1, ParentCategoryId = 2, UpdateBy = "admin", Delete = false, CreateAt = seedDate }
            );

            // Sales
            modelBuilder.Entity<Sale>().HasData(
                new Sale { Id = 1, Name = "Khuyến mãi mùa hè", Description = "Giảm giá mùa hè cho tất cả sản phẩm", Type = "Percentage", SaleValue = 20, StartDate = seedDate, EndDate = seedDate.AddMonths(3), IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new Sale { Id = 2, Name = "Khuyến mãi mùa đông", Description = "Giảm giá mùa đông", Type = "Percentage", SaleValue = 15, StartDate = seedDate.AddMonths(6), EndDate = seedDate.AddMonths(9), IsActive = false, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new Sale { Id = 3, Name = "Khuyến mãi Black Friday", Description = "Siêu sale Black Friday", Type = "Percentage", SaleValue = 30, StartDate = seedDate.AddMonths(10), EndDate = seedDate.AddMonths(11), IsActive = false, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = -1, FullName = "Khách Vãng Lai", PhoneNumber = "0000000000", Email = "abc@example.com", DateOfBirth = new DateTime(1990, 5, 15), PasswordHash = "b855e41c5c5f5061ecba4fd8613a7760", CreateAt = seedDate, Status = 1, Delete = false },

                new Customer { Id = 1, FullName = "Nguyễn Văn An", PhoneNumber = "0912345678", Email = "nguyenvanan@example.com", DateOfBirth = new DateTime(1990, 5, 15), PasswordHash = "b855e41c5c5f5061ecba4fd8613a7760", CreateAt = seedDate, Status = 1, Delete = false },
                new Customer { Id = 2, FullName = "Trần Thị Bình", PhoneNumber = "0987654321", Email = "tranthibinh@example.com", DateOfBirth = new DateTime(1992, 8, 20), PasswordHash = "b855e41c5c5f5061ecba4fd8613a7760", CreateAt = seedDate, Status = 1, Delete = false },
                new Customer { Id = 3, FullName = "Lê Minh Cường", PhoneNumber = "0901234567", Email = "leminhcuong@example.com", DateOfBirth = new DateTime(1988, 3, 10), PasswordHash = "b855e41c5c5f5061ecba4fd8613a7760", CreateAt = seedDate, Status = 1, Delete = false }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, CategoryId = 3, BrandId = 1, SaleId = 1, Name = "Áo thun nam Nike cổ tròn", Description = "Áo thun nam chất liệu cotton mềm mại, thoáng mát, phù hợp mặc hàng ngày", BasePrice = 299000m, CreatedAt = seedDate, Status = 1, UpdateBy = "admin", Delete = false },
                new Product { Id = 2, CategoryId = 4, BrandId = 2, SaleId = null, Name = "Quần jean nam Adidas slim fit", Description = "Quần jean nam kiểu dáng slim fit, chất liệu denim cao cấp, co giãn tốt", BasePrice = 799000m, CreatedAt = seedDate, Status = 1, UpdateBy = "admin", Delete = false },
                new Product { Id = 3, CategoryId = 3, BrandId = 3, SaleId = null, Name = "Áo thun thể thao Puma", Description = "Áo thun thể thao thấm hút mồ hôi tốt, phù hợp tập luyện và vận động", BasePrice = 399000m, CreatedAt = seedDate, Status = 1, UpdateBy = "admin", Delete = false },
                new Product { Id = 4, CategoryId = 5, BrandId = 4, SaleId = null, Name = "Áo sơ mi nam Uniqlo", Description = "Áo sơ mi nam công sở, chất liệu cotton lụa, form dáng đẹp", BasePrice = 499000m, CreatedAt = seedDate, Status = 1, UpdateBy = "admin", Delete = false },
                new Product { Id = 5, CategoryId = 6, BrandId = 2, SaleId = 1, Name = "Váy nữ Adidas", Description = "Váy nữ thể thao, chất liệu thấm hút mồ hôi, thiết kế năng động", BasePrice = 599000m, CreatedAt = seedDate, Status = 1, UpdateBy = "admin", Delete = false }
            );

            // ProductVariants
            modelBuilder.Entity<ProductVariant>().HasData(
                new ProductVariant { Id = 1, ProductId = 1, ColorId = 1, SizeId = 2, SKU = "NKE-TSH-RED-M", StockQuantity = 50, Price = 299000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductVariant { Id = 2, ProductId = 1, ColorId = 2, SizeId = 3, SKU = "NKE-TSH-BLU-L", StockQuantity = 30, Price = 299000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductVariant { Id = 3, ProductId = 1, ColorId = 3, SizeId = 2, SKU = "NKE-TSH-BLK-M", StockQuantity = 40, Price = 299000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductVariant { Id = 4, ProductId = 2, ColorId = 3, SizeId = 2, SKU = "ADD-JNS-BLK-M", StockQuantity = 25, Price = 799000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductVariant { Id = 5, ProductId = 2, ColorId = 6, SizeId = 3, SKU = "ADD-JNS-GRY-L", StockQuantity = 20, Price = 799000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductVariant { Id = 6, ProductId = 3, ColorId = 4, SizeId = 1, SKU = "PMA-TSH-WHT-S", StockQuantity = 35, Price = 399000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductVariant { Id = 7, ProductId = 4, ColorId = 4, SizeId = 2, SKU = "UNQ-SHT-WHT-M", StockQuantity = 15, Price = 499000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductVariant { Id = 8, ProductId = 5, ColorId = 7, SizeId = 2, SKU = "ADD-DRS-PNK-M", StockQuantity = 18, Price = 599000m, IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // ProductImages
            modelBuilder.Entity<ProductImage>().HasData(
                new ProductImage { Id = 1, ProductId = 1, ProductVariantId = 1, ImageUrl = "/images/products/ao-thun-nike-do-m-1.jpg", IsMain = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductImage { Id = 2, ProductId = 1, ProductVariantId = 1, ImageUrl = "/images/products/ao-thun-nike-do-m-2.jpg", IsMain = false, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductImage { Id = 3, ProductId = 2, ProductVariantId = 4, ImageUrl = "/images/products/quan-jean-adidas-den-m-1.jpg", IsMain = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductImage { Id = 4, ProductId = 3, ProductVariantId = 6, ImageUrl = "/images/products/ao-thun-puma-trang-s-1.jpg", IsMain = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductImage { Id = 5, ProductId = 4, ProductVariantId = 7, ImageUrl = "/images/products/ao-so-mi-uniqlo-trang-m-1.jpg", IsMain = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new ProductImage { Id = 6, ProductId = 5, ProductVariantId = 8, ImageUrl = "/images/products/vay-nu-adidas-hong-m-1.jpg", IsMain = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // Addresses
            modelBuilder.Entity<Address>().HasData(
                new Address { Id = 1, CustomerId = 1, FullName = "Nguyễn Văn An", Phone = "0912345678", Street = "13007", City = "201", District = "3440", StreetName="Phương Canh", CityName="Hà Nội", DistrictName="Nam Từ Liêm", OtherInfo = "Chung cư ABC, căn hộ 4B", IsDefault = true, Status = 1, Delete = false, CreateAt = seedDate },
                new Address { Id = 2, CustomerId = 2, FullName = "Trần Thị Bình", Phone = "0987654321", Street = "13007", City = "201", District = "3440", StreetName="Phương Canh", CityName="Hà Nội", DistrictName="Nam Từ Liêm",  IsDefault = true, Status = 1, Delete = false, CreateAt = seedDate },
                new Address { Id = 3, CustomerId = 1, FullName = "Nguyễn Văn An", Phone = "0912345678", Street = "13007", City = "201", District = "3440", StreetName="Phương Canh", CityName="Hà Nội", DistrictName="Nam Từ Liêm",  OtherInfo = "Nhà riêng", IsDefault = false, Status = 1, Delete = false, CreateAt = seedDate },
                new Address { Id = 4, CustomerId = 3, FullName = "Lê Minh Cường", Phone = "0901234567", Street = "13007", City = "201", District = "3440", StreetName = "Phương Canh", CityName = "Hà Nội", DistrictName = "Nam Từ Liêm", IsDefault = true, Status = 1, Delete = false, CreateAt = seedDate }
            );

            // Carts
            modelBuilder.Entity<Cart>().HasData(
                new Cart { Id = 1, CustomerId = 1, Status = 1, UpdateAt = seedDate, Delete = false, CreateAt = seedDate },
                new Cart { Id = 2, CustomerId = 2, Status = 1, UpdateAt = seedDate, Delete = false, CreateAt = seedDate },
                new Cart { Id = 3, CustomerId = 3, Status = 1, UpdateAt = seedDate, Delete = false, CreateAt = seedDate }
            );

            // CartItems
            modelBuilder.Entity<CartItem>().HasData(
                new CartItem { Id = 1, CartId = 1, VariantId = 1, Quantity = 2, UnitPrice = 299000m, IsSelectedForCheckout = true, AddedAt = seedDate, Delete = false, CreateAt = seedDate, Status = 1 },
                new CartItem { Id = 2, CartId = 1, VariantId = 4, Quantity = 1, UnitPrice = 799000m, IsSelectedForCheckout = false, AddedAt = seedDate, Delete = false, CreateAt = seedDate, Status = 1 },
                new CartItem { Id = 3, CartId = 2, VariantId = 6, Quantity = 1, UnitPrice = 399000m, IsSelectedForCheckout = true, AddedAt = seedDate, Delete = false, CreateAt = seedDate, Status = 1 },
                new CartItem { Id = 4, CartId = 3, VariantId = 7, Quantity = 1, UnitPrice = 499000m, IsSelectedForCheckout = true, AddedAt = seedDate, Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // PaymentMethods
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { Id = 1, Name = "Thanh toán khi nhận hàng", Description = "Thanh toán bằng tiền mặt khi nhận hàng", IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new PaymentMethod { Id = 2, Name = "Chuyển khoản ngân hàng", Description = "Chuyển khoản qua ngân hàng", IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // DiscountCodes
            modelBuilder.Entity<DiscountCode>().HasData(
                new DiscountCode { DiscountId = 1, Code = "MUAHHE20", DiscountType = "Percentage", Value = 20, MinOrderValue = 500000, StartDate = seedDate, EndDate = seedDate.AddMonths(3), IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new DiscountCode { DiscountId = 2, Code = "CHAO10", DiscountType = "Percentage", Value = 10, MinOrderValue = 300000, StartDate = seedDate, EndDate = seedDate.AddMonths(6), IsActive = true, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new DiscountCode { DiscountId = 3, Code = "BLACKFRIDAY30", DiscountType = "Percentage", Value = 30, MinOrderValue = 1000000, StartDate = seedDate.AddMonths(10), EndDate = seedDate.AddMonths(11), IsActive = false, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 }
            );

            // Orders
            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, CustomerId = 1, ShippingAddressId = 1, DiscountId = 1, PaymentMethodId = 1, OrderCode = "DH001", OrderDate = seedDate, TotalAmount = 1397000m, DiscountAmount = 279400m, FinalAmount = 1117600m, PaymentStatus = "Chờ thanh toán", OrderStatus = "Đang xử lý", LastUpdate = seedDate, UpdateBy = "admin", Delete = false, CreateAt = seedDate, Status = 1 },
                new Order { OrderId = 2, CustomerId = 2, ShippingAddressId = 2, DiscountId = null, PaymentMethodId = 2, OrderCode = "DH002", OrderDate = seedDate.AddDays(1), TotalAmount = 399000m, DiscountAmount = 0, FinalAmount = 399000m, PaymentStatus = "Đã thanh toán", OrderStatus = "Đã giao hàng", LastUpdate = seedDate.AddDays(1), UpdateBy = "admin", Delete = false, CreateAt = seedDate.AddDays(1), Status = 1 },
                new Order { OrderId = 3, CustomerId = 3, ShippingAddressId = 4, DiscountId = 2, PaymentMethodId = 2, OrderCode = "DH003", OrderDate = seedDate.AddDays(2), TotalAmount = 499000m, DiscountAmount = 49900m, FinalAmount = 449100m, PaymentStatus = "Đã thanh toán", OrderStatus = "Đang vận chuyển", LastUpdate = seedDate.AddDays(2), UpdateBy = "admin", Delete = false, CreateAt = seedDate.AddDays(2), Status = 1 }
            );

            // OrderItems
            modelBuilder.Entity<OrderItem>().HasData(
                new OrderItem { OrderItemId = 1, OrderId = 1, ProductVariantId = 1, Quantity = 2, UnitPrice = 299000m, Subtotal = 598000m, Delete = false, CreateAt = seedDate, Status = 1 },
                new OrderItem { OrderItemId = 2, OrderId = 1, ProductVariantId = 4, Quantity = 1, UnitPrice = 799000m, Subtotal = 799000m, Delete = false, CreateAt = seedDate, Status = 1 },
                new OrderItem { OrderItemId = 3, OrderId = 2, ProductVariantId = 6, Quantity = 1, UnitPrice = 399000m, Subtotal = 399000m, Delete = false, CreateAt = seedDate.AddDays(1), Status = 1 },
                new OrderItem { OrderItemId = 4, OrderId = 3, ProductVariantId = 7, Quantity = 1, UnitPrice = 499000m, Subtotal = 499000m, Delete = false, CreateAt = seedDate.AddDays(2), Status = 1 }
            );

            // Reviews
            modelBuilder.Entity<Review>().HasData(
                new Review { Id = 1, OrderItemId=1, ProductId = 1, CustomerId = 1, Title = "Chất lượng tốt", Content = "Áo rất mềm mại và thoáng mát, chất lượng đúng như mô tả. Tôi rất hài lòng với sản phẩm này!", Overall = 5, CreatedAt = seedDate, Delete = false, Status = 1 },
                new Review { Id = 2, OrderItemId=1, ProductId = 2, CustomerId = 2, Title = "Vừa vặn hoàn hảo", Content = "Quần jean vừa vặn, chất liệu tốt, mặc rất đẹp. Sẽ mua thêm màu khác!", Overall = 5, CreatedAt = seedDate.AddDays(2), Delete = false, Status = 1 },
                new Review { Id = 3, OrderItemId=1, ProductId = 3, CustomerId = 1, Title = "Phù hợp tập thể thao", Content = "Áo thấm hút mồ hôi tốt, mặc tập gym rất thoải mái. Đáng giá tiền!", Overall = 4, CreatedAt = seedDate.AddDays(3), Delete = false, Status = 1 }
            );

            // InventoryLogs
            modelBuilder.Entity<InventoryLog>().HasData(
                new InventoryLog { InventoryLogId = 1, VariantId = 1, ChangeQuantity = 50, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 },
                new InventoryLog { InventoryLogId = 2, VariantId = 2, ChangeQuantity = 30, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 },
                new InventoryLog { InventoryLogId = 3, VariantId = 3, ChangeQuantity = 40, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 },
                new InventoryLog { InventoryLogId = 4, VariantId = 4, ChangeQuantity = 25, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 },
                new InventoryLog { InventoryLogId = 5, VariantId = 5, ChangeQuantity = 20, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 },
                new InventoryLog { InventoryLogId = 6, VariantId = 6, ChangeQuantity = 35, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 },
                new InventoryLog { InventoryLogId = 7, VariantId = 7, ChangeQuantity = 15, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 },
                new InventoryLog { InventoryLogId = 8, VariantId = 8, ChangeQuantity = 18, Reason = "Nhập kho ban đầu", CreateAt = seedDate, Delete = false, Status = 1 }
            );

            // Wishlists
            modelBuilder.Entity<Wishlist>().HasData(
                new Wishlist { Id = 1, CustomerId = 1, ProductVariantId = 2, Delete = false, CreateAt = seedDate, Status = 1 },
                new Wishlist { Id = 2, CustomerId = 1, ProductVariantId = 6, Delete = false, CreateAt = seedDate, Status = 1 },
                new Wishlist { Id = 3, CustomerId = 2, ProductVariantId = 1, Delete = false, CreateAt = seedDate, Status = 1 },
                new Wishlist { Id = 4, CustomerId = 3, ProductVariantId = 5, Delete = false, CreateAt = seedDate, Status = 1 },
                new Wishlist { Id = 5, CustomerId = 3, ProductVariantId = 8, Delete = false, CreateAt = seedDate, Status = 1 }
            );
        }
    }
}
