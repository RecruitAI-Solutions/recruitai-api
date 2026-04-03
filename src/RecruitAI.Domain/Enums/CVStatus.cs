namespace RecruitAI.Domain.Enums;

public enum CVStatus
{
	Pending = 1,      // Chờ upload
	Uploaded = 2,     // Đã upload
	Processing = 3,   // Đang xử lý
	Completed = 4,    // Đã xử lý text xong
	Analyzed = 5,     // Đã phân tích skills (thêm mới)
	Failed = 6
}