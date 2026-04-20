-- ============================================
-- RECRUITAI - SEED DATA SCRIPT
-- Chạy sau khi đã tạo các bảng
-- ============================================

-- Bật IDENTITY INSERT cho bảng Skills (nếu cần)
SET IDENTITY_INSERT [Skills] ON;

-- ============================================
-- 1. SKILLS
-- ============================================
INSERT INTO [Skills] ([Id], [Name], [Category], [Aliases], [ContextKeywords], [IsActive], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy]) VALUES
-- Programming Languages (1-12)
(1, 'C#', 'Programming Language', 'CSharp,C Sharp', 'c#,csharp,c sharp', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(2, 'Java', 'Programming Language', NULL, 'java,java 8,java 11', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(3, 'Python', 'Programming Language', 'py', 'python,py,django', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(4, 'JavaScript', 'Programming Language', 'js,java script,javascript', 'javascript,js,ecmascript', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(5, 'TypeScript', 'Programming Language', 'ts,type script,typescript', 'typescript,ts', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(6, 'SQL', 'Programming Language', 'structured query language', 'sql,tsql,plsql', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(7, 'Go', 'Programming Language', 'golang', 'go,golang', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(8, 'Rust', 'Programming Language', NULL, 'rust', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(9, 'PHP', 'Programming Language', NULL, 'php,laravel', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(10, 'Swift', 'Programming Language', NULL, 'swift,ios', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(11, 'Kotlin', 'Programming Language', NULL, 'kotlin,android', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(12, 'Ruby', 'Programming Language', NULL, 'ruby,rails', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Frameworks & Libraries (13-26)
(13, '.NET Core', 'Framework', 'dotnet core', '.net core,asp.net core', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(14, 'ASP.NET Core', 'Framework', 'asp.net core,aspnetcore', 'asp.net core,aspnet core web api', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(15, 'Entity Framework', 'Framework', 'ef,ef core', 'entity framework,ef core', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(16, 'Web API', 'Framework', 'webapi,rest api', 'web api,restful api', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(17, 'REST API', 'Framework', 'restful api', 'rest api,restful web service', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(18, 'React', 'Framework', 'reactjs', 'react,reactjs,react.js', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(19, 'Angular', 'Framework', 'angularjs', 'angular,angular 2+', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(20, 'Vue.js', 'Framework', 'vue', 'vue,vuejs', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(21, 'Spring Boot', 'Framework', 'spring', 'spring,spring boot', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(22, 'Node.js', 'Framework', 'node', 'node,nodejs', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(23, 'Django', 'Framework', NULL, 'django,python web', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(24, 'Flask', 'Framework', NULL, 'flask', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(25, 'Express.js', 'Framework', 'express', 'express,expressjs', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(26, 'Hibernate', 'Framework', NULL, 'hibernate,jpa', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Databases (27-35)
(27, 'SQL Server', 'Database', 'mssql', 'sql server,mssql', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(28, 'MySQL', 'Database', NULL, 'mysql', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(29, 'PostgreSQL', 'Database', 'postgres', 'postgresql,postgres', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(30, 'MongoDB', 'Database', 'mongo', 'mongodb,mongo', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(31, 'Redis', 'Database', NULL, 'redis,cache', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(32, 'Elasticsearch', 'Database', 'es', 'elasticsearch,es', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(33, 'Oracle', 'Database', NULL, 'oracle', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(34, 'Cassandra', 'Database', NULL, 'cassandra', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(35, 'DynamoDB', 'Database', 'dynamo', 'dynamodb,aws dynamo', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Cloud & DevOps (36-44)
(36, 'Azure', 'Cloud', 'microsoft azure', 'azure,azure devops', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(37, 'AWS', 'Cloud', 'amazon web services', 'aws,ec2,s3,lambda', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(38, 'Google Cloud', 'Cloud', 'gcp', 'gcp,google cloud', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(39, 'Docker', 'DevOps', NULL, 'docker,container', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(40, 'Kubernetes', 'DevOps', 'k8s', 'kubernetes,k8s', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(41, 'Jenkins', 'DevOps', NULL, 'jenkins,ci/cd', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(42, 'Git', 'DevOps', NULL, 'git,github,gitlab', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(43, 'Terraform', 'DevOps', NULL, 'terraform,iac', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(44, 'Ansible', 'DevOps', NULL, 'ansible', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Soft Skills (45-49)
(45, 'Teamwork', 'Soft Skill', 'collaboration', 'teamwork,team work', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(46, 'Communication', 'Soft Skill', NULL, 'communication,verbal,written', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(47, 'Problem Solving', 'Soft Skill', 'analytical', 'problem solving,analytical', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(48, 'Leadership', 'Soft Skill', NULL, 'leadership,lead', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(49, 'Time Management', 'Soft Skill', NULL, 'time management,organize', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Languages (50-54)
(50, 'English', 'Language', NULL, 'english,ielts,toeic', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(51, 'Vietnamese', 'Language', 'tiếng việt', 'vietnamese,tieng viet', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(52, 'Japanese', 'Language', '日本語', 'japanese,nihongo,jlpt', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(53, 'Chinese', 'Language', 'mandarin', 'chinese,mandarin', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(54, 'Korean', 'Language', '한국어', 'korean,topik', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Security (55-56)
(55, 'JWT', 'Security', 'json web token', 'jwt authentication,jwt token', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(56, 'OAuth2', 'Security', 'oauth 2.0', 'oauth2,oauth 2.0', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Architecture (57-60)
(57, 'Microservices', 'Architecture', 'microservice', 'microservices architecture,msa', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(58, 'Clean Architecture', 'Architecture', 'onion architecture', 'clean architecture,onion architecture', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(59, 'CQRS', 'Architecture', 'cqrs pattern', 'cqrs,command query responsibility segregation', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(60, 'Event Sourcing', 'Architecture', 'event sourcing pattern', 'event sourcing,event driven', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Testing (61-64)
(61, 'Unit Testing', 'Testing', 'unit test', 'unit testing,xunit,nunit', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(62, 'Integration Testing', 'Testing', 'integration test', 'integration testing', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(63, 'xUnit', 'Testing', 'xunit.net', 'xunit,unit testing', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(64, 'Moq', 'Testing', 'mock', 'moq,mocking', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Frontend (65-73)
(65, 'HTML5', 'Frontend', 'html', 'html5,html', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(66, 'CSS3', 'Frontend', 'css', 'css3,css', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(67, 'Bootstrap', 'Frontend', NULL, 'bootstrap,css framework', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(68, 'Tailwind CSS', 'Frontend', 'tailwind', 'tailwind,tailwindcss', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(69, 'jQuery', 'Frontend', 'jquery', 'jquery,javascript library', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(70, 'HTML', 'Frontend', 'html5', 'html,html5', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(71, 'CSS', 'Frontend', 'css3', 'css,css3', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(72, 'Tailwind', 'Frontend', 'tailwind css', 'tailwind,tailwindcss', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(73, 'Redux', 'Frontend', 'reduxjs', 'redux,reduxjs,react redux', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Mobile (74-77)
(74, 'React Native', 'Mobile', 'reactnative', 'react native,mobile app', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(75, 'Flutter', 'Mobile', NULL, 'flutter,dart', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(76, 'Android', 'Mobile', 'android dev', 'android,kotlin android', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(77, 'iOS', 'Mobile', 'iphone', 'ios,swift ios', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),

-- Message Queue (78-81)
(78, 'RabbitMQ', 'Message Queue', 'rabbit mq', 'rabbitmq,message queue', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(79, 'Kafka', 'Message Queue', 'apache kafka', 'kafka,event streaming', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(80, 'Azure Service Bus', 'Message Queue', 'service bus', 'azure service bus,servicebus', 1, '2024-01-01 00:00:00', NULL, 'system', NULL),
(81, 'Redis Pub/Sub', 'Message Queue', 'redis pubsub', 'redis pubsub,redis messaging', 1, '2024-01-01 00:00:00', NULL, 'system', NULL);



SET IDENTITY_INSERT [Skills] OFF;

-- Thêm các skill bị thiếu (chạy TRƯỚC phần JobSkills)
SET IDENTITY_INSERT [Skills] ON;

INSERT INTO [Skills] ([Id], [Name], [Category], [Aliases], [ContextKeywords], [IsActive], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy]) VALUES
(82, 'TensorFlow', 'ML/AI', 'tf', 'tensorflow,deep learning', 1, GETUTCDATE(), NULL, 'system', NULL),
(83, 'Machine Learning', 'ML/AI', 'ml', 'machine learning,ml', 1, GETUTCDATE(), NULL, 'system', NULL),
(84, 'Deep Learning', 'ML/AI', 'dl', 'deep learning,neural network', 1, GETUTCDATE(), NULL, 'system', NULL),
(85, 'NLP', 'ML/AI', 'natural language processing', 'nlp,natural language', 1, GETUTCDATE(), NULL, 'system', NULL),
(86, 'Pandas', 'ML/AI', NULL, 'pandas,data analysis', 1, GETUTCDATE(), NULL, 'system', NULL),
(87, 'ETL', 'Data', 'extract transform load', 'etl,data pipeline', 1, GETUTCDATE(), NULL, 'system', NULL),
(88, 'Spark', 'Data', 'apache spark', 'spark,big data', 1, GETUTCDATE(), NULL, 'system', NULL),
(89, 'Power BI', 'Data', 'powerbi', 'power bi,bi,analytics', 1, GETUTCDATE(), NULL, 'system', NULL),
(90, 'Linux', 'OS', NULL, 'linux,unix', 1, GETUTCDATE(), NULL, 'system', NULL),
(91, 'Figma', 'Design', NULL, 'figma,ui design', 1, GETUTCDATE(), NULL, 'system', NULL),
(92, 'Adobe XD', 'Design', 'xd', 'adobe xd,ui/ux', 1, GETUTCDATE(), NULL, 'system', NULL),
(93, 'User Research', 'Design', 'ux research', 'user research,ux', 1, GETUTCDATE(), NULL, 'system', NULL),
(94, 'Prototyping', 'Design', 'prototype', 'prototyping,wireframe', 1, GETUTCDATE(), NULL, 'system', NULL),
(95, 'Design Systems', 'Design', NULL, 'design system,ui kit', 1, GETUTCDATE(), NULL, 'system', NULL);

SET IDENTITY_INSERT [Skills] OFF;

-- ============================================
-- 2. USERS
-- ============================================
INSERT INTO [Users] ([Id], [Email], [FullName], [Role], [Status], [EmailVerified], [CreatedAt], [UpdatedAt], [LastLoginAt], [Gender], [DateOfBirth], [PhoneNumber], [AvatarUrl], [PermissionCodes]) VALUES
-- Admins
('11111111-1111-1111-1111-111111111111', 'admin@recruitai.com', 'System Administrator', 1, 1, 1, GETUTCDATE(), NULL, NULL, NULL, NULL, NULL, NULL, 'P001,P002,P003,P004,P005,P006,P007,P008,P009,P010,P011,P012,P013,P014,P015,P016,P017,P018,P019,P020,P101,P102,P103,P104,P105,P106,P201,P202,P203,P204,P301,P302,P303,P304'),
('22222222-2222-2222-2222-222222222222', 'superadmin@recruitai.com', 'Super Administrator', 1, 1, 1, GETUTCDATE(), NULL, NULL, NULL, NULL, NULL, NULL, 'P001,P002,P003,P004,P005,P006,P007,P008,P009,P010,P011,P012,P013,P014,P015,P016,P017,P018,P019,P020,P101,P102,P103,P104,P105,P106,P201,P202,P203,P204,P301,P302,P303,P304'),

-- Recruiters
('33333333-3333-3333-3333-333333333333', 'recruiter1@techcorp.com', 'Nguyen Van A', 2, 1, 1, GETUTCDATE(), NULL, NULL, 1, '1990-05-15', '0901234001', NULL, 'P001,P004,P005,P006,P007,P008,P009,P010,P011,P201,P202,P203,P204,P301,P302,P303,P304'),
('44444444-4444-4444-4444-444444444444', 'recruiter2@datasolution.com', 'Tran Thi B', 2, 1, 1, GETUTCDATE(), NULL, NULL, 2, '1992-08-20', '0901234002', NULL, 'P001,P004,P005,P006,P007,P008,P009,P010,P011,P201,P202,P203,P204,P301,P302,P303,P304'),
('55555555-5555-5555-5555-555555555555', 'recruiter3@aistartup.com', 'Le Van C', 2, 1, 1, GETUTCDATE(), NULL, NULL, 1, '1988-03-10', '0901234003', NULL, 'P001,P004,P005,P006,P007,P008,P009,P010,P011,P201,P202,P203,P204,P301,P302,P303,P304'),

-- Candidates
('66666666-6666-6666-6666-666666666666', 'candidate1@gmail.com', 'Pham Van D', 3, 1, 1, GETUTCDATE(), NULL, NULL, 1, '1995-10-25', '0901234004', NULL, 'P001,P002,P003,P004,P005,P009,P101,P102,P103,P104,P105,P106,P301,P302,P303,P304'),
('77777777-7777-7777-7777-777777777777', 'candidate2@gmail.com', 'Nguyen Thi E', 3, 1, 1, GETUTCDATE(), NULL, NULL, 2, '1997-02-14', '0901234005', NULL, 'P001,P002,P003,P004,P005,P009,P101,P102,P103,P104,P105,P106,P301,P302,P303,P304'),
('88888888-8888-8888-8888-888888888888', 'candidate3@gmail.com', 'Hoang Van F', 3, 1, 1, GETUTCDATE(), NULL, NULL, 1, '1996-07-30', '0901234006', NULL, 'P001,P002,P003,P004,P005,P009,P101,P102,P103,P104,P105,P106,P301,P302,P303,P304');

-- ============================================
-- 3. AUTH PROVIDERS (Passwords: Admin@123, Recruiter@123, Candidate@123)
-- Lưu ý: Dùng BCrypt hash, khi login cần mã hóa cùng thuật toán
-- ============================================
INSERT INTO [AuthProviders] ([Id], [UserId], [Provider], [ProviderUserId], [ProviderEmail], [PasswordHash], [CreatedAt], [LastLoginAt]) VALUES
(NEWID(), '11111111-1111-1111-1111-111111111111', 0, 'admin@recruitai.com', 'admin@recruitai.com', '$2a$11$SGJiTV7kYqMgWzLZK7cYWOtXVhC4ZLBLPq1T6wLyBQFbh5E56Exx.', GETUTCDATE(), NULL),
(NEWID(), '22222222-2222-2222-2222-222222222222', 0, 'superadmin@recruitai.com', 'superadmin@recruitai.com', '$2a$11$SGJiTV7kYqMgWzLZK7cYWOtXVhC4ZLBLPq1T6wLyBQFbh5E56Exx.', GETUTCDATE(), NULL),
(NEWID(), '33333333-3333-3333-3333-333333333333', 0, 'recruiter1@techcorp.com', 'recruiter1@techcorp.com', '$2a$11$0xRknU3LkYpM5kL6ZRj7N.VP5j9XyWgLvYkQzLwFhRjMpLxHjUeC', GETUTCDATE(), NULL),
(NEWID(), '44444444-4444-4444-4444-444444444444', 0, 'recruiter2@datasolution.com', 'recruiter2@datasolution.com', '$2a$11$0xRknU3LkYpM5kL6ZRj7N.VP5j9XyWgLvYkQzLwFhRjMpLxHjUeC', GETUTCDATE(), NULL),
(NEWID(), '55555555-5555-5555-5555-555555555555', 0, 'recruiter3@aistartup.com', 'recruiter3@aistartup.com', '$2a$11$0xRknU3LkYpM5kL6ZRj7N.VP5j9XyWgLvYkQzLwFhRjMpLxHjUeC', GETUTCDATE(), NULL),
(NEWID(), '66666666-6666-6666-6666-666666666666', 0, 'candidate1@gmail.com', 'candidate1@gmail.com', '$2a$11$NQYk5L6ZRj7NVP5j9XyWgLvYkQzLwFhRjMpLxHjUeC0xRknU3LkYp', GETUTCDATE(), NULL),
(NEWID(), '77777777-7777-7777-7777-777777777777', 0, 'candidate2@gmail.com', 'candidate2@gmail.com', '$2a$11$NQYk5L6ZRj7NVP5j9XyWgLvYkQzLwFhRjMpLxHjUeC0xRknU3LkYp', GETUTCDATE(), NULL),
(NEWID(), '88888888-8888-8888-8888-888888888888', 0, 'candidate3@gmail.com', 'candidate3@gmail.com', '$2a$11$NQYk5L6ZRj7NVP5j9XyWgLvYkQzLwFhRjMpLxHjUeC0xRknU3LkYp', GETUTCDATE(), NULL);

-- ============================================
-- 4. COMPANIES
-- ============================================
INSERT INTO [Companies] ([Id], [Name], [Slug], [Address], [Website], [CreatedAt], [UpdatedAt], [CreatedBy]) VALUES
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'Tech Solutions Vietnam', 'tech-solutions-vietnam', 'Tòa nhà ABC, Quận 1, TP.HCM', 'https://techsolutions.vn', GETUTCDATE(), NULL, '33333333-3333-3333-3333-333333333333'),
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 'Data Solutions', 'data-solutions', 'Tầng 10, Tòa nhà XYZ, Quận 3, TP.HCM', 'https://datasolutions.vn', GETUTCDATE(), NULL, '44444444-4444-4444-4444-444444444444'),
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 'AI Startup', 'ai-startup', 'Tầng 5, Tòa nhà DEF, Quận 7, TP.HCM', 'https://aistartup.vn', GETUTCDATE(), NULL, '55555555-5555-5555-5555-555555555555');

-- ============================================
-- 5. JOBS
-- ============================================
INSERT INTO [Jobs] ([Id], [RecruiterId], [CompanyId], [Title], [Description], [Requirements], [Location], [SalaryMin], [SalaryMax], [Currency], [EmploymentType], [ExperienceLevel], [Department], [Benefits], [ExpirationDate], [CreatedAt], [UpdatedAt], [Status], [IsDeleted], [IsActive], [Views], [Applications], [IsFeatured], [FeaturedOrder]) VALUES
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '33333333-3333-3333-3333-333333333333', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '.NET Backend Developer', 'We are looking for a skilled .NET Backend Developer to join our engineering team. You will be responsible for building RESTful APIs, optimizing database performance, and implementing authentication systems.', 'Strong experience with C#, ASP.NET Core, Entity Framework, SQL Server. Knowledge of Redis and microservices is a plus.', 'Ho Chi Minh City', 15000000, 25000000, 0, 0, 2, 'Engineering', '13th month salary, Health insurance, 12 days annual leave, Hybrid working', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 150, 0, 0, NULL),
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', '33333333-3333-3333-3333-333333333333', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'Frontend React Developer', 'Join our frontend team to build responsive and high-performance user interfaces using ReactJS and modern web technologies.', 'Proficient in ReactJS, JavaScript, TypeScript, HTML5, CSS3. Experience with Tailwind CSS and state management (Redux).', 'Da Nang', 12000000, 20000000, 0, 0, 2, 'Engineering', '13th month salary, Health insurance, Remote work option, Learning budget', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 120, 0, 0, NULL),
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', '55555555-5555-5555-5555-555555555555', 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 'AI/ML Engineer', 'Develop and deploy machine learning models for candidate screening, NLP applications, and data-driven solutions.', 'Strong Python skills, experience with TensorFlow, Scikit-learn, Pandas. Knowledge of NLP and model deployment.', 'Hanoi', 20000000, 35000000, 0, 0, 3, 'AI Research', '13th month salary, Stock options, Premium healthcare, 15 days annual leave', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 200, 0, 0, NULL),
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', '44444444-4444-4444-4444-444444444444', NULL, 'DevOps Engineer', 'Design and maintain CI/CD pipelines, manage cloud infrastructure on AWS, and automate deployment processes.', 'Experience with Docker, Kubernetes, Jenkins, GitHub Actions, AWS services. Knowledge of infrastructure as code.', 'Ho Chi Minh City', 18000000, 28000000, 0, 0, 2, 'Platform Engineering', '13th month salary, AWS certification support, 14 days annual leave, Gym membership', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 100, 0, 0, NULL),
('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', '44444444-4444-4444-4444-444444444444', NULL, 'Data Engineer', 'Build and maintain ETL pipelines, process large datasets, and create data visualizations for business intelligence.', 'Strong SQL and Python skills. Experience with Spark, Power BI, and ETL processes. Data warehousing knowledge.', 'Da Nang', 16000000, 26000000, 0, 0, 2, 'Data', '13th month salary, Health insurance, Flexible working hours, Training courses', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 80, 0, 0, NULL),
('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', '33333333-3333-3333-3333-333333333333', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'Full Stack Developer', 'Looking for a versatile Full Stack Developer who can work on both frontend and backend systems.', 'Experience with React, Node.js, MongoDB, RESTful APIs. Understanding of the full development lifecycle.', 'Ho Chi Minh City', 18000000, 30000000, 0, 0, 2, 'Engineering', '13th month salary, Health insurance, Flexible working hours', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 95, 0, 0, NULL),
('99999999-9999-9999-9999-999999999999', '44444444-4444-4444-4444-444444444444', NULL, 'Java Backend Developer', 'Seeking experienced Java developer to build scalable backend services.', 'Strong Java, Spring Boot, Hibernate, MySQL/MongoDB. Microservices experience is a plus.', 'Hanoi', 16000000, 27000000, 0, 0, 2, 'Engineering', '13th month salary, Health insurance, Laptop provided', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 70, 0, 0, NULL),
('AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE', '55555555-5555-5555-5555-555555555555', 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 'Mobile Developer (React Native)', 'Join our mobile team to build cross-platform mobile applications using React Native.', 'Experience with React Native, JavaScript/TypeScript, mobile app deployment for iOS and Android.', 'Da Nang', 14000000, 24000000, 0, 0, 2, 'Mobile', '13th month salary, Health insurance, Flexible working hours', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 60, 0, 0, NULL),
('BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF', '33333333-3333-3333-3333-333333333333', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'Cloud Engineer (AWS)', 'Manage and optimize cloud infrastructure on AWS platform.', 'AWS certifications, experience with EC2, S3, Lambda, CloudFormation. Knowledge of DevOps practices.', 'Ho Chi Minh City', 20000000, 35000000, 0, 0, 3, 'Cloud', '13th month salary, AWS certification bonus, Premium insurance', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 55, 0, 0, NULL),
('CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA', '44444444-4444-4444-4444-444444444444', NULL, 'UI/UX Designer', 'Design beautiful and user-friendly interfaces for web and mobile applications.', 'Figma, Adobe XD, user research, prototyping. Understanding of design systems.', 'Hanoi', 12000000, 22000000, 0, 0, 2, 'Design', '13th month salary, Health insurance, Creative environment', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), NULL, 1, 0, 1, 45, 0, 0, NULL);

-- ============================================
-- 6. JOB SKILLS
-- ============================================
INSERT INTO [JobSkills] ([JobId], [SkillId], [IsRequired]) VALUES
-- Job 1: .NET Backend Developer
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 1, 1),   -- C#
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 13, 1),  -- .NET Core
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 14, 1),  -- ASP.NET Core
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 15, 1),  -- Entity Framework
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 27, 1),  -- SQL Server
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 31, 0),  -- Redis
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 17, 1),  -- REST API
('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 55, 1),  -- JWT

-- Job 2: Frontend React Developer
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 18, 1),  -- React
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 4, 1),   -- JavaScript
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 5, 1),   -- TypeScript
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 65, 1),  -- HTML5
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 66, 1),  -- CSS3
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 68, 0),  -- Tailwind CSS
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 73, 0),  -- Redux
('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 17, 1),  -- REST API

-- Job 3: AI/ML Engineer
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 3, 1),   -- Python
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 82, 1),  -- TensorFlow (cần thêm)
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 83, 1),  -- Machine Learning
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 84, 1),  -- Deep Learning
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 85, 1),  -- NLP
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 86, 0),  -- Pandas
('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 6, 1),   -- SQL

-- Job 4: DevOps Engineer
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 39, 1),  -- Docker
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 40, 1),  -- Kubernetes
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 41, 1),  -- Jenkins
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 37, 1),  -- AWS
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 42, 1),  -- Git
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 43, 0),  -- Terraform
('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 44, 0),  -- Ansible

-- Job 5: Data Engineer
('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 6, 1),   -- SQL
('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 3, 1),   -- Python
('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 87, 1),  -- ETL
('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 88, 0),  -- Spark
('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 89, 0),  -- Power BI
('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 86, 1),  -- Pandas

-- Job 6: Full Stack Developer
('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 18, 1),   -- React
('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 22, 1),   -- Node.js
('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 4, 1),    -- JavaScript
('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 30, 1),   -- MongoDB
('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 25, 1),   -- Express.js
('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 17, 1),   -- REST API

-- Job 7: Java Backend Developer
('99999999-9999-9999-9999-999999999999', 2, 1),   -- Java
('99999999-9999-9999-9999-999999999999', 21, 1),  -- Spring Boot
('99999999-9999-9999-9999-999999999999', 26, 1),  -- Hibernate
('99999999-9999-9999-9999-999999999999', 28, 0),  -- MySQL
('99999999-9999-9999-9999-999999999999', 30, 0),  -- MongoDB
('99999999-9999-9999-9999-999999999999', 57, 0),  -- Microservices

-- Job 8: Mobile Developer
('AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE', 74, 1),  -- React Native
('AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE', 4, 1),   -- JavaScript
('AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE', 5, 1),   -- TypeScript
('AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE', 73, 0),  -- Redux
('AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE', 17, 1),  -- REST API

-- Job 9: Cloud Engineer
('BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF', 37, 1),  -- AWS
('BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF', 39, 1),  -- Docker
('BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF', 40, 1),  -- Kubernetes
('BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF', 43, 0),  -- Terraform
('BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF', 41, 0),  -- Jenkins
('BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF', 90, 0),  -- Linux

-- Job 10: UI/UX Designer
('CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA', 91, 1),  -- Figma
('CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA', 92, 1),  -- Adobe XD
('CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA', 93, 1),  -- User Research
('CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA', 94, 1),  -- Prototyping
('CCCCCCCC-DDDD-EEEE-FFFF-AAAAAAAAAAAA', 95, 0);  -- Design Systems

-- ============================================
-- 7. CVS (Candidates' CVs)
-- ============================================
INSERT INTO [CVs] ([Id], [UserId], [FileName], [StoredFileName], [FilePath], [FileSize], [ContentType], [Status], [UploadedAt], [ProcessedAt], [ErrorMessage], [IsDeleted], [DeletedAt], [ExtractedText], [AnalyzedAt]) VALUES
(NEWID(), '66666666-6666-6666-6666-666666666666', 'PhamVanD_CV.pdf', 'cv_66666666_1.pdf', '/uploads/cvs/cv_66666666_1.pdf', 245760, 'application/pdf', 2, DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, -14, GETUTCDATE()), NULL, 0, NULL, 'Experienced .NET developer with 3 years of experience in C#, ASP.NET Core, and SQL Server. Familiar with React and cloud deployment.', DATEADD(DAY, -14, GETUTCDATE())),
(NEWID(), '77777777-7777-7777-7777-777777777777', 'NguyenThiE_CV.pdf', 'cv_77777777_1.pdf', '/uploads/cvs/cv_77777777_1.pdf', 189440, 'application/pdf', 2, DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -9, GETUTCDATE()), NULL, 0, NULL, 'Frontend developer with 2 years of React experience. Strong JavaScript, TypeScript, and Tailwind CSS skills. Looking for challenging projects.', DATEADD(DAY, -9, GETUTCDATE())),
(NEWID(), '88888888-8888-8888-8888-888888888888', 'HoangVanF_CV.pdf', 'cv_88888888_1.pdf', '/uploads/cvs/cv_88888888_1.pdf', 302080, 'application/pdf', 2, DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, -19, GETUTCDATE()), NULL, 0, NULL, 'Full-stack developer proficient in Java, Spring Boot, React, and MongoDB. 4 years of experience in building scalable web applications.', DATEADD(DAY, -19, GETUTCDATE()));

-- ============================================
-- 8. JOB APPLICATIONS
-- ============================================
INSERT INTO [JobApplications] ([Id], [JobId], [CVId], [Status], [AppliedAt], [ReviewedAt], [Notes]) VALUES
(NEWID(), 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', (SELECT TOP 1 Id FROM [CVs] WHERE UserId = '66666666-6666-6666-6666-666666666666'), 1, DATEADD(DAY, -10, GETUTCDATE()), NULL, NULL),
(NEWID(), 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', (SELECT TOP 1 Id FROM [CVs] WHERE UserId = '77777777-7777-7777-7777-777777777777'), 1, DATEADD(DAY, -8, GETUTCDATE()), NULL, NULL),
(NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', (SELECT TOP 1 Id FROM [CVs] WHERE UserId = '88888888-8888-8888-8888-888888888888'), 1, DATEADD(DAY, -12, GETUTCDATE()), NULL, NULL),
(NEWID(), 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF', (SELECT TOP 1 Id FROM [CVs] WHERE UserId = '66666666-6666-6666-6666-666666666666'), 1, DATEADD(DAY, -5, GETUTCDATE()), NULL, NULL),
(NEWID(), 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', (SELECT TOP 1 Id FROM [CVs] WHERE UserId = '88888888-8888-8888-8888-888888888888'), 2, DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()), 'Good fit for the role, moving to interview stage');

-- ============================================
-- 9. JOB APPLICATION MATCHES
-- ============================================
INSERT INTO [JobApplicationMatches] ([Id], [ApplicationId], [MatchPercentage], [RequiredSkillCount], [MatchedSkillCount], [MatchedSkillsJson], [MissingSkillsJson], [CalculatedAt]) VALUES
(NEWID(), (SELECT TOP 1 Id FROM [JobApplications] WHERE JobId = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA'), 85, 8, 7, '["C#", ".NET Core", "ASP.NET Core", "SQL Server", "REST API", "JWT"]', '["Redis"]', DATEADD(DAY, -9, GETUTCDATE())),
(NEWID(), (SELECT TOP 1 Id FROM [JobApplications] WHERE JobId = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB'), 90, 8, 8, '["React", "JavaScript", "TypeScript", "HTML5", "CSS3", "REST API", "Redux"]', '[]', DATEADD(DAY, -7, GETUTCDATE())),
(NEWID(), (SELECT TOP 1 Id FROM [JobApplications] WHERE JobId = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC'), 70, 7, 5, '["Python", "SQL", "Pandas"]', '["TensorFlow", "Machine Learning", "NLP"]', DATEADD(DAY, -11, GETUTCDATE())),
(NEWID(), (SELECT TOP 1 Id FROM [JobApplications] WHERE JobId = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF'), 75, 6, 5, '["React", "JavaScript", "MongoDB", "REST API"]', '["Node.js", "Express.js"]', DATEADD(DAY, -4, GETUTCDATE()));

-- ============================================
-- 10. SAVED JOBS
-- ============================================
INSERT INTO [SavedJobs] ([Id], [JobId], [UserId], [SavedAt]) VALUES
(NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', '66666666-6666-6666-6666-666666666666', DATEADD(DAY, -14, GETUTCDATE())),
(NEWID(), 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', '77777777-7777-7777-7777-777777777777', DATEADD(DAY, -7, GETUTCDATE())),
(NEWID(), 'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', '88888888-8888-8888-8888-888888888888', DATEADD(DAY, -5, GETUTCDATE()));

-- ============================================
-- 11. NOTIFICATIONS
-- ============================================
INSERT INTO [Notifications] ([Id], [UserId], [Title], [Content], [Type], [IsRead], [Data], [CreatedAt]) VALUES
(NEWID(), '66666666-6666-6666-6666-666666666666', 'New job match!', 'Your profile matches a new .NET Backend Developer position', 'job_match', 0, '{"jobId": "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA", "matchPercentage": 85}', DATEADD(DAY, -9, GETUTCDATE())),
(NEWID(), '77777777-7777-7777-7777-777777777777', 'Application reviewed', 'Your application for Frontend React Developer has been reviewed', 'application_update', 0, '{"applicationId": "...", "status": "Under Review"}', DATEADD(DAY, -2, GETUTCDATE())),
(NEWID(), '88888888-8888-8888-8888-888888888888', 'New job alert', 'AI/ML Engineer position matches your skills', 'job_match', 1, '{"jobId": "CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"}', DATEADD(DAY, -12, GETUTCDATE()));

-- ============================================
-- 12. REFRESH TOKENS (example for testing)
-- ============================================
INSERT INTO [RefreshTokens] ([Id], [UserId], [Token], [ExpireAt], [IsRevoked], [ReplacedByToken], [CreatedByIp], [RevokedByIp], [CreatedAt], [RevokedAt], [TokenType]) VALUES
(NEWID(), '66666666-6666-6666-6666-666666666666', 'sample_refresh_token_1', DATEADD(DAY, 7, GETUTCDATE()), 0, NULL, '127.0.0.1', NULL, GETUTCDATE(), NULL, 0);

-- ============================================
-- 13. PASSWORD RESET TOKENS (example)
-- ============================================
INSERT INTO [PasswordResetTokens] ([Id], [UserId], [Token], [ExpiryDate], [IsUsed], [CreatedAt], [UsedAt], [CreatedByIp]) VALUES
(NEWID(), '66666666-6666-6666-6666-666666666666', 'sample_reset_token_1', DATEADD(HOUR, 24, GETUTCDATE()), 0, GETUTCDATE(), NULL, '127.0.0.1');

-- ============================================
-- 14. AUDIT LOGS (sample)
-- ============================================
INSERT INTO [AuditLogs] ([Id], [EntityType], [Action], [EntityId], [EntityName], [OldValue], [NewValue], [Reason], [ChangedBy], [ChangedByIp], [UserAgent], [RequestId], [ChangedAt]) VALUES
(NEWID(), 2, 1, 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '.NET Backend Developer', NULL, '{"title": ".NET Backend Developer", "status": "Published"}', 'Initial job posting', 'recruiter1@techcorp.com', '192.168.1.100', 'Mozilla/5.0', 'req_001', DATEADD(DAY, -30, GETUTCDATE())),
(NEWID(), 0, 5, '66666666-6666-6666-6666-666666666666', 'candidate1@gmail.com', NULL, '{"role": "CANDIDATE", "status": "Active"}', 'User registration', 'system', '192.168.1.101', 'System', 'req_002', DATEADD(DAY, -20, GETUTCDATE())),
(NEWID(), 3, 6, (SELECT TOP 1 Id FROM [JobApplications] WHERE JobId = 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD'), 'Job Application', NULL, '{"status": "Reviewed"}', 'Application status update', 'recruiter2@datasolution.com', '192.168.1.102', 'Mozilla/5.0', 'req_003', DATEADD(DAY, -2, GETUTCDATE()));

-- ============================================
-- 15. TESTS (from TestConfiguration)
-- ============================================
INSERT INTO [Tests] ([FirstName], [LastName]) VALUES
('John', 'Doe'),
('Jane', 'Smith'),
('Test', 'User');

-- ============================================
-- UPDATE Job Applications count in Jobs table
-- ============================================
UPDATE [Jobs] SET [Applications] = 1 WHERE [Id] = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA';
UPDATE [Jobs] SET [Applications] = 1 WHERE [Id] = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB';
UPDATE [Jobs] SET [Applications] = 1 WHERE [Id] = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC';
UPDATE [Jobs] SET [Applications] = 1 WHERE [Id] = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF';
UPDATE [Jobs] SET [Applications] = 1 WHERE [Id] = 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD';

-- ============================================
-- VERIFICATION QUERIES (chạy để kiểm tra)
-- ============================================
-- SELECT 'Skills' as TableName, COUNT(*) as RowCount FROM Skills
-- UNION ALL SELECT 'Users', COUNT(*) FROM Users
-- UNION ALL SELECT 'AuthProviders', COUNT(*) FROM AuthProviders
-- UNION ALL SELECT 'Companies', COUNT(*) FROM Companies
-- UNION ALL SELECT 'Jobs', COUNT(*) FROM Jobs
-- UNION ALL SELECT 'JobSkills', COUNT(*) FROM JobSkills
-- UNION ALL SELECT 'CVs', COUNT(*) FROM CVs
-- UNION ALL SELECT 'JobApplications', COUNT(*) FROM JobApplications
-- UNION ALL SELECT 'JobApplicationMatches', COUNT(*) FROM JobApplicationMatches
-- UNION ALL SELECT 'SavedJobs', COUNT(*) FROM SavedJobs
-- UNION ALL SELECT 'Notifications', COUNT(*) FROM Notifications;