using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecruitAI.Domain.Entities;
using RecruitAI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecruitAI.Infrastructure.Data.SeedData
{
	public static class DatabaseSeeder
	{
		public static async Task SeedAsync(RecruitDevContext context, ILogger logger)
		{
			try
			{
				logger.LogInformation("========== SEEDING STARTED ==========");
				if (!context.Skills.Any())
				{
					try
					{
						logger.LogInformation("Seeding Skills...");
						var skills = SkillSeedData.GetSkills();
						logger.LogInformation("Got {Count} skills to seed", skills.Count);

						await context.Skills.AddRangeAsync(skills);
						logger.LogInformation("Added skills to context, saving changes...");

						await context.SaveChangesAsync();
						logger.LogInformation("Seeded {Count} skills successfully", skills.Count);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Failed to seed Skills");
						throw;
					}
				}

				// 2. Seed Users
				if (!context.Users.Any())
				{
					logger.LogInformation("Seeding Users...");
					var users = UserSeedData.GetUsers();
					await context.Users.AddRangeAsync(users);
					await context.SaveChangesAsync();
					logger.LogInformation("Seeded {Count} users", users.Count);
				}

				// 3. Seed AuthProviders
				if (!context.AuthProviders.Any())
				{
					logger.LogInformation("Seeding AuthProviders...");
					var users = context.Users.ToList();
					var authProviders = AuthProviderSeedData.GetAuthProviders(users);
					await context.AuthProviders.AddRangeAsync(authProviders);
					await context.SaveChangesAsync();
					logger.LogInformation("Seeded {Count} auth providers", authProviders.Count);
				}

				// 4. Seed Jobs
				if (!context.Jobs.Any())
				{
					logger.LogInformation("Seeding Jobs...");
					var recruiters = context.Users.Where(u => u.Role == Domain.Enums.UserRole.RECRUITER).ToList();
					var jobs = JobSeedData.GetJobs(recruiters);
					await context.Jobs.AddRangeAsync(jobs);
					await context.SaveChangesAsync();
					logger.LogInformation("Seeded {Count} jobs", jobs.Count);
				}

				// 5. Seed JobSkills
				if (!context.JobSkills.Any())
				{
					logger.LogInformation("Seeding JobSkills...");
					var jobs = context.Jobs.ToList();
					var skills = context.Skills.ToList();
					var jobSkills = JobSkillSeedData.GetJobSkills(jobs, skills);
					await context.JobSkills.AddRangeAsync(jobSkills);
					await context.SaveChangesAsync();
					logger.LogInformation("Seeded {Count} job skills", jobSkills.Count);
				}

				logger.LogInformation("Database seeding completed successfully!");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred while seeding the database");
				throw;
			}
		}
	}
}