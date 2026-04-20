-- Seed JobApplications (mỗi candidate ứng tuyển 2-3 job)
INSERT INTO [JobApplications] (Id, JobId, CVId, Status, AppliedAt, ReviewedAt, Notes) VALUES
-- Candidate1 ứng tuyển
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'.NET Backend Developer'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '66666666-6666-6666-6666-666666666666'), 2, GETUTCDATE(), GETUTCDATE(), N'Good candidate with .NET experience'),
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Java Backend Developer'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '66666666-6666-6666-6666-666666666666'), 1, GETUTCDATE(), NULL, NULL),

-- Candidate2 ứng tuyển
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Frontend React'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '77777777-7777-7777-7777-777777777777'), 3, GETUTCDATE(), GETUTCDATE(), N'Strong React skills'),
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Full Stack'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '77777777-7777-7777-7777-777777777777'), 1, GETUTCDATE(), NULL, NULL),
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Thiết kế UI/UX'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '77777777-7777-7777-7777-777777777777'), 1, GETUTCDATE(), NULL, NULL),

-- Candidate3 ứng tuyển
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'AI/ML Engineer'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '88888888-8888-8888-8888-888888888888'), 4, GETUTCDATE(), GETUTCDATE(), N'Missing some ML skills'),
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Data Engineer'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '88888888-8888-8888-8888-888888888888'), 2, GETUTCDATE(), GETUTCDATE(), N'Good fit for data role'),
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Cloud Engineer AWS'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '88888888-8888-8888-8888-888888888888'), 1, GETUTCDATE(), NULL, NULL),

-- Candidate4 (quaqduy) ứng tuyển
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Full Stack'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '99999999-9999-9999-9999-999999999999'), 1, GETUTCDATE(), NULL, NULL),
(NEWID(), (SELECT TOP 1 Id FROM Jobs WHERE Title = N'Lập trình viên Mobile React Native'), (SELECT TOP 1 Id FROM CVs WHERE UserId = '99999999-9999-9999-9999-999999999999'), 1, GETUTCDATE(), NULL, NULL);