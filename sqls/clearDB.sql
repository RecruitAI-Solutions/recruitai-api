-- ========================================================
-- CLEAR DATA SCRIPT
-- Xóa dữ liệu theo đúng thứ tự (tránh lỗi khóa ngoại)
-- ========================================================

-- Bước 1: Xóa các bảng có khóa ngoại tham chiếu đến nhiều bảng khác
DELETE FROM [JobApplicationMatches];
DELETE FROM [JobApplications];
DELETE FROM [SavedJobs];
DELETE FROM [Notifications];
DELETE FROM [PasswordResetToken];
DELETE FROM [RefreshTokens];

-- Bước 2: Xóa các bảng trung gian (nhiều-nhiều)
DELETE FROM [JobSkills];
DELETE FROM [CVAnalysisResult];

-- Bước 3: Xóa các bảng con (tham chiếu đến bảng chính)
DELETE FROM [AuthProviders];
DELETE FROM [CVs];

-- Bước 4: Xóa các bảng chính (theo thứ tự dependency)
DELETE FROM [Jobs];
DELETE FROM [Companies];
DELETE FROM [Users];
DELETE FROM [Skills];

-- Bước 5: Reset Identity (nếu cần)
-- DBCC CHECKIDENT ('[Skills]', RESEED, 0);
-- DBCC CHECKIDENT ('[Tests]', RESEED, 0);

-- Kiểm tra kết quả
SELECT 'Users' AS TableName, COUNT(*) AS RemainingRows FROM [Users] UNION ALL
SELECT 'Skills', COUNT(*) FROM [Skills] UNION ALL
SELECT 'Companies', COUNT(*) FROM [Companies] UNION ALL
SELECT 'Jobs', COUNT(*) FROM [Jobs] UNION ALL
SELECT 'AuthProviders', COUNT(*) FROM [AuthProviders] UNION ALL
SELECT 'CVs', COUNT(*) FROM [CVs] UNION ALL
SELECT 'JobSkills', COUNT(*) FROM [JobSkills] UNION ALL
SELECT 'CVAnalysisResults', COUNT(*) FROM [CVAnalysisResult] UNION ALL
SELECT 'JobApplications', COUNT(*) FROM [JobApplications] UNION ALL
SELECT 'JobApplicationMatches', COUNT(*) FROM [JobApplicationMatches] UNION ALL
SELECT 'SavedJobs', COUNT(*) FROM [SavedJobs] UNION ALL
SELECT 'Notifications', COUNT(*) FROM [Notifications] UNION ALL
SELECT 'RefreshTokens', COUNT(*) FROM [RefreshTokens] UNION ALL
SELECT 'PasswordResetTokens', COUNT(*) FROM [PasswordResetToken];