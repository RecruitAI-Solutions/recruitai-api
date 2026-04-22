using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Infrastructure.Helpers;
using System.Security.Claims;

namespace RecruitAI.Infrastructure.Services
{
	public class WorkContext : IWorkContext
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly RecruitDevContext _context;
		private readonly IMemoryCache _cache;
		private User? _cachedUser;
		private Guid? _cachedUserId;
		private string? _cachedUserEmail;
		private string? _cachedUserFullName;
		private UserRole? _cachedUserRole;

		public WorkContext(
			IHttpContextAccessor httpContextAccessor,
			RecruitDevContext context,
			IMemoryCache cache)
		{
			_httpContextAccessor = httpContextAccessor;
			_context = context;
			_cache = cache;
		}

		/// <inheritdoc />
		public Guid? GetCurrentUserId()
		{
			// Nếu đã cache thì trả về
			if (_cachedUserId.HasValue)
				return _cachedUserId.Value;

			var user = _httpContextAccessor.HttpContext?.User;

			if (user == null || !user.Identity?.IsAuthenticated == true)
				return null;

			// Thử các claim types phổ biến
			var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
				?? user.FindFirst("sub")
				?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
				?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier");

			if (userIdClaim == null)
				return null;

			if (Guid.TryParse(userIdClaim.Value, out var userId))
			{
				_cachedUserId = userId;
				return userId;
			}

			return null;
		}

		/// <inheritdoc />
		public string? GetCurrentUserEmail()
		{
			if (!string.IsNullOrEmpty(_cachedUserEmail))
				return _cachedUserEmail;

			var user = _httpContextAccessor.HttpContext?.User;

			if (user == null || !user.Identity?.IsAuthenticated == true)
				return null;

			var emailClaim = user.FindFirst(ClaimTypes.Email)
				?? user.FindFirst("email")
				?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

			_cachedUserEmail = emailClaim?.Value;
			return _cachedUserEmail;
		}

		/// <inheritdoc />
		public string? GetCurrentUserFullName()
		{
			if (!string.IsNullOrEmpty(_cachedUserFullName))
				return _cachedUserFullName;

			var user = _httpContextAccessor.HttpContext?.User;

			if (user == null || !user.Identity?.IsAuthenticated == true)
				return null;

			// Ưu tiên lấy từ claim FullName trước
			var fullNameClaim = user.FindFirst("FullName")?.Value
							 ?? user.FindFirst(ClaimTypes.GivenName)?.Value
							 ?? user.FindFirst(ClaimTypes.Name)?.Value;

			if (!string.IsNullOrEmpty(fullNameClaim))
			{
				_cachedUserFullName = fullNameClaim;
				return fullNameClaim;
			}

			// Fallback: lấy từ database cache
			var userId = GetCurrentUserId();
			if (userId.HasValue && _cache.TryGetValue($"user_{userId}", out User? cachedUser) && cachedUser != null)
			{
				_cachedUserFullName = cachedUser.FullName;
				return cachedUser.FullName;
			}

			return null;
		}

		/// <inheritdoc />
		public UserRole? GetCurrentUserRole()
		{
			if (_cachedUserRole.HasValue)
				return _cachedUserRole.Value;

			var user = _httpContextAccessor.HttpContext?.User;

			if (user == null || !user.Identity?.IsAuthenticated == true)
				return null;

			var roleClaim = user.FindFirst(ClaimTypes.Role)
				?? user.FindFirst("role")
				?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

			if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, true, out var role))
			{
				_cachedUserRole = role;
				return role;
			}

			return null;
		}

		/// <inheritdoc />
		public async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
		{
			// Nếu đã cache thì trả về
			if (_cachedUser != null)
				return _cachedUser;

			var userId = GetCurrentUserId();
			if (userId == null)
				return null;

			// Thử lấy từ memory cache trước
			var cacheKey = $"user_{userId}";
			if (_cache.TryGetValue(cacheKey, out User? cachedUser) && cachedUser != null)
			{
				_cachedUser = cachedUser;
				return cachedUser;
			}

			// Lấy từ database
			_cachedUser = await _context.Users
				.Include(u => u.AuthProviders)
				.Include(u => u.RefreshTokens.Where(rt => !rt.IsRevoked))
				.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

			// Lưu vào cache (5 phút)
			if (_cachedUser != null)
			{
				_cache.Set(cacheKey, _cachedUser, TimeSpan.FromMinutes(5));
			}

			return _cachedUser;
		}

		/// <inheritdoc />
		public bool IsAuthenticated()
		{
			return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
		}

		/// <inheritdoc />
		public bool IsInRole(UserRole role)
		{
			return GetCurrentUserRole() == role;
		}

		/// <inheritdoc />
		public string? GetCurrentIpAddress()
		{
			return IpHelper.GetClientIpAddress(_httpContextAccessor.HttpContext);
		}

		/// <inheritdoc />
		public void ClearCache()
		{
			var userId = _cachedUserId;
			_cachedUser = null;
			_cachedUserId = null;
			_cachedUserEmail = null;
			_cachedUserFullName = null;
			_cachedUserRole = null;

			// Xóa cache entry nếu có
			if (userId.HasValue)
			{
				_cache.Remove($"user_{userId.Value}");
			}
		}
	}
}