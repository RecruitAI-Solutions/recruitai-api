// RecruitAI.Domain/Enums/DisplayType.cs
namespace RecruitAI.Domain.Enums
{
	/// <summary>
	/// Kiểu hiển thị địa chỉ theo Vietmap API v4
	/// </summary>
	public enum DisplayType
	{
		/// <summary>
		/// Format mới (2 cấp: ward → city)
		/// </summary>
		NewOnly = 1,

		/// <summary>
		/// Format cũ (3 cấp: ward → district → city)
		/// </summary>
		OldOnly = 2,

		/// <summary>
		/// Tự động detect theo input
		/// </summary>
		Auto = 3,

		/// <summary>
		/// Trả về format mới, kèm old boundaries (khuyến nghị)
		/// </summary>
		BothNewWithOld = 5,

		/// <summary>
		/// Trả về format cũ, kèm new boundaries
		/// </summary>
		BothOldWithNew = 6
	}
}