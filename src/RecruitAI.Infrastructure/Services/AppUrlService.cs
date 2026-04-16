using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RecruitAI.Application.Interfaces;

namespace RecruitAI.Infrastructure.Services
{
	public class AppUrlService : IAppUrlService
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AppUrlService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
		{
			_configuration = configuration;
			_httpContextAccessor = httpContextAccessor;
		}

		public string GetClientUrl()
		{
			return _configuration["App:ClientUrl"] ?? "https://recruitai.com";
		}

		public string GetApiUrl()
		{
			var request = _httpContextAccessor.HttpContext?.Request;
			if (request == null) return "https://api.recruitai.com";

			return $"{request.Scheme}://{request.Host}";
		}
	}
}
