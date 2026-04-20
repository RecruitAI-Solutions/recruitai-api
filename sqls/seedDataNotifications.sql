-- Seed Notifications
INSERT INTO [Notifications] (Id, UserId, Title, Content, Type, IsRead, Data, CreatedAt) VALUES
-- Candidate1
(NEWID(), '66666666-6666-6666-6666-666666666666', N'Đơn ứng tuyển đã được xem', N'Nhà tuyển dụng đã xem CV của bạn cho vị trí .NET Backend Developer', N'application_update', 0, NULL, GETUTCDATE()),
(NEWID(), '66666666-6666-6666-6666-666666666666', N'Công việc mới phù hợp', N'Có 3 công việc mới phù hợp với kỹ năng của bạn', N'job_match', 1, N'{"jobCount":3}', DATEADD(DAY, -2, GETUTCDATE())),

-- Candidate2
(NEWID(), '77777777-7777-7777-7777-777777777777', N'Đơn ứng tuyển được chấp nhận', N'Chúc mừng! Bạn đã được chấp nhận cho vị trí Frontend React', N'application_update', 0, NULL, GETUTCDATE()),
(NEWID(), '77777777-7777-7777-7777-777777777777', N'Lời mời phỏng vấn', N'Công ty Tech Solutions mời bạn tham gia phỏng vấn vào ngày 25/04', N'system', 0, N'{"interviewDate":"2026-04-25"}', GETUTCDATE()),

-- Candidate3
(NEWID(), '88888888-8888-8888-8888-888888888888', N'Kết quả phân tích CV', N'CV của bạn đã được phân tích. Điểm phù hợp với AI/ML Engineer là 60%', N'cv_processed', 1, N'{"matchPercentage":60}', DATEADD(DAY, -1, GETUTCDATE())),
(NEWID(), '88888888-8888-8888-8888-888888888888', N'Đơn ứng tuyển bị từ chối', N'Rất tiếc, đơn ứng tuyển AI/ML Engineer của bạn chưa phù hợp', N'application_update', 0, NULL, GETUTCDATE()),

-- Candidate4
(NEWID(), '99999999-9999-9999-9999-999999999999', N'Chào mừng đến với RecruitAI', N'Chào mừng bạn đã đăng ký tài khoản thành công!', N'system', 1, NULL, DATEADD(DAY, -5, GETUTCDATE())),
(NEWID(), '99999999-9999-9999-9999-999999999999', N'Cập nhật hồ sơ', N'Vui lòng hoàn thiện hồ sơ của bạn để tăng cơ hội việc làm', N'system', 0, NULL, DATEADD(DAY, -3, GETUTCDATE())),

-- Recruiter1
(NEWID(), '33333333-3333-3333-3333-333333333333', N'Có ứng viên mới', N'Có 2 ứng viên mới ứng tuyển vào vị trí .NET Backend Developer', N'application_update', 0, NULL, GETUTCDATE()),

-- Recruiter2
(NEWID(), '44444444-4444-4444-4444-444444444444', N'Công việc sắp hết hạn', N'Công việc Data Engineer của bạn sẽ hết hạn sau 3 ngày', N'job_expiring', 0, N'{"daysLeft":3}', GETUTCDATE());