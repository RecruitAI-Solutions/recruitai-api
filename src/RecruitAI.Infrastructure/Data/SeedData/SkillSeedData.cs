using RecruitAI.Domain.Entities;
using System;
using System.Collections.Generic;

namespace RecruitAI.Infrastructure.Data.SeedData
{
	public static class SkillSeedData
	{
		public static List<Skill> GetSkills()
		{
			var skills = new List<Skill>();

			var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var systemUser = "system";
			var id = 1;

			// Programming Languages
			skills.AddRange(new[]
			{
				new Skill { Id = id++, Name = "C#", Category = "Programming Language", Aliases = "CSharp,C Sharp", ContextKeywords = "c#,csharp,c sharp", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Java", Category = "Programming Language", Aliases = null, ContextKeywords = "java,java 8,java 11", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Python", Category = "Programming Language", Aliases = "py", ContextKeywords = "python,py,django", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "JavaScript", Category = "Programming Language", Aliases = "js", ContextKeywords = "javascript,js,ecmascript", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "TypeScript", Category = "Programming Language", Aliases = "ts", ContextKeywords = "typescript,ts", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "SQL", Category = "Programming Language", Aliases = "structured query language", ContextKeywords = "sql,tsql,plsql", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Go", Category = "Programming Language", Aliases = "golang", ContextKeywords = "go,golang", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Rust", Category = "Programming Language", Aliases = null, ContextKeywords = "rust", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "PHP", Category = "Programming Language", Aliases = null, ContextKeywords = "php,laravel", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Swift", Category = "Programming Language", Aliases = null, ContextKeywords = "swift,ios", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Kotlin", Category = "Programming Language", Aliases = null, ContextKeywords = "kotlin,android", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Ruby", Category = "Programming Language", Aliases = null, ContextKeywords = "ruby,rails", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// Frameworks & Libraries
			skills.AddRange(new[]
			{
				new Skill { Id = id++, Name = ".NET Core", Category = "Framework", Aliases = "dotnet core", ContextKeywords = ".net core,asp.net core", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "React", Category = "Framework", Aliases = "reactjs", ContextKeywords = "react,reactjs,react.js", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Angular", Category = "Framework", Aliases = "angularjs", ContextKeywords = "angular,angular 2+", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Vue.js", Category = "Framework", Aliases = "vue", ContextKeywords = "vue,vuejs", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Spring Boot", Category = "Framework", Aliases = "spring", ContextKeywords = "spring,spring boot", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Node.js", Category = "Framework", Aliases = "node", ContextKeywords = "node,nodejs", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Django", Category = "Framework", Aliases = null, ContextKeywords = "django,python web", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Flask", Category = "Framework", Aliases = null, ContextKeywords = "flask", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Express.js", Category = "Framework", Aliases = "express", ContextKeywords = "express,expressjs", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Entity Framework", Category = "Framework", Aliases = "ef", ContextKeywords = "entity framework,ef core", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Hibernate", Category = "Framework", Aliases = null, ContextKeywords = "hibernate,jpa", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// Databases
			skills.AddRange(new[]
			{
				new Skill { Id = id++, Name = "SQL Server", Category = "Database", Aliases = "mssql", ContextKeywords = "sql server,mssql", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "MySQL", Category = "Database", Aliases = null, ContextKeywords = "mysql", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "PostgreSQL", Category = "Database", Aliases = "postgres", ContextKeywords = "postgresql,postgres", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "MongoDB", Category = "Database", Aliases = "mongo", ContextKeywords = "mongodb,mongo", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Redis", Category = "Database", Aliases = null, ContextKeywords = "redis,cache", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Elasticsearch", Category = "Database", Aliases = "es", ContextKeywords = "elasticsearch,es", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Oracle", Category = "Database", Aliases = null, ContextKeywords = "oracle", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Cassandra", Category = "Database", Aliases = null, ContextKeywords = "cassandra", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "DynamoDB", Category = "Database", Aliases = "dynamo", ContextKeywords = "dynamodb,aws dynamo", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// Cloud & DevOps
			skills.AddRange(new[]
			{
				new Skill { Id = id++, Name = "Azure", Category = "Cloud", Aliases = "microsoft azure", ContextKeywords = "azure,azure devops", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "AWS", Category = "Cloud", Aliases = "amazon web services", ContextKeywords = "aws,ec2,s3,lambda", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Google Cloud", Category = "Cloud", Aliases = "gcp", ContextKeywords = "gcp,google cloud", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Docker", Category = "DevOps", Aliases = null, ContextKeywords = "docker,container", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Kubernetes", Category = "DevOps", Aliases = "k8s", ContextKeywords = "kubernetes,k8s", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Jenkins", Category = "DevOps", Aliases = null, ContextKeywords = "jenkins,ci/cd", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Git", Category = "DevOps", Aliases = null, ContextKeywords = "git,github,gitlab", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Terraform", Category = "DevOps", Aliases = null, ContextKeywords = "terraform,iac", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Ansible", Category = "DevOps", Aliases = null, ContextKeywords = "ansible", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// Soft Skills
			skills.AddRange(new[]
			{
				new Skill { Id = id++, Name = "Teamwork", Category = "Soft Skill", Aliases = "collaboration", ContextKeywords = "teamwork,team work", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Communication", Category = "Soft Skill", Aliases = null, ContextKeywords = "communication,verbal,written", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Problem Solving", Category = "Soft Skill", Aliases = "analytical", ContextKeywords = "problem solving,analytical", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Leadership", Category = "Soft Skill", Aliases = null, ContextKeywords = "leadership,lead", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Time Management", Category = "Soft Skill", Aliases = null, ContextKeywords = "time management,organize", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// Languages
			skills.AddRange(new[]
			{
				new Skill { Id = id++, Name = "English", Category = "Language", Aliases = null, ContextKeywords = "english,ielts,toeic", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Vietnamese", Category = "Language", Aliases = "tiếng việt", ContextKeywords = "vietnamese,tieng viet", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Japanese", Category = "Language", Aliases = "日本語", ContextKeywords = "japanese,nihongo,jlpt", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Chinese", Category = "Language", Aliases = "mandarin", ContextKeywords = "chinese,mandarin", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Id = id++, Name = "Korean", Category = "Language", Aliases = "한국어", ContextKeywords = "korean,topik", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			return skills;
		}
	}
}