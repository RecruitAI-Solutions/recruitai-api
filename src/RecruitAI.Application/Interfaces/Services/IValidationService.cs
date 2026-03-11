namespace RecruitAI.Application.Interfaces.Services
{
    public interface IValidationService
    {
        /// <summary>
        /// Kiểm tra email đã tồn tại trong hệ thống chưa
        /// </summary>
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra user có tồn tại không
        /// </summary>
        Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra email có thuộc về user không
        /// </summary>
        Task<bool> IsEmailBelongToUserAsync(string email, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra role có hợp lệ không
        /// </summary>
        bool IsValidRole(string role);

        /// <summary>
        /// Kiểm tra mật khẩu có đủ mạnh không
        /// </summary>
        bool IsStrongPassword(string password);
	}
}