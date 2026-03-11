using System.Security.Claims;

namespace RecruitAI.API.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		public static Guid? GetUserId(this ClaimsPrincipal user)
		{
			// Thử các claim types phổ biến
			var possibleClaims = new[]
			{
				ClaimTypes.NameIdentifier,
				"sub",
				"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
				"http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier",
				"userId",
				"id"
			};

			foreach (var claimType in possibleClaims)
			{
				var claim = user.FindFirst(claimType);
				if (claim != null && Guid.TryParse(claim.Value, out var userId))
					return userId;
			}

			// Nếu không tìm thấy, thử tìm bất kỳ claim nào có value là Guid
			foreach (var claim in user.Claims)
			{
				if (Guid.TryParse(claim.Value, out var userId))
					return userId;
			}

			return null;
		}

		public static string GetUserEmail(this ClaimsPrincipal user)
		{
			return user.FindFirst(ClaimTypes.Email)?.Value
				?? user.FindFirst("email")?.Value
				?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
		}

		public static string GetUserRole(this ClaimsPrincipal user)
		{
			return user.FindFirst(ClaimTypes.Role)?.Value
				?? user.FindFirst("role")?.Value
				?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
		}
	}
}