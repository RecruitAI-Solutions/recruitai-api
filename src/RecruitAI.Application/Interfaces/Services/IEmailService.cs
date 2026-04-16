using RecruitAI.Application.DTOs.Responses.Applications;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IEmailService
	{
		/// <summary>
		/// Gửi email đơn giản (plain text)
		/// </summary>
		Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email HTML
		/// </summary>
		Task SendHtmlEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email đặt lại mật khẩu
		/// </summary>
		Task SendPasswordResetEmailAsync(string to, string resetLink, string? userName = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email xác thực tài khoản
		/// </summary>
		Task SendVerificationEmailAsync(string to, string verificationLink, string? userName = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email xác nhận ứng tuyển
		/// </summary>
		Task SendJobApplicationEmailAsync(string to, string userName, string jobTitle,
			string companyName, string jobLocation, string salaryRange,
			DateTime appliedDate, string trackingLink, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email mời phỏng vấn
		/// </summary>
		Task SendInterviewInvitationEmailAsync(string to, string userName, string jobTitle,
			string companyName, DateTime interviewDate, string interviewTime,
			string interviewLocation, string interviewerName, int duration,
			string interviewFormat, string confirmLink, string declineLink,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email nhắc nhở
		/// </summary>
		Task SendReminderEmailAsync(string to, string userName, string reminderSubject,
			string reminderMessage, DateTime reminderTime, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email thông báo hệ thống
		/// </summary>
		Task SendSystemAnnouncementEmailAsync(string to, string userName,
			string announcementTitle, string announcementContent, CancellationToken cancellationToken = default);

		// ========== APPLICATION STATUS EMAILS ==========

		/// <summary>
		/// Gửi email thông báo cập nhật trạng thái chung
		/// </summary>
		Task SendApplicationStatusUpdateEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string status, string? notes, string trackingLink,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email thông báo đã xem xét đơn ứng tuyển (Reviewed)
		/// </summary>
		Task SendApplicationReviewedEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string status, string? notes, string trackingLink,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email thông báo chấp nhận đơn ứng tuyển (Accepted)
		/// </summary>
		Task SendApplicationAcceptedEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string status, string? nextSteps, string trackingLink,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email thông báo từ chối đơn ứng tuyển (Rejected)
		/// </summary>
		Task SendApplicationRejectedEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string reason, string trackingLink,
			CancellationToken cancellationToken = default);
	}
}