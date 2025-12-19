namespace Pro219.Web.Constants
{
    public static class RouterConst
    {
        public const string AccessDenied = "/access-denined";
        public const string NotFound = "/not-found";
        public static class User
        {
            // Home
            public const string Home = "/";

            // Auth
            public const string SignUp = "/sign-up";
            public const string Login = "/login";
            public const string ForgotPassword = "/forgot-password";
            public const string ChangePassword = "/change-password";

            // Profile
            public const string Profile = "/profile";

            // Cart
            public const string CartDetail = "/cart/detail";

            // Product
            public const string ProductDetail = "/product/detail";

            // Checkout
            public const string CheckoutDetail = "/checkout/customer";
            public const string GusestCheckoutDetail = "/checkout/guest";

            // Search
            public const string SearchByCategoryId = "/products/category/:categoryId";
            public const string SearchByKeyword = "/products/search";
            public const string SearchAll = "/products/all";

            // Order
            public const string Orders = "/orders";
            public const string OrderDetail = "/orders/:id/detail";
            public const string PaymentSuccess = "/order/payment-success";
            public const string PaymentCancelled = "/order/payment-cancelled";
        }

        public static class Admin
        {
            // Root
            public const string Root = "/admin/";

            // Home
            public const string Home = "/admin/home";

            // Auth
            public const string Login = "/admin/login";

            // Product
            public const string Product = "/admin/products";
            public const string ProductDetail = "/admin/products/:id/detail";
            public const string CreateProduct = "/admin/products/create";
            public const string EditProduct = "/admin/products/:id/edit";

            // Product Variant
            public const string ProductVariant = "/admin/product-variants";
            public const string CreateProductVariant = "/admin/:productId/product-variants/create";
            public const string UpdateProductVariant = "/admin/product-variants/:id/edit";

            // Brand
            public const string Brand = "/admin/brands";
            public const string CreateBrand = "/admin/brands/create";
            public const string EditBrand = "/admin/brands/edit";

            // Category
            public const string Category = "/admin/categories";
            public const string CreateCategory = "/admin/categories/create";
            public const string CreateChildCategory = "/categories/:id/child";
            public const string EditCategory = "/admin/categories/edit";

            // Size
            public const string Size = "/admin/sizes";
            public const string CreateSize = "/admin/sizes/create";
            public const string EditSize = "/admin/sizes/edit";

            // Color
            public const string Color = "/admin/colors";
            public const string CreateColor = "/admin/colors/create";
            public const string EditColor = "/admin/colors/edit";

            // Sale
            public const string Sale = "/admin/sales";
            public const string CreateSale = "/admin/sales/create";
            public const string EditSale = "/admin/sales/edit";

            // Coupon
            public const string Coupon = "/admin/coupons";
            public const string CreateCoupon = "/admin/coupons/create";
            public const string EditCoupon = "/admin/coupons/:id/edit";

            // User
            public const string User = "/admin/users";
            public const string CreateUser = "/admin/users/create";
            public const string EditUser = "/admin/users/edit";

            // Customer
            public const string Customer = "/admin/customers";
            public const string CreateCustomer = "/admin/customers/create";
            public const string EditCustomer = "/admin/customers/:id/edit";

            // Bill
            public const string Bill = "/admin/orders";
            public const string BillDetail = "/admin/orders/:id/detail";

            // Statistical
            public const string Statistical = "/admin/statistical";
        }
    }
}
