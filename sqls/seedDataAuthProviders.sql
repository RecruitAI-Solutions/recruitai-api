-- Seed AuthProviders với BCrypt hash thật
INSERT INTO [AuthProviders] (Id, UserId, Provider, ProviderUserId, ProviderEmail, PasswordHash, CreatedAt, LastLoginAt) VALUES
(NEWID(), '11111111-1111-1111-1111-111111111111', 1, 'admin@recruitai.com', 'admin@recruitai.com', '$2a$11$jx43c9m7qUFCj7zUBQ8Y1OHDBpOPzYU0zhl1aNgY2q7a/xUMBoAzO', GETUTCDATE(), NULL),
(NEWID(), '22222222-2222-2222-2222-222222222222', 1, 'superadmin@recruitai.com', 'superadmin@recruitai.com', '$2a$11$jx43c9m7qUFCj7zUBQ8Y1OHDBpOPzYU0zhl1aNgY2q7a/xUMBoAzO', GETUTCDATE(), NULL),
(NEWID(), '33333333-3333-3333-3333-333333333333', 1, 'recruiter1@techcorp.com', 'recruiter1@techcorp.com', '$2a$11$yHZaLAmOcLsV0XoMg/0hP.5KYrTt/rw28g34cOcK538b3YITz2lCy', GETUTCDATE(), NULL),
(NEWID(), '44444444-4444-4444-4444-444444444444', 1, 'recruiter2@datasolution.com', 'recruiter2@datasolution.com', '$2a$11$yHZaLAmOcLsV0XoMg/0hP.5KYrTt/rw28g34cOcK538b3YITz2lCy', GETUTCDATE(), NULL),
(NEWID(), '55555555-5555-5555-5555-555555555555', 1, 'recruiter3@aistartup.com', 'recruiter3@aistartup.com', '$2a$11$yHZaLAmOcLsV0XoMg/0hP.5KYrTt/rw28g34cOcK538b3YITz2lCy', GETUTCDATE(), NULL),
(NEWID(), '66666666-6666-6666-6666-666666666666', 1, 'candidate1@gmail.com', 'candidate1@gmail.com', '$2a$11$VCyNLWHoA5o22iPEGGMWoOzhWnjNNsnup121IUmWkH.ZmkTtLc6C6', GETUTCDATE(), NULL),
(NEWID(), '77777777-7777-7777-7777-777777777777', 1, 'candidate2@gmail.com', 'candidate2@gmail.com', '$2a$11$VCyNLWHoA5o22iPEGGMWoOzhWnjNNsnup121IUmWkH.ZmkTtLc6C6', GETUTCDATE(), NULL),
(NEWID(), '88888888-8888-8888-8888-888888888888', 1, 'candidate3@gmail.com', 'candidate3@gmail.com', '$2a$11$VCyNLWHoA5o22iPEGGMWoOzhWnjNNsnup121IUmWkH.ZmkTtLc6C6', GETUTCDATE(), NULL),
(NEWID(), '99999999-9999-9999-9999-999999999999', 1, 'quaqduy@gmail.com', 'quaqduy@gmail.com', '$2a$11$57TiyL1t5uLoy8.xNytwDexPoNDAHVIBmg98Y1gPR8zLhegaA0Rru', GETUTCDATE(), NULL);