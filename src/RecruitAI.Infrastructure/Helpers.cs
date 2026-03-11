using Microsoft.AspNetCore.Http;

namespace RecruitAI.Infrastructure.Helpers
{
    public static class IpHelper
    {
        public static string? GetClientIpAddress(HttpContext? httpContext)
        {
            if (httpContext == null) return null;

            // Ưu tiên header X-Forwarded-For (khi chạy sau proxy)
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ips = forwardedFor.ToString().Split(',');
                return ips.FirstOrDefault()?.Trim();
            }

            // Fallback về RemoteIpAddress
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}