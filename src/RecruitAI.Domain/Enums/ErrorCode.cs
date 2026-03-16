namespace RecruitAI.Domain.Enums
{
	/// <summary>
	/// Mã lỗi chuẩn cho API responses
	/// </summary>
	public enum ErrorCode
	{
		// ===== AUTHENTICATION ERRORS (1000-1999) =====
		/// <summary>Email hoặc mật khẩu không đúng (401)</summary>
		InvalidCredentials = 1001,

		/// <summary>Email đã tồn tại (409)</summary>
		EmailAlreadyExists = 1002,

		/// <summary>Không tìm thấy người dùng (404)</summary>
		UserNotFound = 1003,

		/// <summary>Tài khoản đã bị khóa (403)</summary>
		AccountLocked = 1004,

		/// <summary>Email chưa được xác thực (403)</summary>
		EmailNotVerified = 1005,

		/// <summary>Đăng nhập thất bại nhiều lần (429)</summary>
		TooManyLoginAttempts = 1006,

		/// <summary>Tài khoản chưa kích hoạt (403)</summary>
		AccountNotActive = 1007,

		/// <summary>Refresh token không hợp lệ (400)</summary>
		InvalidRefreshToken = 1008,

		/// <summary>OTP không hợp lệ (400)</summary>
		InvalidOtp = 1009,

		/// <summary>OTP đã hết hạn (410)</summary>
		OtpExpired = 1010,

		/// <summary>Email đã được xác thực rồi (400)</summary>
		EmailAlreadyVerified = 1011,

		// ===== TOKEN ERRORS (2000-2999) =====
		/// <summary>Token không hợp lệ (401)</summary>
		InvalidToken = 2001,

		/// <summary>Token đã hết hạn (401)</summary>
		TokenExpired = 2002,

		/// <summary>Token đã bị thu hồi (401)</summary>
		TokenRevoked = 2003,

		/// <summary>Thiếu token trong request (401)</summary>
		TokenMissing = 2004,

		/// <summary>Token format không đúng (400)</summary>
		TokenFormatInvalid = 2005,

		/// <summary>Không thể tạo token (500)</summary>
		TokenGenerationFailed = 2006,

		// ===== VALIDATION ERRORS (3000-3999) =====
		/// <summary>Dữ liệu đầu vào không hợp lệ (400)</summary>
		ValidationFailed = 3001,

		/// <summary>Email không đúng định dạng (400)</summary>
		InvalidEmail = 3002,

		/// <summary>Mật khẩu quá yếu (400)</summary>
		PasswordTooWeak = 3003,

		/// <summary>Thiếu trường bắt buộc (400)</summary>
		RequiredFieldMissing = 3004,

		/// <summary>Độ dài không hợp lệ (400)</summary>
		InvalidLength = 3005,

		/// <summary>Giá trị không hợp lệ (400)</summary>
		InvalidValue = 3006,

		/// <summary>Số điện thoại không hợp lệ (400)</summary>
		InvalidPhoneNumber = 3007,

		/// <summary>Ngày tháng không hợp lệ (400)</summary>
		InvalidDate = 3008,

		/// <summary>File không hợp lệ (400)</summary>
		InvalidFile = 3009,

		/// <summary>File quá lớn (400)</summary>
		FileTooLarge = 3010,

		/// <summary>Định dạng file không hỗ trợ (400)</summary>
		UnsupportedFileFormat = 3011,

		/// <summary>URL không hợp lệ (400)</summary>
		InvalidUrl = 3012,

		/// <summary>Địa chỉ không hợp lệ (400)</summary>
		InvalidAddress = 3013,

		/// <summary>Mã số thuế không hợp lệ (400)</summary>
		InvalidTaxCode = 3014,

		/// <summary>CCCD/CMND không hợp lệ (400)</summary>
		InvalidIdentityNumber = 3015,

		/// <summary>Request không hợp lệ (400)</summary>
		InvalidRequest = 3016,

		// ===== AUTHORIZATION ERRORS (4000-4999) =====
		/// <summary>Không có quyền truy cập (401)</summary>
		Unauthorized = 4001,

		/// <summary>Không đủ quyền (403)</summary>
		Forbidden = 4002,

		/// <summary>Không có quyền truy cập tài nguyên này (403)</summary>
		ResourceForbidden = 4003,

		/// <summary>Role không tồn tại (404)</summary>
		RoleNotFound = 4004,

		/// <summary>Permission không tồn tại (404)</summary>
		PermissionNotFound = 4005,

		/// <summary>Session đã hết hạn (401)</summary>
		SessionExpired = 4006,

		// ===== RESOURCE ERRORS (4500-4599) =====
		/// <summary>Không tìm thấy tài nguyên (404)</summary>
		ResourceNotFound = 4501,

		/// <summary>Tài nguyên đã tồn tại (409)</summary>
		ResourceAlreadyExists = 4502,

		/// <summary>Không thể xóa tài nguyên (409)</summary>
		ResourceCannotBeDeleted = 4503,

		/// <summary>Tài nguyên đang được sử dụng (409)</summary>
		ResourceInUse = 4504,

		// ===== SERVER ERRORS (5000-5999) =====
		/// <summary>Lỗi server nội bộ (500)</summary>
		InternalServerError = 5001,

		/// <summary>Lỗi database (500)</summary>
		DatabaseError = 5002,

		/// <summary>Lỗi kết nối (503)</summary>
		ConnectionError = 5003,

		/// <summary>Service không khả dụng (503)</summary>
		ServiceUnavailable = 5004,

		/// <summary>Timeout (504)</summary>
		Timeout = 5005,

		/// <summary>Lỗi third-party service (502)</summary>
		ExternalServiceError = 5006,

		/// <summary>Lỗi cấu hình (500)</summary>
		ConfigurationError = 5007,

		/// <summary>Lỗi không xác định (500)</summary>
		UnknownError = 5008,

		// ===== BUSINESS LOGIC ERRORS (6000-6999) =====
		/// <summary>Hành động không được phép (400)</summary>
		ActionNotAllowed = 6001,

		/// <summary>Dữ liệu không hợp lệ (400)</summary>
		InvalidData = 6002,

		/// <summary>Quá trình xử lý thất bại (500)</summary>
		ProcessingFailed = 6003,

		/// <summary>Không thể thực hiện hành động (400)</summary>
		CannotPerformAction = 6004,

		/// <summary>Giới hạn đã đạt (429)</summary>
		LimitExceeded = 6005,

		/// <summary>Đã hết hạn (410)</summary>
		Expired = 6006,

		/// <summary>Trạng thái không hợp lệ (400)</summary>
		InvalidState = 6007,

		/// <summary>Dữ liệu đã thay đổi (409)</summary>
		DataChanged = 6008,

		/// <summary>Cần xác nhận (400)</summary>
		ConfirmationRequired = 6009,

		// ===== FILE ERRORS (7000-7999) =====
		/// <summary>File không tìm thấy (404)</summary>
		FileNotFound = 7001,

		/// <summary>Không thể upload file (500)</summary>
		FileUploadFailed = 7002,

		/// <summary>Không thể download file (500)</summary>
		FileDownloadFailed = 7003,

		/// <summary>File bị hỏng (400)</summary>
		FileCorrupted = 7004,

		// ===== PAYMENT ERRORS (8000-8999) =====
		/// <summary>Thanh toán thất bại (402)</summary>
		PaymentFailed = 8001,

		/// <summary>Số dư không đủ (402)</summary>
		InsufficientBalance = 8002,

		/// <summary>Phương thức thanh toán không hợp lệ (400)</summary>
		InvalidPaymentMethod = 8003,

		/// <summary>Giao dịch đã bị hủy (400)</summary>
		TransactionCancelled = 8004,

		/// <summary>Giao dịch không tìm thấy (404)</summary>
		TransactionNotFound = 8005,

		/// <summary>Giao dịch đã hoàn thành (400)</summary>
		TransactionAlreadyCompleted = 8006,

		/// <summary>Giao dịch đang xử lý (409)</summary>
		TransactionProcessing = 8007,

		// ===== THIRD-PARTY ERRORS (9000-9999) =====
		/// <summary>Lỗi từ Facebook API (502)</summary>
		FacebookApiError = 9001,

		/// <summary>Lỗi từ Google API (502)</summary>
		GoogleApiError = 9002,

		/// <summary>Lỗi từ Github API (502)</summary>
		GithubApiError = 9003,

		/// <summary>Lỗi từ SMS service (502)</summary>
		SmsServiceError = 9004,

		/// <summary>Lỗi từ Email service (502)</summary>
		EmailServiceError = 9005,

		/// <summary>Request bị hủy bởi client (499)</summary>
		OperationCancelled = 499,  

		// ===== CV RELATED ERRORS (2000-2999) =====
		/// <summary>Không tìm thấy CV</summary>
		CVNotFound = 2001,

		/// <summary>File CV bị thiếu</summary>
		CVFileMissing = 2002,

		/// <summary>Loại file không hợp lệ</summary>
		InvalidFileType = 2003,
	}
}