// RecruitAI.Domain/Enums/DisplayType.cs
namespace RecruitAI.Domain.Enums
{
	/// <summary>
	/// Kiểu hiển thị địa chỉ
	/// </summary>
	public enum DisplayType
	{
		/// <summary>
		/// Chỉ hiển thị format mới (2 cấp)
		/// </summary>
		NewOnly = 1,

		/// <summary>
		/// Chỉ hiển thị format cũ (3 cấp)
		/// </summary>
		OldOnly = 2,

		/// <summary>
		/// Hiển thị cả format cũ và mới
		/// </summary>
		Both = 6
	}
}