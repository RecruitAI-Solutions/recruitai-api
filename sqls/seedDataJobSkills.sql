-- Seed JobSkills (dùng subquery)
INSERT INTO [JobSkills] (JobId, SkillId, IsRequired) VALUES
-- Job 1: .NET Backend Developer
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'.NET Backend Developer'), 1, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'.NET Backend Developer'), 13, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'.NET Backend Developer'), 14, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'.NET Backend Developer'), 27, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'.NET Backend Developer'), 17, 0),

-- Job 2: Frontend React
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Frontend React'), 18, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Frontend React'), 4, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Frontend React'), 5, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Frontend React'), 65, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Frontend React'), 73, 0),

-- Job 3: AI/ML Engineer
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'AI/ML Engineer'), 3, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'AI/ML Engineer'), 37, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'AI/ML Engineer'), 6, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'AI/ML Engineer'), 50, 0),

-- Job 4: DevOps
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư DevOps'), 39, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư DevOps'), 40, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư DevOps'), 37, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư DevOps'), 42, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư DevOps'), 41, 0),

-- Job 5: Data Engineer
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Data Engineer'), 3, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Data Engineer'), 6, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Data Engineer'), 30, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Data Engineer'), 46, 0),

-- Job 6: Full Stack
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Full Stack'), 18, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Full Stack'), 22, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Full Stack'), 4, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Full Stack'), 30, 0),

-- Job 7: Java Backend
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Java Backend Developer'), 2, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Java Backend Developer'), 21, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Java Backend Developer'), 26, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Java Backend Developer'), 28, 1),

-- Job 8: Mobile React Native
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Mobile React Native'), 74, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Mobile React Native'), 4, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Mobile React Native'), 5, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Mobile React Native'), 17, 0),

-- Job 9: Cloud Engineer
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Cloud Engineer AWS'), 37, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Cloud Engineer AWS'), 39, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Cloud Engineer AWS'), 40, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Cloud Engineer AWS'), 43, 1),

-- Job 10: UI/UX
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Thiết kế UI/UX'), 67, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Thiết kế UI/UX'), 46, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Thiết kế UI/UX'), 47, 1),

-- Job 11: Product Manager
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Product Manager'), 46, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Product Manager'), 48, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Product Manager'), 47, 1),

-- Job 12: QA Testing
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Chuyên viên QA Testing'), 61, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Chuyên viên QA Testing'), 62, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Chuyên viên QA Testing'), 42, 0),

-- Job 13: DBA
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Database Administrator'), 27, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Database Administrator'), 28, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Database Administrator'), 29, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Database Administrator'), 31, 0),

-- Job 14: Security Engineer
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư bảo mật'), 55, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư bảo mật'), 56, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Kỹ sư bảo mật'), 3, 0),

-- Job 15: Technical Support
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Technical Support Engineer'), 46, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Technical Support Engineer'), 47, 1),
((SELECT TOP 1 Id FROM Jobs WHERE Title = N'Technical Support Engineer'), 6, 0);