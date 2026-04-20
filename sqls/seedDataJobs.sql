-- Seed Jobs
INSERT INTO [Jobs] (Id, RecruiterId, Title, Description, Requirements, Location, SalaryMin, SalaryMax, Currency, EmploymentType, ExperienceLevel, Department, Benefits, ExpirationDate, CreatedAt, Status, IsActive, Views, Applications, CompanyId, IsFeatured) VALUES
-- 1. .NET Backend Developer (EN)
(NEWID(), '33333333-3333-3333-3333-333333333333', N'.NET Backend Developer', N'Build RESTful APIs and backend services.', N'C#, .NET Core, SQL Server', N'Ho Chi Minh City', 15000000, 25000000, 1, 1, 3, N'Engineering', N'13th month, Health insurance', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '11111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 1),
-- 2. Lập trình viên Frontend React (VI)
(NEWID(), '44444444-4444-4444-4444-444444444444', N'Lập trình viên Frontend React', N'Xây dựng giao diện người dùng với ReactJS.', N'React, JavaScript, TypeScript, Tailwind', N'Đà Nẵng', 12000000, 20000000, 1, 4, 2, N'Engineering', N'13th month, Remote option', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '22222222-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 0),
-- 3. AI/ML Engineer (EN)
(NEWID(), '55555555-5555-5555-5555-555555555555', N'AI/ML Engineer', N'Develop ML models for candidate screening.', N'Python, TensorFlow, NLP', N'Hanoi', 20000000, 35000000, 1, 1, 4, N'AI Research', N'Stock options, Premium healthcare', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '33333333-CCCC-CCCC-CCCC-CCCCCCCCCCCC', 1),
-- 4. Kỹ sư DevOps (VI)
(NEWID(), '33333333-3333-3333-3333-333333333333', N'Kỹ sư DevOps', N'Quản lý CI/CD và cloud infrastructure.', N'Docker, Kubernetes, AWS, Jenkins', N'Ho Chi Minh City', 18000000, 28000000, 1, 1, 3, N'Platform', N'AWS certification support', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '44444444-DDDD-DDDD-DDDD-DDDDDDDDDDDD', 0),
-- 5. Data Engineer (EN)
(NEWID(), '44444444-4444-4444-4444-444444444444', N'Data Engineer', N'Build ETL pipelines and data warehouses.', N'SQL, Python, Spark, Power BI', N'Da Nang', 16000000, 26000000, 1, 1, 3, N'Data', N'Training courses', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '55555555-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 0),
-- 6. Full Stack Developer (VI)
(NEWID(), '55555555-5555-5555-5555-555555555555', N'Lập trình viên Full Stack', N'Phát triển cả frontend và backend.', N'React, Node.js, MongoDB', N'Ho Chi Minh City', 18000000, 30000000, 1, 1, 3, N'Engineering', N'Flexible hours', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '66666666-FFFF-FFFF-FFFF-FFFFFFFFFFFF', 0),
-- 7. Java Backend Developer (EN)
(NEWID(), '33333333-3333-3333-3333-333333333333', N'Java Backend Developer', N'Build scalable backend services with Java.', N'Java, Spring Boot, Hibernate, MySQL', N'Hanoi', 16000000, 27000000, 1, 1, 3, N'Engineering', N'Laptop provided', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '77777777-1111-1111-1111-111111111111', 0),
-- 8. Mobile Developer React Native (VI)
(NEWID(), '44444444-4444-4444-4444-444444444444', N'Lập trình viên Mobile React Native', N'Xây dựng ứng dụng di động đa nền tảng.', N'React Native, JavaScript, TypeScript, Redux', N'Da Nang', 14000000, 24000000, 1, 1, 2, N'Mobile', N'Flexible working', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '88888888-2222-2222-2222-222222222222', 0),
-- 9. Cloud Engineer AWS (EN)
(NEWID(), '55555555-5555-5555-5555-555555555555', N'Cloud Engineer AWS', N'Manage cloud infrastructure on AWS.', N'AWS, Docker, Kubernetes, Terraform', N'Ho Chi Minh City', 20000000, 35000000, 1, 1, 4, N'Cloud', N'Premium insurance', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, '99999999-3333-3333-3333-333333333333', 1),
-- 10. UI/UX Designer (VI)
(NEWID(), '33333333-3333-3333-3333-333333333333', N'Thiết kế UI/UX', N'Thiết kế giao diện thân thiện người dùng.', N'Figma, Adobe XD, Prototyping', N'Hanoi', 12000000, 22000000, 1, 1, 2, N'Design', N'Creative environment', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, 'AAAAAAAA-4444-4444-4444-444444444444', 0),
-- 11. Product Manager (EN)
(NEWID(), '44444444-4444-4444-4444-444444444444', N'Product Manager', N'Lead product development and roadmap.', N'Agile, Scrum, Product Strategy', N'Ho Chi Minh City', 25000000, 40000000, 1, 1, 5, N'Product', N'Bonus 2 months', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, 'BBBBBBBB-5555-5555-5555-555555555555', 1),
-- 12. Chuyên viên QA Testing (VI)
(NEWID(), '55555555-5555-5555-5555-555555555555', N'Chuyên viên QA Testing', N'Kiểm thử và đảm bảo chất lượng phần mềm.', N'Manual Testing, Automation, Selenium', N'Da Nang', 10000000, 18000000, 1, 1, 2, N'QA', N'Training provided', DATEADD(MONTH, 2, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, 'CCCCCCCC-6666-6666-6666-666666666666', 0),
-- 13. Database Administrator (EN)
(NEWID(), '33333333-3333-3333-3333-333333333333', N'Database Administrator', N'Manage and optimize database systems.', N'SQL Server, PostgreSQL, MongoDB, Performance Tuning', N'Ho Chi Minh City', 17000000, 28000000, 1, 1, 3, N'DBA', N'13th month salary', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, 'DDDDDDDD-7777-7777-7777-777777777777', 0),
-- 14. Kỹ sư bảo mật (VI)
(NEWID(), '44444444-4444-4444-4444-444444444444', N'Kỹ sư bảo mật', N'Đảm bảo an toàn thông tin hệ thống.', N'Cybersecurity, OWASP, Penetration Testing', N'Hanoi', 18000000, 32000000, 1, 1, 4, N'Security', N'Certification support', DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, 'EEEEEEEE-8888-8888-8888-888888888888', 0),
-- 15. Technical Support (EN)
(NEWID(), '55555555-5555-5555-5555-555555555555', N'Technical Support Engineer', N'Provide technical support to customers.', N'Customer Service, Troubleshooting, SQL', N'Ho Chi Minh City', 8000000, 12000000, 1, 1, 1, N'Support', N'Shift allowance', DATEADD(MONTH, 1, GETUTCDATE()), GETUTCDATE(), 2, 1, 0, 0, 'FFFFFFFF-9999-9999-9999-999999999999', 0);