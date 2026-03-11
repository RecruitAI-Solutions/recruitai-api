using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Infrastructure.Helpers;

namespace RecruitAI.Infrastructure.Services
{
	public class WorkContext : IWorkContext
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly RecruitDevContext _context;
		private User? _cachedUser;
		private Guid? _cachedUserId;
		private string? _cachedUserEmail;
		private UserRole? _cachedUserRole;

		public WorkContext(
			IHttpContextAccessor httpContextAccessor,
			RecruitDevContext context)
		{
			_httpContextAccessor = httpContextAccessor;
			_context = context;
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

			_cachedUser = await _context.Users
				.Include(u => u.AuthProviders)
				.Include(u => u.RefreshTokens.Where(rt => !rt.IsRevoked))
				.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

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
			_cachedUser = null;
			_cachedUserId = null;
			_cachedUserEmail = null;
			_cachedUserRole = null;
		}
	}
}