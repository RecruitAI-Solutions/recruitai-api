// RecruitAI.Domain/Enums/BoundaryType.cs
namespace RecruitAI.Domain.Enums
{
	/// <summary>
	/// Loại đơn vị hành chính
	/// </summary>
	public enum BoundaryType
	{
		/// <summary>
		/// Tỉnh/Thành phố trực thuộc trung ương
		/// </summary>
		Province = 0,

		/// <summary>
		/// Quận/Huyện/Thị xã/Thành phố thuộc tỉnh
		/// </summary>
		District = 1,

		/// <summary>
		/// Phường/Xã/Thị trấn
		/// </summary>
		Ward = 2
	}
}