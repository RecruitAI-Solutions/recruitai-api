using Microsoft.Extensions.Localization;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Resources;

namespace RecruitAI.Application.Helpers
{
	public class MessageService : IMessageService
	{
		private readonly IStringLocalizer<Messages> _localizer;

		public MessageService(IStringLocalizer<Messages> localizer)
		{
			_localizer = localizer;
		}

		public string Get(string key, params object[] args)
		{
			var message = _localizer[key];
			return args?.Length > 0 ? string.Format(message, args) : message;
		}

		public string Business(string key, params object[] args) =>
			Get($"Business{key}", args);

		public string Log(string key, params object[] args) =>
			Get($"Log{key}", args);

		public string Validation(string key, params object[] args) =>
			Get($"Validation{key}", args);

		public string Success(string key, params object[] args) =>
			Get($"Success{key}", args);
	}
}