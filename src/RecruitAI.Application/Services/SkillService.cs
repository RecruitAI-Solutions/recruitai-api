using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Services;

public class SkillService : ISkillService
{
	private readonly IUnitOfWork _uow;
	private readonly ILogger<SkillService> _logger;

	public SkillService(IUnitOfWork uow, ILogger<SkillService> logger)
	{
		_uow = uow;
		_logger = logger;
	}

	public async Task<Skill> CreateOrGetSkillAsync(string skillName)
	{
		var trimmedName = skillName.Trim();

		// Tìm skill theo tên (không phân biệt hoa thường)
		var existingSkill = await _uow.Skills
		.FirstOrDefaultAsync(s => s.Name.ToLower() == trimmedName.ToLower());

		if (existingSkill != null)
		{
			return existingSkill;
		}

		// Tạo skill mới
		var newSkill = new Skill
		{
			Name = trimmedName,
			Category = DetectCategory(trimmedName),
			IsActive = true,
			CreatedAt = DateTime.UtcNow
		};

		await _uow.Skills.AddAsync(newSkill);
		await _uow.SaveChangesAsync();

		_logger.LogInformation("Created new skill: {SkillName} (Category: {Category})",
			trimmedName, newSkill.Category);

		return newSkill;
	}

	public async Task<List<Skill>> CreateOrGetSkillsAsync(List<string> skillNames)
	{
		var skills = new List<Skill>();
		foreach (var name in skillNames)
		{
			var skill = await CreateOrGetSkillAsync(name);
			skills.Add(skill);
		}
		return skills;
	}

	public string DetectCategory(string skillName)
	{
		var lowerName = skillName.ToLower();

		// Programming Languages
		if (new[] { "java", "python", "c#", "csharp", "javascript", "typescript", "go", "rust", "php", "swift", "kotlin", "c++", "c" }
			.Any(lang => lowerName.Contains(lang)))
			return "Programming Language";

		// Frameworks
		if (new[] { "spring", "react", "angular", "vue", "django", "flask", "laravel", "asp.net", "rails", "hibernate", "struts" }
			.Any(fw => lowerName.Contains(fw)))
			return "Framework";

		// Databases
		if (new[] { "sql", "mysql", "postgresql", "mongodb", "redis", "oracle", "sqlite", "cassandra", "dynamodb" }
			.Any(db => lowerName.Contains(db)))
			return "Database";

		// Design Tools
		if (new[] { "figma", "adobe xd", "sketch", "photoshop", "illustrator", "invision", "zeplin" }
			.Any(design => lowerName.Contains(design)))
			return "Design Tool";

		// UX/UI
		if (new[] { "user research", "prototyping", "wireframing", "usability", "ux", "ui", "design system" }
			.Any(ux => lowerName.Contains(ux)))
			return "UX/UI";

		// Cloud & DevOps
		if (new[] { "aws", "azure", "gcp", "docker", "kubernetes", "terraform", "jenkins", "gitlab", "github actions" }
			.Any(cloud => lowerName.Contains(cloud)))
			return "Cloud/DevOps";

		// Mobile
		if (new[] { "android", "ios", "flutter", "react native", "xamarin", "swiftui", "jetpack" }
			.Any(mobile => lowerName.Contains(mobile)))
			return "Mobile Development";

		// Soft Skills
		if (new[] { "leadership", "communication", "teamwork", "problem solving", "critical thinking", "time management" }
			.Any(soft => lowerName.Contains(soft)))
			return "Soft Skill";

		return "Other";
	}

	//private async Task<int> GetNextSkillId()
	//{
	//	var maxId = await _uow.GetMaxSkillIdAsync();
	//	return maxId + 1;
	//}
}