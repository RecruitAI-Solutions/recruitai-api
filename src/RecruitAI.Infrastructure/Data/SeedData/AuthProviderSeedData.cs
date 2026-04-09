using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
using BCrypt.Net;

namespace RecruitAI.Infrastructure.Data.SeedData
{
	public static class AuthProviderSeedData
	{
		public static List<AuthProvider> GetAuthProviders(List<User> users)
		{
			var authProviders = new List<AuthProvider>();
			var now = DateTime.UtcNow;

			foreach (var user in users)
			{
				string passwordHash;

				// Tạo hash động dựa trên role
				if (user.Role == UserRole.ADMIN)
				{
					passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
				}
				else if (user.Role == UserRole.RECRUITER)
				{
					passwordHash = BCrypt.Net.BCrypt.HashPassword("Recruiter@123");
				}
				else if (user.Role == UserRole.CANDIDATE)
				{
					passwordHash = BCrypt.Net.BCrypt.HashPassword("Candidate@123");
				}
				else
				{
					continue; // Bỏ qua nếu role không xác định
				}

				var authProvider = new AuthProvider
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					Provider = AuthProviderType.Email,
					ProviderUserId = user.Email,
					ProviderEmail = user.Email,
					PasswordHash = passwordHash,
					CreatedAt = now,
					LastLoginAt = null
				};

				authProviders.Add(authProvider);
			}

			return authProviders;
		}
	}
}