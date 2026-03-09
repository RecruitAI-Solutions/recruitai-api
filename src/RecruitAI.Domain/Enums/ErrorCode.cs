namespace RecruitAI.Domain.Enums
{
    /// <summary>
    /// Mã lỗi chuẩn cho API responses
    /// </summary>
    public enum ErrorCode
    {
        // ===== Authentication errors (1000-1999) =====
        /// <summary>Email hoặc mật khẩu không đúng</summary>
        InvalidCredentials = 1001,

        /// <summary>Email đã tồn tại</summary>
        EmailAlreadyExists = 1002,

        /// <summary>Không tìm thấy người dùng</summary>
        UserNotFound = 1003,

        /// <summary>Tài khoản đã bị khóa</summary>
        AccountLocked = 1004,

        /// <summary>Email chưa được xác thực</summary>
        EmailNotVerified = 1005,

        // ===== Token errors (2000-2999) =====
        /// <summary>Token không hợp lệ</summary>
        InvalidToken = 2001,

        /// <summary>Token đã hết hạn</summary>
        TokenExpired = 2002,

        /// <summary>Token đã bị thu hồi</summary>
        TokenRevoked = 2003,

        /// <summary>Thiếu token trong request</summary>
        TokenMissing = 2004,

        // ===== Validation errors (3000-3999) =====
        /// <summary>Dữ liệu đầu vào không hợp lệ</summary>
        ValidationFailed = 3001,

        /// <summary>Email không đúng định dạng</summary>
        InvalidEmail = 3002,

        /// <summary>Mật khẩu quá yếu</summary>
        PasswordTooWeak = 3003,

        /// <summary>Thiếu trường bắt buộc</summary>
        RequiredFieldMissing = 3004,

        // ===== Authorization errors (4000-4999) =====
        /// <summary>Không có quyền truy cập</summary>
        Unauthorized = 4001,

        /// <summary>Không đủ quyền</summary>
        Forbidden = 4002,

        // ===== Server errors (5000-5999) =====
        /// <summary>Lỗi server nội bộ</summary>
        InternalServerError = 5001,

        /// <summary>Lỗi database</summary>
        DatabaseError = 5002,

        /// <summary>Lỗi kết nối</summary>
        ConnectionError = 5003
    }
}
