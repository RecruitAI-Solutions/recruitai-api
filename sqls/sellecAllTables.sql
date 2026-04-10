-- Xem tất cả dữ liệu từ các bảng chính (đã fix lỗi kiểu dữ liệu)
SELECT 'Users' AS TableName, CAST(Id AS nvarchar(36)) AS Id, Email, FullName, CAST(Role AS nvarchar(10)) AS RoleInfo, CAST(Status AS nvarchar(10)) AS StatusInfo, CAST(EmailVerified AS nvarchar(5)) AS Verified, CAST(CreatedAt AS nvarchar(50)) AS CreatedAt FROM Users
UNION ALL
SELECT 'AuthProviders', CAST(Id AS nvarchar(36)), CAST(UserId AS nvarchar(36)), CAST(Provider AS nvarchar(10)), ProviderUserId, ProviderEmail, NULL, CAST(CreatedAt AS nvarchar(50)) FROM AuthProviders
UNION ALL
SELECT 'CVs', CAST(Id AS nvarchar(36)), CAST(UserId AS nvarchar(36)), FileName, CAST(Status AS nvarchar(10)), NULL, NULL, CAST(UploadedAt AS nvarchar(50)) FROM CVs
UNION ALL
SELECT 'Jobs', CAST(Id AS nvarchar(36)), CAST(RecruiterId AS nvarchar(36)), Title, CAST(Status AS nvarchar(10)), CAST(IsActive AS nvarchar(5)), NULL, CAST(CreatedAt AS nvarchar(50)) FROM Jobs
UNION ALL
SELECT 'Skills', CAST(Id AS nvarchar(36)), Name, Category, NULL, NULL, NULL, CAST(CreatedAt AS nvarchar(50)) FROM Skills
UNION ALL
SELECT 'JobApplications', CAST(Id AS nvarchar(36)), CAST(JobId AS nvarchar(36)), CAST(CVId AS nvarchar(36)), CAST(Status AS nvarchar(10)), NULL, NULL, CAST(AppliedAt AS nvarchar(50)) FROM JobApplications
UNION ALL
SELECT 'JobApplicationMatches', CAST(Id AS nvarchar(36)), CAST(ApplicationId AS nvarchar(36)), CAST(MatchPercentage AS nvarchar(10)), NULL, NULL, NULL, CAST(CalculatedAt AS nvarchar(50)) FROM JobApplicationMatches;