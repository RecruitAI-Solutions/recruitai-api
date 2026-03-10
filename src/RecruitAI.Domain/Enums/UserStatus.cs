namespace RecruitAI.Domain.Enums
{
	public enum UserStatus
	{
		/// <summary>
		/// Tài khoản đang hoạt động bình thường
		/// </summary>
		Active = 1,

		/// <summary>
		/// Tài khoản chưa kích hoạt (cần xác thực email)
		/// </summary>
		Inactive = 2,

		/// <summary>
		/// Tài khoản bị khóa do vi phạm
		/// </summary>
		Locked = 3,

		/// <summary>
		/// Tài khoản đang chờ xác thực
		/// </summary>
		PendingVerification = 4,

		/// <summary>
		/// Tài khoản đã bị xóa (soft delete)
		/// </summary>
		Deleted = 5
	}
}
