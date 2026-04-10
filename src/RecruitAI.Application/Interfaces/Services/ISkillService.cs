using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Interfaces.Services;

public interface ISkillService
{
	Task<Skill> CreateOrGetSkillAsync(string skillName);
	Task<List<Skill>> CreateOrGetSkillsAsync(List<string> skillNames);
	string DetectCategory(string skillName);
}