using RecruitAI.Domain.Entities;
using System;
using System.Collections.Generic;

namespace RecruitAI.Infrastructure.Data.SeedData
{
	public static class JobSkillSeedData
	{
		public static List<JobSkill> GetJobSkills(List<Job> jobs, List<Skill> skills)
		{
			var jobSkills = new List<JobSkill>();
			var skillDict = new Dictionary<string, int>();

			foreach (var skill in skills)
			{
				skillDict[skill.Name] = skill.Id;
			}

			// Job 1: .NET Backend Developer
			var job1Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
			AddJobSkill(jobSkills, job1Id, "C#", skillDict);
			AddJobSkill(jobSkills, job1Id, ".NET Core", skillDict);
			AddJobSkill(jobSkills, job1Id, "ASP.NET Core", skillDict);
			AddJobSkill(jobSkills, job1Id, "Entity Framework", skillDict);
			AddJobSkill(jobSkills, job1Id, "SQL Server", skillDict);
			AddJobSkill(jobSkills, job1Id, "Redis", skillDict);
			AddJobSkill(jobSkills, job1Id, "REST API", skillDict);
			AddJobSkill(jobSkills, job1Id, "JWT", skillDict);

			// Job 2: Frontend React Developer
			var job2Id = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
			AddJobSkill(jobSkills, job2Id, "React", skillDict);
			AddJobSkill(jobSkills, job2Id, "JavaScript", skillDict);
			AddJobSkill(jobSkills, job2Id, "TypeScript", skillDict);
			AddJobSkill(jobSkills, job2Id, "HTML5", skillDict);
			AddJobSkill(jobSkills, job2Id, "CSS3", skillDict);
			AddJobSkill(jobSkills, job2Id, "Tailwind CSS", skillDict);
			AddJobSkill(jobSkills, job2Id, "Redux", skillDict);
			AddJobSkill(jobSkills, job2Id, "REST API", skillDict);

			// Job 3: AI/ML Engineer
			var job3Id = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC");
			AddJobSkill(jobSkills, job3Id, "Python", skillDict);
			AddJobSkill(jobSkills, job3Id, "TensorFlow", skillDict);
			AddJobSkill(jobSkills, job3Id, "Machine Learning", skillDict);
			AddJobSkill(jobSkills, job3Id, "Deep Learning", skillDict);
			AddJobSkill(jobSkills, job3Id, "NLP", skillDict);
			AddJobSkill(jobSkills, job3Id, "Pandas", skillDict);
			AddJobSkill(jobSkills, job3Id, "SQL", skillDict);

			// Job 4: DevOps Engineer
			var job4Id = Guid.Parse("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD");
			AddJobSkill(jobSkills, job4Id, "Docker", skillDict);
			AddJobSkill(jobSkills, job4Id, "Kubernetes", skillDict);
			AddJobSkill(jobSkills, job4Id, "Jenkins", skillDict);
			AddJobSkill(jobSkills, job4Id, "AWS", skillDict);
			AddJobSkill(jobSkills, job4Id, "Git", skillDict);
			AddJobSkill(jobSkills, job4Id, "Terraform", skillDict);
			AddJobSkill(jobSkills, job4Id, "Ansible", skillDict);

			// Job 5: Data Engineer
			var job5Id = Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE");
			AddJobSkill(jobSkills, job5Id, "SQL", skillDict);
			AddJobSkill(jobSkills, job5Id, "Python", skillDict);
			AddJobSkill(jobSkills, job5Id, "ETL", skillDict);
			AddJobSkill(jobSkills, job5Id, "Spark", skillDict);
			AddJobSkill(jobSkills, job5Id, "Power BI", skillDict);
			AddJobSkill(jobSkills, job5Id, "Pandas", skillDict);

			// Job 6: Full Stack Developer
			var job6Id = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
			AddJobSkill(jobSkills, job6Id, "React", skillDict);
			AddJobSkill(jobSkills, job6Id, "Node.js", skillDict);
			AddJobSkill(jobSkills, job6Id, "JavaScript", skillDict);
			AddJobSkill(jobSkills, job6Id, "MongoDB", skillDict);
			AddJobSkill(jobSkills, job6Id, "Express.js", skillDict);
			AddJobSkill(jobSkills, job6Id, "REST API", skillDict);

			// Job 7: Java Backend Developer
			var job7Id = Guid.Parse("99999999-9999-9999-9999-999999999999");
			AddJobSkill(jobSkills, job7Id, "Java", skillDict);
			AddJobSkill(jobSkills, job7Id, "Spring Boot", skillDict);
			AddJobSkill(jobSkills, job7Id, "Hibernate", skillDict);
			AddJobSkill(jobSkills, job7Id, "MySQL", skillDict);
			AddJobSkill(jobSkills, job7Id, "MongoDB", skillDict);
			AddJobSkill(jobSkills, job7Id, "Microservices", skillDict);

			// Job 8: Mobile Developer (React Native)
			var job8Id = Guid.Parse("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");
			AddJobSkill(jobSkills, job8Id, "React Native", skillDict);
			AddJobSkill(jobSkills, job8Id, "JavaScript", skillDict);
			AddJobSkill(jobSkills, job8Id, "TypeScript", skillDict);
			AddJobSkill(jobSkills, job8Id, "Redux", skillDict);
			AddJobSkill(jobSkills, job8Id, "REST API", skillDict);

			// Job 9: Cloud Engineer (AWS)
			var job9Id = Guid.Parse("BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF");
			AddJobSkill(jobSkills, job9Id, "AWS", skillDict);
			AddJobSkill(jobSkills, job9Id, "Docker", skillDict);
			AddJobSkill(jobSkills, job9Id, "Kubernetes", skillDict);
			AddJobSkill(jobSkills, job9Id, "Terraform", skillDict);
			AddJobSkill(jobSkills, job9Id, "Jenkins", skillDict);
			AddJobSkill(jobSkills, job9Id, "Linux", skillDict);

			// Job 10: UI/UX Designer
			var job10Id = Guid.Parse("CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA");
			AddJobSkill(jobSkills, job10Id, "Figma", skillDict);
			AddJobSkill(jobSkills, job10Id, "Adobe XD", skillDict);
			AddJobSkill(jobSkills, job10Id, "User Research", skillDict);
			AddJobSkill(jobSkills, job10Id, "Prototyping", skillDict);
			AddJobSkill(jobSkills, job10Id, "Design Systems", skillDict);

			return jobSkills;
		}

		private static void AddJobSkill(List<JobSkill> jobSkills, Guid jobId, string skillName, Dictionary<string, int> skillDict)
		{
			if (skillDict.ContainsKey(skillName))
			{
				jobSkills.Add(new JobSkill
				{
					JobId = jobId,
					SkillId = skillDict[skillName],
					IsRequired = true
				});
			}
		}
	}
}