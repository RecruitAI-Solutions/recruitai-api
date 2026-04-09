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

			// ========================================================
			// PROGRAMMING LANGUAGES (12 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "C#", Category = "Programming Language", Aliases = "CSharp,C Sharp", ContextKeywords = "c#,csharp,c sharp", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Java", Category = "Programming Language", Aliases = null, ContextKeywords = "java,java 8,java 11", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Python", Category = "Programming Language", Aliases = "py", ContextKeywords = "python,py,django", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "JavaScript", Category = "Programming Language", Aliases = "js,java script,javascript", ContextKeywords = "javascript,js,ecmascript", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "TypeScript", Category = "Programming Language", Aliases = "ts,type script,typescript", ContextKeywords = "typescript,ts", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "SQL", Category = "Programming Language", Aliases = "structured query language", ContextKeywords = "sql,tsql,plsql", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Go", Category = "Programming Language", Aliases = "golang", ContextKeywords = "go,golang", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Rust", Category = "Programming Language", Aliases = null, ContextKeywords = "rust", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "PHP", Category = "Programming Language", Aliases = null, ContextKeywords = "php,laravel", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Swift", Category = "Programming Language", Aliases = null, ContextKeywords = "swift,ios", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Kotlin", Category = "Programming Language", Aliases = null, ContextKeywords = "kotlin,android", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Ruby", Category = "Programming Language", Aliases = null, ContextKeywords = "ruby,rails", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// FRAMEWORKS & LIBRARIES (15 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = ".NET Core", Category = "Framework", Aliases = "dotnet core", ContextKeywords = ".net core,asp.net core", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "ASP.NET Core", Category = "Framework", Aliases = "asp.net core,aspnetcore", ContextKeywords = "asp.net core,aspnet core web api", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Entity Framework", Category = "Framework", Aliases = "ef,ef core", ContextKeywords = "entity framework,ef core", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Web API", Category = "Framework", Aliases = "webapi,rest api", ContextKeywords = "web api,restful api", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "REST API", Category = "Framework", Aliases = "restful api", ContextKeywords = "rest api,restful web service", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "React", Category = "Framework", Aliases = "reactjs", ContextKeywords = "react,reactjs,react.js", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Angular", Category = "Framework", Aliases = "angularjs", ContextKeywords = "angular,angular 2+", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Vue.js", Category = "Framework", Aliases = "vue", ContextKeywords = "vue,vuejs", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Spring Boot", Category = "Framework", Aliases = "spring", ContextKeywords = "spring,spring boot", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Node.js", Category = "Framework", Aliases = "node", ContextKeywords = "node,nodejs", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Django", Category = "Framework", Aliases = null, ContextKeywords = "django,python web", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Flask", Category = "Framework", Aliases = null, ContextKeywords = "flask", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Express.js", Category = "Framework", Aliases = "express", ContextKeywords = "express,expressjs", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Hibernate", Category = "Framework", Aliases = null, ContextKeywords = "hibernate,jpa", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// DATABASES (9 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "SQL Server", Category = "Database", Aliases = "mssql", ContextKeywords = "sql server,mssql", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "MySQL", Category = "Database", Aliases = null, ContextKeywords = "mysql", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "PostgreSQL", Category = "Database", Aliases = "postgres", ContextKeywords = "postgresql,postgres", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "MongoDB", Category = "Database", Aliases = "mongo", ContextKeywords = "mongodb,mongo", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Redis", Category = "Database", Aliases = null, ContextKeywords = "redis,cache", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Elasticsearch", Category = "Database", Aliases = "es", ContextKeywords = "elasticsearch,es", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Oracle", Category = "Database", Aliases = null, ContextKeywords = "oracle", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Cassandra", Category = "Database", Aliases = null, ContextKeywords = "cassandra", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "DynamoDB", Category = "Database", Aliases = "dynamo", ContextKeywords = "dynamodb,aws dynamo", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// CLOUD & DEVOPS (9 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "Azure", Category = "Cloud", Aliases = "microsoft azure", ContextKeywords = "azure,azure devops", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "AWS", Category = "Cloud", Aliases = "amazon web services", ContextKeywords = "aws,ec2,s3,lambda", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Google Cloud", Category = "Cloud", Aliases = "gcp", ContextKeywords = "gcp,google cloud", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Docker", Category = "DevOps", Aliases = null, ContextKeywords = "docker,container", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Kubernetes", Category = "DevOps", Aliases = "k8s", ContextKeywords = "kubernetes,k8s", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Jenkins", Category = "DevOps", Aliases = null, ContextKeywords = "jenkins,ci/cd", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Git", Category = "DevOps", Aliases = null, ContextKeywords = "git,github,gitlab", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Terraform", Category = "DevOps", Aliases = null, ContextKeywords = "terraform,iac", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Ansible", Category = "DevOps", Aliases = null, ContextKeywords = "ansible", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// SOFT SKILLS (5 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "Teamwork", Category = "Soft Skill", Aliases = "collaboration", ContextKeywords = "teamwork,team work", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Communication", Category = "Soft Skill", Aliases = null, ContextKeywords = "communication,verbal,written", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Problem Solving", Category = "Soft Skill", Aliases = "analytical", ContextKeywords = "problem solving,analytical", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Leadership", Category = "Soft Skill", Aliases = null, ContextKeywords = "leadership,lead", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Time Management", Category = "Soft Skill", Aliases = null, ContextKeywords = "time management,organize", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// LANGUAGES (5 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "English", Category = "Language", Aliases = null, ContextKeywords = "english,ielts,toeic", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Vietnamese", Category = "Language", Aliases = "tiếng việt", ContextKeywords = "vietnamese,tieng viet", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Japanese", Category = "Language", Aliases = "日本語", ContextKeywords = "japanese,nihongo,jlpt", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Chinese", Category = "Language", Aliases = "mandarin", ContextKeywords = "chinese,mandarin", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Korean", Category = "Language", Aliases = "한국어", ContextKeywords = "korean,topik", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// SECURITY (2 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "JWT", Category = "Security", Aliases = "json web token", ContextKeywords = "jwt authentication,jwt token", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "OAuth2", Category = "Security", Aliases = "oauth 2.0", ContextKeywords = "oauth2,oauth 2.0", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// ARCHITECTURE & DESIGN (4 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "Microservices", Category = "Architecture", Aliases = "microservice", ContextKeywords = "microservices architecture,msa", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Clean Architecture", Category = "Architecture", Aliases = "onion architecture", ContextKeywords = "clean architecture,onion architecture", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "CQRS", Category = "Architecture", Aliases = "cqrs pattern", ContextKeywords = "cqrs,command query responsibility segregation", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Event Sourcing", Category = "Architecture", Aliases = "event sourcing pattern", ContextKeywords = "event sourcing,event driven", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// TESTING (4 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "Unit Testing", Category = "Testing", Aliases = "unit test", ContextKeywords = "unit testing,xunit,nunit", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Integration Testing", Category = "Testing", Aliases = "integration test", ContextKeywords = "integration testing", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "xUnit", Category = "Testing", Aliases = "xunit.net", ContextKeywords = "xunit,unit testing", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Moq", Category = "Testing", Aliases = "mock", ContextKeywords = "moq,mocking", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// FRONTEND (9 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "HTML5", Category = "Frontend", Aliases = "html", ContextKeywords = "html5,html", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "CSS3", Category = "Frontend", Aliases = "css", ContextKeywords = "css3,css", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Bootstrap", Category = "Frontend", Aliases = null, ContextKeywords = "bootstrap,css framework", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Tailwind CSS", Category = "Frontend", Aliases = "tailwind", ContextKeywords = "tailwind,tailwindcss", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "jQuery", Category = "Frontend", Aliases = "jquery", ContextKeywords = "jquery,javascript library", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "HTML", Category = "Frontend",
					Aliases = "html5",
					ContextKeywords = "html,html5",
					IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },

				new Skill { Name = "CSS", Category = "Frontend",
					Aliases = "css3",
					ContextKeywords = "css,css3",
					IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },

				new Skill { Name = "Tailwind", Category = "Frontend",
					Aliases = "tailwind css",
					ContextKeywords = "tailwind,tailwindcss",
					IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },

				new Skill { Name = "Redux", Category = "Frontend",
					Aliases = "reduxjs",
					ContextKeywords = "redux,reduxjs,react redux",
					IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }

			});

			// ========================================================
			// MOBILE (4 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "React Native", Category = "Mobile", Aliases = "reactnative", ContextKeywords = "react native,mobile app", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Flutter", Category = "Mobile", Aliases = null, ContextKeywords = "flutter,dart", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Android", Category = "Mobile", Aliases = "android dev", ContextKeywords = "android,kotlin android", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "iOS", Category = "Mobile", Aliases = "iphone", ContextKeywords = "ios,swift ios", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			// ========================================================
			// MESSAGE QUEUE (4 skills)
			// ========================================================
			skills.AddRange(new[]
			{
				new Skill { Name = "RabbitMQ", Category = "Message Queue", Aliases = "rabbit mq", ContextKeywords = "rabbitmq,message queue", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Kafka", Category = "Message Queue", Aliases = "apache kafka", ContextKeywords = "kafka,event streaming", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Azure Service Bus", Category = "Message Queue", Aliases = "service bus", ContextKeywords = "azure service bus,servicebus", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser },
				new Skill { Name = "Redis Pub/Sub", Category = "Message Queue", Aliases = "redis pubsub", ContextKeywords = "redis pubsub,redis messaging", IsActive = true, CreatedAt = seedDate, CreatedBy = systemUser }
			});

			return skills;
		}
	}
}