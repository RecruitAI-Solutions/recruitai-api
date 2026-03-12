namespace RecruitAI.Application.Interfaces.Services
{
	public interface IEmailTemplateService
	{
		/// <summary>
		/// Load template email theo tên và ngôn ngữ
		/// </summary>
		/// <param name="templateName">Tên template (vd: reset-password, verify-email)</param>
		/// <param name="language">Ngôn ngữ (vi/en)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Nội dung template dạng string</returns>
		Task<string> LoadTemplateAsync(string templateName, string language, CancellationToken cancellationToken = default);

		/// <summary>
		/// Thay thế các placeholder trong template bằng giá trị thực
		/// </summary>
		/// <param name="template">Nội dung template</param>
		/// <param name="placeholders">Dictionary chứa các cặp key-value cần thay thế</param>
		/// <returns>Template đã được thay thế placeholder</returns>
		string ReplacePlaceholders(string template, Dictionary<string, string> placeholders);
	}
}