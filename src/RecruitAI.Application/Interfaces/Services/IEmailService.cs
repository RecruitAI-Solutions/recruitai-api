namespace RecruitAI.Application.Interfaces.Services
{
	public interface IEmailService
	{
		/// <summary>
		/// Gửi email đơn giản (plain text)
		/// </summary>
		/// <param name="to">Email người nhận</param>
		/// <param name="subject">Tiêu đề email</param>
		/// <param name="body">Nội dung email (plain text)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email HTML
		/// </summary>
		/// <param name="to">Email người nhận</param>
		/// <param name="subject">Tiêu đề email</param>
		/// <param name="htmlBody">Nội dung email (HTML)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task SendHtmlEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email đặt lại mật khẩu
		/// </summary>
		/// <param name="to">Email người nhận</param>
		/// <param name="resetLink">Link đặt lại mật khẩu</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task SendPasswordResetEmailAsync(string to, string resetLink, string? userName = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email xác thực tài khoản
		/// </summary>
		/// <param name="to">Email người nhận</param>
		/// <param name="verificationLink">Link xác thực email</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task SendVerificationEmailAsync(string to, string verificationLink, string? userName = null, CancellationToken cancellationToken = default);
	}
}