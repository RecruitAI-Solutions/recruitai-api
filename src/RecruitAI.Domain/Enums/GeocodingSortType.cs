// RecruitAI.Domain/Enums/GeocodingSortType.cs
namespace RecruitAI.Domain.Enums
{
	/// <summary>
	/// Cách sắp xếp kết quả tìm kiếm địa chỉ
	/// </summary>
	public enum GeocodingSortType
	{
		/// <summary>
		/// Sắp xếp theo độ chính xác (mặc định)
		/// </summary>
		Relevance = 0,

		/// <summary>
		/// Sắp xếp theo khoảng cách từ vị trí hiện tại
		/// </summary>
		Distance = 1,

		/// <summary>
		/// Sắp xếp theo tên địa điểm
		/// </summary>
		Name = 2
	}
}