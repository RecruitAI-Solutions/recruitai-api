-- ============================================
-- Database: RecruitAI
-- Script tạo toàn bộ bảng
-- ============================================

-- Bảng Users
CREATE TABLE [Users] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Email] NVARCHAR(256) NOT NULL,
    [FullName] NVARCHAR(100) NOT NULL,
    [Role] INT NOT NULL DEFAULT 0,  -- 0: CANDIDATE, ...
    [Status] INT NOT NULL DEFAULT 0, -- 0: PendingVerification
    [EmailVerified] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME NULL,
    [LastLoginAt] DATETIME NULL,
    [Gender] INT NULL,
    [DateOfBirth] DATETIME NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [AvatarUrl] NVARCHAR(500) NULL,
    [PermissionCodes] NVARCHAR(1000) NULL
);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

-- Bảng AuthProviders
CREATE TABLE [AuthProviders] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Provider] INT NOT NULL,
    [ProviderUserId] NVARCHAR(255) NOT NULL,
    [ProviderEmail] NVARCHAR(256) NULL,
    [PasswordHash] NVARCHAR(255) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [LastLoginAt] DATETIME NULL,
    CONSTRAINT [FK_AuthProviders_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_AuthProviders_Provider_ProviderUserId] ON [AuthProviders] ([Provider], [ProviderUserId]);

-- Bảng Companies
CREATE TABLE [Companies] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(200) NOT NULL,
    [Slug] NVARCHAR(200) NULL,
    [Logo] NVARCHAR(500) NULL,
    [Address] NVARCHAR(500) NULL,
    [Website] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME NULL,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [FK_Companies_Creator] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_Companies_Name] ON [Companies] ([Name]);
CREATE UNIQUE INDEX [IX_Companies_Slug] ON [Companies] ([Slug]);

-- Bảng Jobs
CREATE TABLE [Jobs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RecruiterId] UNIQUEIDENTIFIER NOT NULL,
    [CompanyId] UNIQUEIDENTIFIER NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [Requirements] NVARCHAR(4000) NULL,
    [Location] NVARCHAR(255) NOT NULL,
    [SalaryMin] DECIMAL(18,2) NULL,
    [SalaryMax] DECIMAL(18,2) NULL,
    [Currency] INT NOT NULL DEFAULT 0,  -- 0: VND
    [EmploymentType] INT NULL,
    [ExperienceLevel] INT NULL,
    [Department] NVARCHAR(255) NOT NULL DEFAULT '',
    [Benefits] NVARCHAR(2000) NULL,
    [ExpirationDate] DATETIME NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME NULL,
    [Status] INT NOT NULL DEFAULT 0,  -- 0: Draft
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Views] INT NOT NULL DEFAULT 0,
    [Applications] INT NOT NULL DEFAULT 0,
    [IsFeatured] BIT NOT NULL DEFAULT 0,
    [FeaturedOrder] INT NULL,
    CONSTRAINT [FK_Jobs_Recruiter] FOREIGN KEY ([RecruiterId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Jobs_Company] FOREIGN KEY ([CompanyId]) REFERENCES [Companies]([Id]) ON DELETE SET NULL
);

CREATE INDEX [IX_Jobs_RecruiterId] ON [Jobs] ([RecruiterId]);
CREATE INDEX [IX_Jobs_Title] ON [Jobs] ([Title]);
CREATE INDEX [IX_Jobs_Location] ON [Jobs] ([Location]);
CREATE INDEX [IX_Jobs_EmploymentType] ON [Jobs] ([EmploymentType]);
CREATE INDEX [IX_Jobs_ExperienceLevel] ON [Jobs] ([ExperienceLevel]);
CREATE INDEX [IX_Jobs_Status] ON [Jobs] ([Status]);
CREATE INDEX [IX_Jobs_CreatedAt] ON [Jobs] ([CreatedAt]);
CREATE INDEX [IX_Jobs_ExpirationDate] ON [Jobs] ([ExpirationDate]);
CREATE INDEX [IX_Jobs_IsDeleted] ON [Jobs] ([IsDeleted]);
CREATE INDEX [IX_Jobs_IsFeatured] ON [Jobs] ([IsFeatured]);

