namespace RecruitAI.Application.Helpers
{
	public static class AuditExtensions
	{
		/// <summary>
		/// Chuyển đổi Guid thành string cho EntityId
		/// </summary>
		public static string ToEntityId(this Guid id) => id.ToString();

		/// <summary>
		/// Chuyển đổi int thành string cho EntityId (dùng cho Skill, Permission, etc.)
		/// </summary>
		public static string ToEntityId(this int id) => id.ToString();

		/// <summary>
		/// Chuyển đổi int thành string với prefix (phân biệt loại entity)
		/// </summary>
		public static string ToEntityIdWithPrefix(this int id, string prefix) => $"{prefix}_{id}";
	}
}