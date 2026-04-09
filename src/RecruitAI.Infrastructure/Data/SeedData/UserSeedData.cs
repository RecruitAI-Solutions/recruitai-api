using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RecruitAI.Infrastructure.Data.SeedData
{
	public static class UserSeedData
	{
		public static List<User> GetUsers()
		{
			var users = new List<User>();
			var now = DateTime.UtcNow;

			// ========================================================
			// ADMIN USERS (2 users)
			// ========================================================
			var admin1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var admin2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

			users.Add(new User
			{
				Id = admin1Id,
				Email = "admin@recruitai.com",
				FullName = "System Administrator",
				Role = UserRole.ADMIN,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PermissionCodes = "P001,P002,P003,P004,P005,P006,P007,P008,P009,P010,P011,P012,P013,P014,P015,P016,P017,P018,P019,P020,P101,P102,P103,P104,P105,P106,P201,P202,P203,P204,P301,P302,P303,P304"
			});

			users.Add(new User
			{
				Id = admin2Id,
				Email = "superadmin@recruitai.com",
				FullName = "Super Administrator",
				Role = UserRole.ADMIN,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PermissionCodes = "P001,P002,P003,P004,P005,P006,P007,P008,P009,P010,P011,P012,P013,P014,P015,P016,P017,P018,P019,P020,P101,P102,P103,P104,P105,P106,P201,P202,P203,P204,P301,P302,P303,P304"
			});

			// ========================================================
			// RECRUITER USERS (3 users)
			// ========================================================
			var recruiter1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
			var recruiter2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
			var recruiter3Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

			var recruiterPermissions = "P001,P004,P005,P006,P007,P008,P009,P010,P011,P201,P202,P203,P204,P301,P302,P303,P304";

			users.Add(new User
			{
				Id = recruiter1Id,
				Email = "recruiter1@techcorp.com",
				FullName = "Nguyen Van A",
				Role = UserRole.RECRUITER,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PhoneNumber = "0901234001",
				Gender = Gender.Male,
				DateOfBirth = new DateTime(1990, 5, 15),
				PermissionCodes = recruiterPermissions
			});

			users.Add(new User
			{
				Id = recruiter2Id,
				Email = "recruiter2@datasolution.com",
				FullName = "Tran Thi B",
				Role = UserRole.RECRUITER,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PhoneNumber = "0901234002",
				Gender = Gender.Female,
				DateOfBirth = new DateTime(1992, 8, 20),
				PermissionCodes = recruiterPermissions
			});

			users.Add(new User
			{
				Id = recruiter3Id,
				Email = "recruiter3@aistartup.com",
				FullName = "Le Van C",
				Role = UserRole.RECRUITER,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PhoneNumber = "0901234003",
				Gender = Gender.Male,
				DateOfBirth = new DateTime(1988, 3, 10),
				PermissionCodes = recruiterPermissions
			});

			// ========================================================
			// CANDIDATE USERS (3 users)
			// ========================================================
			var candidate1Id = Guid.Parse("66666666-6666-6666-6666-666666666666");
			var candidate2Id = Guid.Parse("77777777-7777-7777-7777-777777777777");
			var candidate3Id = Guid.Parse("88888888-8888-8888-8888-888888888888");

			var candidatePermissions = "P001,P002,P003,P004,P005,P009,P101,P102,P103,P104,P105,P106,P301,P302,P303,P304";

			users.Add(new User
			{
				Id = candidate1Id,
				Email = "candidate1@gmail.com",
				FullName = "Pham Van D",
				Role = UserRole.CANDIDATE,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PhoneNumber = "0901234004",
				Gender = Gender.Male,
				DateOfBirth = new DateTime(1995, 10, 25),
				PermissionCodes = candidatePermissions
			});

			users.Add(new User
			{
				Id = candidate2Id,
				Email = "candidate2@gmail.com",
				FullName = "Nguyen Thi E",
				Role = UserRole.CANDIDATE,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PhoneNumber = "0901234005",
				Gender = Gender.Female,
				DateOfBirth = new DateTime(1997, 2, 14),
				PermissionCodes = candidatePermissions
			});

			users.Add(new User
			{
				Id = candidate3Id,
				Email = "candidate3@gmail.com",
				FullName = "Hoang Van F",
				Role = UserRole.CANDIDATE,
				Status = UserStatus.Active,
				EmailVerified = true,
				CreatedAt = now,
				PhoneNumber = "0901234006",
				Gender = Gender.Male,
				DateOfBirth = new DateTime(1996, 7, 30),
				PermissionCodes = candidatePermissions
			});

			return users;
		}
	}
}