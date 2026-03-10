// RecruitAI.Domain/Extensions/ErrorCodeExtensions.cs
using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Extensions
{
    public static class ErrorCodeExtensions
    {
        public static int GetStatusCode(this ErrorCode errorCode) => errorCode switch
        {
            // ===== 400 BAD REQUEST =====
            ErrorCode.InvalidRefreshToken => 400,
            ErrorCode.InvalidOtp => 400,
            ErrorCode.EmailAlreadyVerified => 400,
            ErrorCode.TokenFormatInvalid => 400,
            ErrorCode.ValidationFailed => 400,
            ErrorCode.InvalidEmail => 400,
            ErrorCode.PasswordTooWeak => 400,
            ErrorCode.RequiredFieldMissing => 400,
            ErrorCode.InvalidLength => 400,
            ErrorCode.InvalidValue => 400,
            ErrorCode.InvalidPhoneNumber => 400,
            ErrorCode.InvalidDate => 400,
            ErrorCode.InvalidFile => 400,
            ErrorCode.FileTooLarge => 400,
            ErrorCode.UnsupportedFileFormat => 400,
            ErrorCode.InvalidUrl => 400,
            ErrorCode.InvalidAddress => 400,
            ErrorCode.InvalidTaxCode => 400,
            ErrorCode.InvalidIdentityNumber => 400,
            ErrorCode.ActionNotAllowed => 400,
            ErrorCode.InvalidData => 400,
            ErrorCode.CannotPerformAction => 400,
            ErrorCode.InvalidState => 400,
            ErrorCode.ConfirmationRequired => 400,
            ErrorCode.FileCorrupted => 400,
            ErrorCode.InvalidPaymentMethod => 400,
            ErrorCode.TransactionCancelled => 400,
            ErrorCode.TransactionAlreadyCompleted => 400,

            // ===== 401 UNAUTHORIZED =====
            ErrorCode.InvalidCredentials => 401,
            ErrorCode.InvalidToken => 401,
            ErrorCode.TokenExpired => 401,
            ErrorCode.TokenRevoked => 401,
            ErrorCode.TokenMissing => 401,
            ErrorCode.Unauthorized => 401,
            ErrorCode.SessionExpired => 401,

            // ===== 402 PAYMENT REQUIRED =====
            ErrorCode.PaymentFailed => 402,
            ErrorCode.InsufficientBalance => 402,

            // ===== 403 FORBIDDEN =====
            ErrorCode.AccountLocked => 403,
            ErrorCode.EmailNotVerified => 403,
            ErrorCode.AccountNotActive => 403,
            ErrorCode.Forbidden => 403,
            ErrorCode.ResourceForbidden => 403,

            // ===== 404 NOT FOUND =====
            ErrorCode.UserNotFound => 404,
            ErrorCode.RoleNotFound => 404,
            ErrorCode.PermissionNotFound => 404,
            ErrorCode.ResourceNotFound => 404,
            ErrorCode.TransactionNotFound => 404,
            ErrorCode.FileNotFound => 404,

            // ===== 409 CONFLICT =====
            ErrorCode.EmailAlreadyExists => 409,
            ErrorCode.ResourceAlreadyExists => 409,
            ErrorCode.ResourceCannotBeDeleted => 409,
            ErrorCode.ResourceInUse => 409,
            ErrorCode.DataChanged => 409,
            ErrorCode.TransactionProcessing => 409,

            // ===== 410 GONE =====
            ErrorCode.OtpExpired => 410,
            ErrorCode.Expired => 410,

            // ===== 429 TOO MANY REQUESTS =====
            ErrorCode.TooManyLoginAttempts => 429,
            ErrorCode.LimitExceeded => 429,

            // ===== 500 INTERNAL SERVER ERROR =====
            ErrorCode.TokenGenerationFailed => 500,
            ErrorCode.InternalServerError => 500,
            ErrorCode.DatabaseError => 500,
            ErrorCode.ConfigurationError => 500,
            ErrorCode.UnknownError => 500,
            ErrorCode.ProcessingFailed => 500,
            ErrorCode.FileUploadFailed => 500,
            ErrorCode.FileDownloadFailed => 500,

            // ===== 502 BAD GATEWAY =====
            ErrorCode.ExternalServiceError => 502,
            ErrorCode.FacebookApiError => 502,
            ErrorCode.GoogleApiError => 502,
            ErrorCode.GithubApiError => 502,
            ErrorCode.SmsServiceError => 502,
            ErrorCode.EmailServiceError => 502,

            // ===== 503 SERVICE UNAVAILABLE =====
            ErrorCode.ConnectionError => 503,
            ErrorCode.ServiceUnavailable => 503,

            // ===== 504 GATEWAY TIMEOUT =====
            ErrorCode.Timeout => 504,

            // Default fallback
            _ => 500
        };
    }
}