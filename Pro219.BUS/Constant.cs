namespace Pro219.API
{
    public class Constant
    {
        public static class ErrorCode
        {
            public const string EmailOrPhoneRequired = "email_phone_required";
            public const string EmailOrPhoneNotFound = "email_phone_not_found";
            public const string EmailOrPhoneAlreadyExit = "email_phone_already_exit";
            public const string CustomerNotFound = "customer_not_found";
            public const string CustomerNotFoundWidthEmailOrPhone = "customer_not_found_with_email_or_phone";
            public const string Unauthorized = "unauthorized";
            public const string TokenExpired = "token_expired";
            public const string InvalidToken = "invalid_token";

            public const string NotFound = "not_found";
            public const string DataNotFound = "data_not_found";

            public const string InvalidData = "invalid_data";
            public const string DataRequired = "data_required";
           

            public const string OtherError = "other_error";
            public const string DatabaseError = "database_error";

            public const string OutOfStock = "out_of_stock";

        }

        public static class CustomerType
        {
            public const byte GuestOrder = 1;
            public const byte RegisteredOrder = 2;
        }

        public static class OrderStatus
        {
            public const string PaymentPending = "Chờ thanh toán";
            public const string PaymentCompleted = "Đã thanh toán";
            public const string PaymentCancelled = "Hủy thanh toán";

            public const string OrderStatusWaitingForPayment = "Đang chờ thanh toán chuyển khoản";
            public const string OrderStatusPending = "Đang chờ xử lý";
            public const string OrderStatusConfirm = "Đã xác nhận";
            public const string OrderStatusCanceledByUser = "Đã hủy bởi người dùng";
            public const string OrderStatusShipping = "Đang giao hàng";
            public const string OrderStatusShippingDone = "Đã giao hàng";
            public const string OrderStatusShippingFailed = "Giao hàng thất bại";
            public const string OrderStatusDone = "Hoàn thành";

            public const byte StatusWaitingForPayment = 0;
            public const byte StatusPending = 1;
            public const byte StatusConfirm = 2;
            public const byte StatusCanceledByUser = 3;
            public const byte StatusShipping = 4;
            public const byte StatusShippingDone = 5;
            public const byte StatusShippingFailed = 6;
            public const byte StatusDone = 7;


        }

        public static class DiscountCode { 
        
            public const byte StatusPercentage = 1;
            public const byte StatusFixedAmount = 2;
        }
    }
}
