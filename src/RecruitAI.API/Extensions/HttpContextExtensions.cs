namespace RecruitAI.API.Extensions
{
	public interface IClientIpService
	{
		string GetClientIpAddress();
	}
	public static class HttpContextExtensions
	{
		public static string GetClientIpAddress(this HttpContext httpContext)
		{
			if (httpContext == null) return "unknown";

			try
			{
				// 1. Kiểm tra header X-Forwarded-For (khi chạy sau proxy/load balancer)
				if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
				{
					var ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
					if (ips.Any())
						return ips.First().Trim();
				}

				// 2. Kiểm tra header CF-Connecting-IP (Cloudflare)
				if (httpContext.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp))
				{
					return cfIp.ToString();
				}

				// 3. Kiểm tra header X-Real-IP (Nginx)
				if (httpContext.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
				{
					return realIp.ToString();
				}

				// 4. Fallback về RemoteIpAddress
				var remoteIp = httpContext.Connection.RemoteIpAddress;
				if (remoteIp != null)
				{
					// Nếu là IPv6 localhost (::1) thì trả về 127.0.0.1
					if (remoteIp.IsIPv6SiteLocal || remoteIp.ToString() == "::1")
						return "127.0.0.1";

					return remoteIp.ToString();
				}

				return "unknown";
			}
			catch
			{
				return "unknown";
			}
		}
	}
}