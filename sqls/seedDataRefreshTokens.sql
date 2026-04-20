-- Seed RefreshTokens (cho các user đã đăng nhập)
INSERT INTO [RefreshTokens] (Id, UserId, Token, ExpireAt, IsRevoked, CreatedByIp, CreatedAt, TokenType) VALUES
-- Admin
(NEWID(), '11111111-1111-1111-1111-111111111111', N'refresh_token_admin_1', DATEADD(DAY, 7, GETUTCDATE()), 0, N'127.0.0.1', GETUTCDATE(), 2),
(NEWID(), '11111111-1111-1111-1111-111111111111', N'refresh_token_admin_2', DATEADD(DAY, 5, GETUTCDATE()), 1, N'192.168.1.1', DATEADD(DAY, -2, GETUTCDATE()), 2),

-- Recruiter1
(NEWID(), '33333333-3333-3333-3333-333333333333', N'refresh_token_recruiter1', DATEADD(DAY, 7, GETUTCDATE()), 0, N'127.0.0.1', GETUTCDATE(), 2),

-- Candidate1
(NEWID(), '66666666-6666-6666-6666-666666666666', N'refresh_token_candidate1', DATEADD(DAY, 7, GETUTCDATE()), 0, N'127.0.0.1', GETUTCDATE(), 2),
(NEWID(), '66666666-6666-6666-6666-666666666666', N'refresh_token_candidate1_old', DATEADD(DAY, -3, GETUTCDATE()), 1, N'192.168.1.2', DATEADD(DAY, -10, GETUTCDATE()), 2),

-- Candidate2
(NEWID(), '77777777-7777-7777-7777-777777777777', N'refresh_token_candidate2', DATEADD(DAY, 7, GETUTCDATE()), 0, N'127.0.0.1', GETUTCDATE(), 2),

-- Candidate4
(NEWID(), '99999999-9999-9999-9999-999999999999', N'refresh_token_candidate4', DATEADD(DAY, 7, GETUTCDATE()), 0, N'127.0.0.1', GETUTCDATE(), 2);