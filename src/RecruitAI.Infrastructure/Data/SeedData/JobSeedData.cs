using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RecruitAI.Infrastructure.Data.SeedData
{
	public static class JobSeedData
	{
		public static List<Job> GetJobs(List<User> recruiters)
		{
			var jobs = new List<Job>();
			var now = DateTime.UtcNow;
			var recruiterIds = new List<Guid>();

			foreach (var recruiter in recruiters)
			{
				if (recruiter.Role == UserRole.RECRUITER)
				{
					recruiterIds.Add(recruiter.Id);
				}
			}

			if (recruiterIds.Count == 0) return jobs;

			var jobId = 1;

			// Job 1: .NET Backend Developer
			jobs.Add(new Job
			{
				Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Title = ".NET Backend Developer",
				Description = "We are looking for a skilled .NET Backend Developer to join our engineering team. You will be responsible for building RESTful APIs, optimizing database performance, and implementing authentication systems.",
				Requirements = "Strong experience with C#, ASP.NET Core, Entity Framework, SQL Server. Knowledge of Redis and microservices is a plus.",
				Location = "Ho Chi Minh City",
				SalaryMin = 15000000,
				SalaryMax = 25000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Engineering",
				Benefits = "13th month salary, Health insurance, 12 days annual leave, Hybrid working",
				ExpirationDate = now.AddMonths(3),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 150,
				Applications = 0,
				RecruiterId = recruiterIds[0]
			});

			// Job 2: Frontend React Developer
			jobs.Add(new Job
			{
				Id = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
				Title = "Frontend React Developer",
				Description = "Join our frontend team to build responsive and high-performance user interfaces using ReactJS and modern web technologies.",
				Requirements = "Proficient in ReactJS, JavaScript, TypeScript, HTML5, CSS3. Experience with Tailwind CSS and state management (Redux).",
				Location = "Da Nang",
				SalaryMin = 12000000,
				SalaryMax = 20000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Engineering",
				Benefits = "13th month salary, Health insurance, Remote work option, Learning budget",
				ExpirationDate = now.AddMonths(3),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 120,
				Applications = 0,
				RecruiterId = recruiterIds[0]
			});

			// Job 3: AI/ML Engineer
			jobs.Add(new Job
			{
				Id = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
				Title = "AI/ML Engineer",
				Description = "Develop and deploy machine learning models for candidate screening, NLP applications, and data-driven solutions.",
				Requirements = "Strong Python skills, experience with TensorFlow, Scikit-learn, Pandas. Knowledge of NLP and model deployment.",
				Location = "Hanoi",
				SalaryMin = 20000000,
				SalaryMax = 35000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Senior,
				Department = "AI Research",
				Benefits = "13th month salary, Stock options, Premium healthcare, 15 days annual leave",
				ExpirationDate = now.AddMonths(3),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 200,
				Applications = 0,
				RecruiterId = recruiterIds[1]
			});

			// Job 4: DevOps Engineer
			jobs.Add(new Job
			{
				Id = Guid.Parse("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
				Title = "DevOps Engineer",
				Description = "Design and maintain CI/CD pipelines, manage cloud infrastructure on AWS, and automate deployment processes.",
				Requirements = "Experience with Docker, Kubernetes, Jenkins, GitHub Actions, AWS services. Knowledge of infrastructure as code.",
				Location = "Ho Chi Minh City",
				SalaryMin = 18000000,
				SalaryMax = 28000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Platform Engineering",
				Benefits = "13th month salary, AWS certification support, 14 days annual leave, Gym membership",
				ExpirationDate = now.AddMonths(3),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 100,
				Applications = 0,
				RecruiterId = recruiterIds[1]
			});

			// Job 5: Data Engineer
			jobs.Add(new Job
			{
				Id = Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE"),
				Title = "Data Engineer",
				Description = "Build and maintain ETL pipelines, process large datasets, and create data visualizations for business intelligence.",
				Requirements = "Strong SQL and Python skills. Experience with Spark, Power BI, and ETL processes. Data warehousing knowledge.",
				Location = "Da Nang",
				SalaryMin = 16000000,
				SalaryMax = 26000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Data",
				Benefits = "13th month salary, Health insurance, Flexible working hours, Training courses",
				ExpirationDate = now.AddMonths(3),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 80,
				Applications = 0,
				RecruiterId = recruiterIds[2]
			});

			// Job 6: Full Stack Developer
			jobs.Add(new Job
			{
				Id = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
				Title = "Full Stack Developer",
				Description = "Looking for a versatile Full Stack Developer who can work on both frontend and backend systems.",
				Requirements = "Experience with React, Node.js, MongoDB, RESTful APIs. Understanding of the full development lifecycle.",
				Location = "Ho Chi Minh City",
				SalaryMin = 18000000,
				SalaryMax = 30000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Engineering",
				Benefits = "13th month salary, Health insurance, Flexible working hours",
				ExpirationDate = now.AddMonths(2),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 95,
				Applications = 0,
				RecruiterId = recruiterIds[0]
			});

			// Job 7: Java Backend Developer
			jobs.Add(new Job
			{
				Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
				Title = "Java Backend Developer",
				Description = "Seeking experienced Java developer to build scalable backend services.",
				Requirements = "Strong Java, Spring Boot, Hibernate, MySQL/MongoDB. Microservices experience is a plus.",
				Location = "Hanoi",
				SalaryMin = 16000000,
				SalaryMax = 27000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Engineering",
				Benefits = "13th month salary, Health insurance, Laptop provided",
				ExpirationDate = now.AddMonths(2),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 70,
				Applications = 0,
				RecruiterId = recruiterIds[1]
			});

			// Job 8: Mobile Developer (React Native)
			jobs.Add(new Job
			{
				Id = Guid.Parse("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"),
				Title = "Mobile Developer (React Native)",
				Description = "Join our mobile team to build cross-platform mobile applications using React Native.",
				Requirements = "Experience with React Native, JavaScript/TypeScript, mobile app deployment for iOS and Android.",
				Location = "Da Nang",
				SalaryMin = 14000000,
				SalaryMax = 24000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Mobile",
				Benefits = "13th month salary, Health insurance, Flexible working hours",
				ExpirationDate = now.AddMonths(2),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 60,
				Applications = 0,
				RecruiterId = recruiterIds[2]
			});

			// Job 9: Cloud Engineer (AWS)
			jobs.Add(new Job
			{
				Id = Guid.Parse("BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF"),
				Title = "Cloud Engineer (AWS)",
				Description = "Manage and optimize cloud infrastructure on AWS platform.",
				Requirements = "AWS certifications, experience with EC2, S3, Lambda, CloudFormation. Knowledge of DevOps practices.",
				Location = "Ho Chi Minh City",
				SalaryMin = 20000000,
				SalaryMax = 35000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Senior,
				Department = "Cloud",
				Benefits = "13th month salary, AWS certification bonus, Premium insurance",
				ExpirationDate = now.AddMonths(3),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 55,
				Applications = 0,
				RecruiterId = recruiterIds[0]
			});

			// Job 10: UI/UX Designer
			jobs.Add(new Job
			{
				Id = Guid.Parse("CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA"),
				Title = "UI/UX Designer",
				Description = "Design beautiful and user-friendly interfaces for web and mobile applications.",
				Requirements = "Figma, Adobe XD, user research, prototyping. Understanding of design systems.",
				Location = "Hanoi",
				SalaryMin = 12000000,
				SalaryMax = 22000000,
				Currency = Currency.VND,
				EmploymentType = EmploymentType.FullTime,
				ExperienceLevel = ExperienceLevel.Intermediate,
				Department = "Design",
				Benefits = "13th month salary, Health insurance, Creative environment",
				ExpirationDate = now.AddMonths(2),
				CreatedAt = now,
				Status = JobStatus.Published,
				IsActive = true,
				IsDeleted = false,
				Views = 45,
				Applications = 0,
				RecruiterId = recruiterIds[1]
			});

			return jobs;
		}
	}
}