namespace RecruitAI.Application.Interfaces.Services
{
	public interface IEmailService
	{
		/// <summary>
		/// Gửi email đơn giản
		/// </summary>
		Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email HTML
		/// </summary>
		Task SendHtmlEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email reset password
		/// </summary>
		Task SendPasswordResetEmailAsync(string to, string resetLink, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gửi email xác thực tài khoản
		/// </summary>
		Task SendVerificationEmailAsync(string to, string verificationLink, CancellationToken cancellationToken = default);
	}
}