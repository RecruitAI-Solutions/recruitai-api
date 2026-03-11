using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Interfaces
{
    public interface IWorkContext
    {
        /// <summary>
        /// Lấy ID của user hiện tại
        /// </summary>
        Guid GetCurrentUserId();

        /// <summary>
        /// Lấy Email của user hiện tại
        /// </summary>
        string GetCurrentUserEmail();

        /// <summary>
        /// Lấy Role của user hiện tại
        /// </summary>
        UserRole GetCurrentUserRole();

        /// <summary>
        /// Lấy toàn bộ thông tin user hiện tại (có cache)
        /// </summary>
        Task<User> GetCurrentUserAsync();

        /// <summary>
        /// Kiểm tra user đã đăng nhập chưa
        /// </summary>
        bool IsAuthenticated();

        /// <summary>
        /// Kiểm tra user có role cụ thể không
        /// </summary>
        bool IsInRole(UserRole role);

        /// <summary>
        /// Lấy IP của client hiện tại
        /// </summary>
        string GetCurrentIpAddress();
    }
}