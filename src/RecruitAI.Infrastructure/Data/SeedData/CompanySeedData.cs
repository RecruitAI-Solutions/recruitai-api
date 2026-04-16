using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Data.SeedData;

public static class CompanySeedData
{
	public static List<Company> GetCompanies(List<User> recruiters)
	{
		var companies = new List<Company>();
		var now = DateTime.UtcNow;

		if (recruiters.Count >= 3)
		{
			companies.Add(new Company
			{
				Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Name = "Tech Solutions Vietnam",
				Slug = "tech-solutions-vietnam",
				Address = "Tòa nhà ABC, Quận 1, TP.HCM",
				Website = "https://techsolutions.vn",
				CreatedAt = now,
				CreatedBy = recruiters[0].Id
			});

			companies.Add(new Company
			{
				Id = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
				Name = "Data Solutions",
				Slug = "data-solutions",
				Address = "Tầng 10, Tòa nhà XYZ, Quận 3, TP.HCM",
				Website = "https://datasolutions.vn",
				CreatedAt = now,
				CreatedBy = recruiters[1].Id
			});

			companies.Add(new Company
			{
				Id = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
				Name = "AI Startup",
				Slug = "ai-startup",
				Address = "Tầng 5, Tòa nhà DEF, Quận 7, TP.HCM",
				Website = "https://aistartup.vn",
				CreatedAt = now,
				CreatedBy = recruiters[2].Id
			});
		}

		return companies;
	}
}