// RecruitAI.API/Helpers/ErrorCodeMapper.cs
using RecruitAI.Domain.Enums;

namespace RecruitAI.API.Helpers
{
	public static class ErrorCodeMapper
	{
		private static readonly Dictionary<string, (ErrorCode Code, int StatusCode)> _map = new()
		{
			// ===== AUTHENTICATION (1000-1999) =====
			["EmailExists"] = (ErrorCode.EmailAlreadyExists, 409),
			["InvalidCredentials"] = (ErrorCode.InvalidCredentials, 401),
			["AccountLocked"] = (ErrorCode.AccountLocked, 403),
			["UserNotFound"] = (ErrorCode.UserNotFound, 404),
			["EmailNotVerified"] = (ErrorCode.EmailNotVerified, 403),
			["TooManyLoginAttempts"] = (ErrorCode.TooManyLoginAttempts, 429),
			["AccountNotActive"] = (ErrorCode.AccountNotActive, 403),
			["InvalidRefreshToken"] = (ErrorCode.InvalidRefreshToken, 400),
			["InvalidOtp"] = (ErrorCode.InvalidOtp, 400),
			["OtpExpired"] = (ErrorCode.OtpExpired, 410),
			["EmailAlreadyVerified"] = (ErrorCode.EmailAlreadyVerified, 400),

			// ===== TOKEN (2000-2999) =====
			["InvalidToken"] = (ErrorCode.InvalidToken, 401),
			["TokenExpired"] = (ErrorCode.TokenExpired, 401),
			["TokenRevoked"] = (ErrorCode.TokenRevoked, 401),
			["TokenMissing"] = (ErrorCode.TokenMissing, 401),
			["TokenFormatInvalid"] = (ErrorCode.TokenFormatInvalid, 400),
			["TokenGenerationFailed"] = (ErrorCode.TokenGenerationFailed, 500),

			// ===== VALIDATION (3000-3999) =====
			["EmailRequired"] = (ErrorCode.ValidationFailed, 400),
			["EmailInvalid"] = (ErrorCode.ValidationFailed, 400),
			["PasswordRequired"] = (ErrorCode.ValidationFailed, 400),
			["PasswordMinLength"] = (ErrorCode.ValidationFailed, 400),
			["FullNameRequired"] = (ErrorCode.ValidationFailed, 400),
			["InvalidLength"] = (ErrorCode.InvalidLength, 400),
			["InvalidValue"] = (ErrorCode.InvalidValue, 400),
			["InvalidPhoneNumber"] = (ErrorCode.InvalidPhoneNumber, 400),
			["InvalidDate"] = (ErrorCode.InvalidDate, 400),
			["InvalidFile"] = (ErrorCode.InvalidFile, 400),
			["FileTooLarge"] = (ErrorCode.FileTooLarge, 400),
			["UnsupportedFileFormat"] = (ErrorCode.UnsupportedFileFormat, 400),
			["InvalidUrl"] = (ErrorCode.InvalidUrl, 400),
			["InvalidAddress"] = (ErrorCode.InvalidAddress, 400),
			["InvalidTaxCode"] = (ErrorCode.InvalidTaxCode, 400),
			["InvalidIdentityNumber"] = (ErrorCode.InvalidIdentityNumber, 400),

			// ===== AUTHORIZATION (4000-4999) =====
			["Unauthorized"] = (ErrorCode.Unauthorized, 401),
			["Forbidden"] = (ErrorCode.Forbidden, 403),
			["ResourceForbidden"] = (ErrorCode.ResourceForbidden, 403),
			["RoleNotFound"] = (ErrorCode.RoleNotFound, 404),
			["PermissionNotFound"] = (ErrorCode.PermissionNotFound, 404),
			["SessionExpired"] = (ErrorCode.SessionExpired, 401),

			// ===== RESOURCE (4500-4599) =====
			["ResourceNotFound"] = (ErrorCode.ResourceNotFound, 404),
			["ResourceAlreadyExists"] = (ErrorCode.ResourceAlreadyExists, 409),
			["ResourceCannotBeDeleted"] = (ErrorCode.ResourceCannotBeDeleted, 409),
			["ResourceInUse"] = (ErrorCode.ResourceInUse, 409),

			// ===== SERVER (5000-5999) =====
			["InternalServerError"] = (ErrorCode.InternalServerError, 500),
			["DatabaseError"] = (ErrorCode.DatabaseError, 500),
			["ConnectionError"] = (ErrorCode.ConnectionError, 503),
			["ServiceUnavailable"] = (ErrorCode.ServiceUnavailable, 503),
			["Timeout"] = (ErrorCode.Timeout, 504),
			["ExternalServiceError"] = (ErrorCode.ExternalServiceError, 502),
			["ConfigurationError"] = (ErrorCode.ConfigurationError, 500),
			["UnknownError"] = (ErrorCode.UnknownError, 500),

			// ===== BUSINESS LOGIC (6000-6999) =====
			["ActionNotAllowed"] = (ErrorCode.ActionNotAllowed, 400),
			["InvalidData"] = (ErrorCode.InvalidData, 400),
			["ProcessingFailed"] = (ErrorCode.ProcessingFailed, 500),
			["CannotPerformAction"] = (ErrorCode.CannotPerformAction, 400),
			["LimitExceeded"] = (ErrorCode.LimitExceeded, 429),
			["Expired"] = (ErrorCode.Expired, 410),
			["InvalidState"] = (ErrorCode.InvalidState, 400),
			["DataChanged"] = (ErrorCode.DataChanged, 409),
			["ConfirmationRequired"] = (ErrorCode.ConfirmationRequired, 400),

			// ===== FILE (7000-7999) =====
			["FileNotFound"] = (ErrorCode.FileNotFound, 404),
			["FileUploadFailed"] = (ErrorCode.FileUploadFailed, 500),
			["FileDownloadFailed"] = (ErrorCode.FileDownloadFailed, 500),
			["FileCorrupted"] = (ErrorCode.FileCorrupted, 400),

			// ===== PAYMENT (8000-8999) =====
			["PaymentFailed"] = (ErrorCode.PaymentFailed, 402),
			["InsufficientBalance"] = (ErrorCode.InsufficientBalance, 402),
			["InvalidPaymentMethod"] = (ErrorCode.InvalidPaymentMethod, 400),
			["TransactionCancelled"] = (ErrorCode.TransactionCancelled, 400),
			["TransactionNotFound"] = (ErrorCode.TransactionNotFound, 404),
			["TransactionAlreadyCompleted"] = (ErrorCode.TransactionAlreadyCompleted, 400),
			["TransactionProcessing"] = (ErrorCode.TransactionProcessing, 409),

			// ===== JWT MESSAGES =====
			["JwtKeyNotConfigured"] = (ErrorCode.InternalServerError, 500),
			["JwtGenerationFailed"] = (ErrorCode.TokenGenerationFailed, 500),
			["UserNull"] = (ErrorCode.ValidationFailed, 400),
			["UserIdEmpty"] = (ErrorCode.ValidationFailed, 400),
			["UserEmailEmpty"] = (ErrorCode.ValidationFailed, 400),

			// ===== SUCCESS MESSAGES (không phải lỗi) =====
			// Có thể dùng cho response thành công nếu cần
			["RegistrationSuccess"] = (ErrorCode.InternalServerError, 200), // Special case
			["LoginSuccess"] = (ErrorCode.InternalServerError, 200),        // Special case
			["LogoutSuccess"] = (ErrorCode.InternalServerError, 200)        // Special case
		};

		public static (ErrorCode Code, int StatusCode) GetErrorInfo(string messageKey)
		{
			return _map.TryGetValue(messageKey, out var info)
				? info
				: (ErrorCode.UnknownError, 500);
		}

		// Thêm method để kiểm tra có phải success không
		public static bool IsSuccess(string messageKey)
		{
			return messageKey.Contains("Success") && _map.TryGetValue(messageKey, out var info) && info.StatusCode == 200;
		}
	}
}