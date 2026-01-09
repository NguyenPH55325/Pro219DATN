namespace Pro219.Web.Constants
{
    public static class Constant
    {
        public const string PageTitleUser = "Adams Store -";
        public const string PageTitleAdmin = "Adams Store Management -";

        public const string TokenNameLocalStorage = "token";
        public const string TokenExpiredLocalStorage = "expired";
        public const string UserInfoLocalStorage = "userInfo";
        public const string UserFirstLoginLocalStorage = "firstLogin";
        public const string GuestCartLocalStorage = "GuestCartData";
        public const string AuthCartIdLocalStorage = "AuthCartIdKey";
        public const string CartItemKey = "CartItemKey";
        public const string OrderPOSTemp = "OrderPOSTemp";
        public const string ExpiredCartTempUser = "CartTempUserTime";

        public const string DefaultImages = "/Assets/Images/default-image.png";
        public const int DefaultSkeletons = 10;

        public const string DiscountTypePercent = "Percentage";
        public const string DiscountTypeFixed = "FixedAmount";
        public static class Regex
        {
            public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,16}$";
            public const string PhoneNumber = @"(03|05|07|08|09|01[2|6|8|9])+([0-9]{8})\b";
            public const string HexColor = @"^#([A-Fa-f0-9]{8}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
            public const string Pattern = @"^\d+$";
            public const string SKU = @"^[A-Z0-9-_]+$";
        }

        public static class MessageValid
        {
            public const string Password = "Mật khẩu phải có từ 8 đến 16 ký tự chữ và số, bao gồm cả chữ hoa, chữ thường, số và ký hiệu.";
            public const string Required = "Không được để trống.";
            public const string Max2000 = "Tối đa 2000 kí tự.";
            public const string Max1000 = "Tối đa 1000 kí tự.";
            public const string Max255 = "Tối đa 255 kí tự.";
            public const string Max500 = "Tối đa 500 kí tự.";
            public const string Max200 = "Tối đa 200 kí tự.";
            public const string Max20 = "Tối đa 20 kí tự.";
            public const string Max100 = "Tối đa 100 kí tự.";
            public const string Max50 = "Tối đa 50 kí tự.";
            public const string Min2 = "Tối thiểu 2 kí tự.";
            public const string Email = "Sai định dạng Email.";
            public const string PhoneNumber = "Sai định dạng số điện thoại.";
            public const string PhoneNumberLength = "Tối thiểu 10 số và tối đa 11 số";
            public const string DateFuture = "Dữ liệu ngày tháng không hợp lệ. Vui lòng chọn ngày tháng hiện tại đổ lại.";
            public const string HexColor = "Mã màu không hợp lệ.";
        }

        public static class Role
        {
            public const string Admin = "Admin";
            public const string Manager = "Manager";
            public const string Customer = "Customer";
        }

        public static class CascadingNameParams
        {
            public const string UserInfo = "CurrentUserInfo";
        }

        public static class ErrorCode
        {
            public const string EmailOrPhoneAlreadyExit = "email_phone_already_exit";
            public const string EmailOrPhoneRequired = "email_phone_required";
            public const string EmailOrPhoneNotFound = "email_phone_not_found";
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

        public static readonly Dictionary<string, string> Errors = new Dictionary<string, string>
        {
            { ErrorCode.EmailOrPhoneAlreadyExit, "Email hoặc số điện thoại này đã được sử dụng." },
            { ErrorCode.EmailOrPhoneRequired, "Hãy nhập email của bạn." },
            { ErrorCode.EmailOrPhoneNotFound, "Email không tồn tại trong hệ thống." },
            { ErrorCode.CustomerNotFound, "Khách hàng không tồn tại." },
            { ErrorCode.CustomerNotFoundWidthEmailOrPhone, "Khách hàng không tồn tại." },
            { ErrorCode.Unauthorized, "unauthorized" },
            { ErrorCode.InvalidToken, "Token invalid" },
            { ErrorCode.TokenExpired, "Token expired" },
            { ErrorCode.NotFound, "Không tìm thấy." },
            { ErrorCode.DataNotFound, "Không có dữ liệu." },
            { ErrorCode.InvalidData, "Dữ liệu không hợp lệ." },
            { ErrorCode.DataRequired, "Thiếu dữ liệu gửi đi." },
            { ErrorCode.DatabaseError, "Lỗi database." },
            { ErrorCode.OtherError, "Đã có lỗi xảy ra." },
            { ErrorCode.OutOfStock, "Số lượng đạt tối đa."},
            { "", "Đã có lỗi xảy ra." },
        };

        public static class ErrorSatusCode
        {
            public const int BadRequest = 400;
            public const int Fobidden = 403;
            public const int NotFound = 404;
            public const int Internal = 500;
        }

        public static class StatusDefault
        {
            public const byte Active = 1;
            public const byte InActive = 0;
        }

        public static class Pagination
        {
            public const int DefaultPage = 1;
            public const int DefaultPerPage = 20;
        }
        public static class PaymentMethod
        {
            public const int CashBack = 1;
            public const int Transfer = 2;
        }

        public static class OrderStatus
        {
            public const string OrderCheckoutPending = "Chờ thanh toán";
            public const string OrderStatusPending = "Đang chờ xử lý";
            public const string OrderStatusConfirm = "Đã xác nhận";
            public const string OrderStatusCanceledByUser = "Đã hủy";
            public const string OrderStatusShipping = "Đang giao hàng";
            public const string OrderStatusShippingDone = "Đã giao hàng";
            public const string OrderStatusShippingFailed = "Giao hàng thất bại";
            public const string OrderStatusDone = "Hoàn thành";

            public const byte StatusNoCheckout = 0;
            public const byte StatusPending = 1;
            public const byte StatusConfirm = 2;
            public const byte StatusCanceledByUser = 3;
            public const byte StatusShipping = 4;
            public const byte StatusShippingDone = 5;
            public const byte StatusShippingFailed = 6;
            public const byte StatusDone = 7;
        }

        public class OrderStatusItem
        {
            public byte id { get; set; }
            public string label { get; set; }
        }

        public static readonly OrderStatusItem[] OrderStatusArray = new OrderStatusItem[]
        {
            new OrderStatusItem { id = OrderStatus.StatusNoCheckout, label = OrderStatus.OrderCheckoutPending },
            new OrderStatusItem { id = OrderStatus.StatusPending, label = OrderStatus.OrderStatusPending },
            new OrderStatusItem { id = OrderStatus.StatusConfirm, label = OrderStatus.OrderStatusConfirm },
            new OrderStatusItem { id = OrderStatus.StatusCanceledByUser, label = OrderStatus.OrderStatusCanceledByUser },
            new OrderStatusItem { id = OrderStatus.StatusShipping, label = OrderStatus.OrderStatusShipping },
            new OrderStatusItem { id = OrderStatus.StatusShippingDone, label = OrderStatus.OrderStatusShippingDone },
            new OrderStatusItem { id = OrderStatus.StatusShippingFailed, label = OrderStatus.OrderStatusShippingFailed },
            new OrderStatusItem { id = OrderStatus.StatusDone, label = OrderStatus.OrderStatusDone }
        };

        public static class DiscountType
        {
            public const int Order = 1;
            public const int Shipping = 2;
        }
    }
}