-- Bảng Skills
CREATE TABLE [Skills] (
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Category] NVARCHAR(50) NULL,
    [Aliases] NVARCHAR(500) NULL,
    [ContextKeywords] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME NULL,
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedBy] NVARCHAR(100) NULL
);

CREATE UNIQUE INDEX [IX_Skills_Name] ON [Skills] ([Name]);
CREATE INDEX [IX_Skills_Category] ON [Skills] ([Category]);
CREATE INDEX [IX_Skills_IsActive] ON [Skills] ([IsActive]);

-- Bảng JobSkills (bảng trung gian Job - Skill)
CREATE TABLE [JobSkills] (
    [JobId] UNIQUEIDENTIFIER NOT NULL,
    [SkillId] INT NOT NULL,
    [IsRequired] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_JobSkills] PRIMARY KEY ([JobId], [SkillId]),
    CONSTRAINT [FK_JobSkills_Job] FOREIGN KEY ([JobId]) REFERENCES [Jobs]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_JobSkills_Skill] FOREIGN KEY ([SkillId]) REFERENCES [Skills]([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_JobSkills_SkillId] ON [JobSkills] ([SkillId]);

-- Bảng CVs
CREATE TABLE [CVs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [FileName] NVARCHAR(255) NOT NULL,
    [StoredFileName] NVARCHAR(255) NOT NULL,
    [FilePath] NVARCHAR(500) NOT NULL,
    [FileSize] BIGINT NOT NULL DEFAULT 0,
    [ContentType] NVARCHAR(100) NOT NULL,
    [Status] INT NOT NULL DEFAULT 0,  -- 0: Pending
    [UploadedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [ProcessedAt] DATETIME NULL,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME NULL,
    [ExtractedText] NVARCHAR(MAX) NULL,
    [AnalyzedAt] DATETIME NULL,
    CONSTRAINT [FK_CVs_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_CVs_UserId] ON [CVs] ([UserId]);
CREATE INDEX [IX_CVs_Status] ON [CVs] ([Status]);

-- Bảng CVAnalysisResults
CREATE TABLE [CVAnalysisResults] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [CVId] UNIQUEIDENTIFIER NOT NULL,
    [SkillId] INT NOT NULL,
    [Confidence] DECIMAL(18,4) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_CVAnalysisResults_CV] FOREIGN KEY ([CVId]) REFERENCES [CVs]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CVAnalysisResults_Skill] FOREIGN KEY ([SkillId]) REFERENCES [Skills]([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_CVAnalysisResults_CVId] ON [CVAnalysisResults] ([CVId]);
CREATE INDEX [IX_CVAnalysisResults_SkillId] ON [CVAnalysisResults] ([SkillId]);
CREATE UNIQUE INDEX [IX_CVAnalysisResults_CVId_SkillId] ON [CVAnalysisResults] ([CVId], [SkillId]);

-- Bảng JobApplications
CREATE TABLE [JobApplications] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [JobId] UNIQUEIDENTIFIER NOT NULL,
    [CVId] UNIQUEIDENTIFIER NOT NULL,
    [Status] INT NOT NULL DEFAULT 0,  -- 0: Pending
    [AppliedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [ReviewedAt] DATETIME NULL,
    [Notes] NVARCHAR(2000) NULL,
    CONSTRAINT [FK_JobApplications_Job] FOREIGN KEY ([JobId]) REFERENCES [Jobs]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_JobApplications_CV] FOREIGN KEY ([CVId]) REFERENCES [CVs]([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_JobApplications_JobId] ON [JobApplications] ([JobId]);
CREATE INDEX [IX_JobApplications_CVId] ON [JobApplications] ([CVId]);
CREATE INDEX [IX_JobApplications_Status] ON [JobApplications] ([Status]);
CREATE INDEX [IX_JobApplications_AppliedAt] ON [JobApplications] ([AppliedAt]);
CREATE INDEX [IX_JobApplications_JobId_Status] ON [JobApplications] ([JobId], [Status]);
CREATE INDEX [IX_JobApplications_CVId_Status] ON [JobApplications] ([CVId], [Status]);

-- Bảng JobApplicationMatches
CREATE TABLE [JobApplicationMatches] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [ApplicationId] UNIQUEIDENTIFIER NOT NULL,
    [MatchPercentage] INT NOT NULL DEFAULT 0,
    [RequiredSkillCount] INT NOT NULL DEFAULT 0,
    [MatchedSkillCount] INT NOT NULL DEFAULT 0,
    [MatchedSkillsJson] NVARCHAR(MAX) NULL,
    [MissingSkillsJson] NVARCHAR(MAX) NULL,
    [CalculatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_JobApplicationMatches_Application] FOREIGN KEY ([ApplicationId]) REFERENCES [JobApplications]([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_JobApplicationMatches_ApplicationId] ON [JobApplicationMatches] ([ApplicationId]);
CREATE INDEX [IX_JobApplicationMatches_MatchPercentage] ON [JobApplicationMatches] ([MatchPercentage]);
CREATE INDEX [IX_JobApplicationMatches_CalculatedAt] ON [JobApplicationMatches] ([CalculatedAt]);
CREATE INDEX [IX_JobApplicationMatches_AppId_MatchPct] ON [JobApplicationMatches] ([ApplicationId], [MatchPercentage]);

-- CHECK CONSTRAINT cho MatchPercentage (0-100)
ALTER TABLE [JobApplicationMatches] ADD CONSTRAINT [CK_MatchPercentage_Range] CHECK ([MatchPercentage] BETWEEN 0 AND 100);

-- Bảng SavedJobs
CREATE TABLE [SavedJobs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [JobId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [SavedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_SavedJobs_Job] FOREIGN KEY ([JobId]) REFERENCES [Jobs]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SavedJobs_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_SavedJobs_JobId_UserId] ON [SavedJobs] ([JobId], [UserId]);

-- Bảng Notifications
CREATE TABLE [Notifications] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Content] NVARCHAR(2000) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL,
    [IsRead] BIT NOT NULL DEFAULT 0,
    [Data] NVARCHAR(2000) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Notifications_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);

-- Bảng RefreshTokens
CREATE TABLE [RefreshTokens] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Token] NVARCHAR(500) NOT NULL,
    [ExpireAt] DATETIME NOT NULL,
    [IsRevoked] BIT NOT NULL DEFAULT 0,
    [ReplacedByToken] NVARCHAR(500) NULL,
    [CreatedByIp] NVARCHAR(50) NULL,
    [RevokedByIp] NVARCHAR(50) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [RevokedAt] DATETIME NULL,
    [TokenType] INT NOT NULL DEFAULT 0,  -- 0: RefreshToken
    CONSTRAINT [FK_RefreshTokens_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
CREATE INDEX [IX_RefreshTokens_ExpireAt] ON [RefreshTokens] ([ExpireAt]);

-- Bảng PasswordResetTokens
CREATE TABLE [PasswordResetTokens] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Token] NVARCHAR(500) NOT NULL,
    [ExpiryDate] DATETIME NOT NULL,
    [IsUsed] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UsedAt] DATETIME NULL,
    [CreatedByIp] NVARCHAR(50) NULL,
    CONSTRAINT [FK_PasswordResetTokens_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_PasswordResetTokens_Token] ON [PasswordResetTokens] ([Token]);
CREATE INDEX [IX_PasswordResetTokens_ExpiryDate] ON [PasswordResetTokens] ([ExpiryDate]);

-- Bảng AuditLogs
CREATE TABLE [AuditLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [EntityType] INT NOT NULL,
    [Action] INT NOT NULL,
    [EntityId] NVARCHAR(50) NOT NULL,
    [EntityName] NVARCHAR(256) NOT NULL,
    [OldValue] NVARCHAR(MAX) NULL,
    [NewValue] NVARCHAR(MAX) NULL,
    [Reason] NVARCHAR(500) NULL,
    [ChangedBy] NVARCHAR(256) NOT NULL,
    [ChangedByIp] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [RequestId] NVARCHAR(100) NULL,
    [ChangedAt] DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
CREATE INDEX [IX_AuditLogs_ChangedAt] ON [AuditLogs] ([ChangedAt]);
CREATE INDEX [IX_AuditLogs_Action] ON [AuditLogs] ([Action]);
CREATE INDEX [IX_AuditLogs_ChangedBy] ON [AuditLogs] ([ChangedBy]);

-- Bảng Tests (từ file TestConfiguration.cs)
CREATE TABLE [Tests] (
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [FirstName] NVARCHAR(50) NOT NULL,
    [LastName] NVARCHAR(50) NOT NULL
);