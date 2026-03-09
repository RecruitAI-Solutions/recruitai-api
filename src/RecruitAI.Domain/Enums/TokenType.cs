namespace RecruitAI.Domain.Enums
{
    public enum TokenType
    {
        /// <summary>
        /// Token truy cập API (ngắn hạn)
        /// </summary>
        AccessToken = 1,

        /// <summary>
        /// Token làm mới (dài hạn)
        /// </summary>
        RefreshToken = 2,

        /// <summary>
        /// Token xác thực email
        /// </summary>
        EmailVerification = 3,

        /// <summary>
        /// Token đặt lại mật khẩu
        /// </summary>
        PasswordReset = 4,

        /// <summary>
        /// Token xác thực 2 lớp
        /// </summary>
        TwoFactor = 5
    }
}
