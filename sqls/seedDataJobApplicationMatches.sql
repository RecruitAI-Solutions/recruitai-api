-- Seed JobApplicationMatches với ApplicationId thật
INSERT INTO [JobApplicationMatches] (Id, ApplicationId, MatchPercentage, RequiredSkillCount, MatchedSkillCount, MatchedSkillsJson, MissingSkillsJson, CalculatedAt) VALUES
(NEWID(), '5116dd3f-b022-45e7-9e9d-ca5af983e13e', 85, 5, 4, N'["C#",".NET Core","SQL Server","REST API"]', N'["ASP.NET Core"]', GETUTCDATE()),
(NEWID(), '609f0e14-7116-4c81-af29-306f5219cdfa', 75, 4, 3, N'["Python","SQL","MongoDB"]', N'["Communication"]', GETUTCDATE()),
(NEWID(), '3e1286a9-deb3-4f2e-b4e2-4aba8668ec88', 90, 4, 4, N'["React","JavaScript","TypeScript","HTML5"]', N'[]', GETUTCDATE()),
(NEWID(), '394d9a50-2ead-4bff-b09e-6968a14a4bbf', 70, 3, 2, N'["React","JavaScript"]', N'["Node.js"]', GETUTCDATE()),
(NEWID(), '5e422982-7a17-431c-9e70-fc0db5b78a6d', 60, 4, 2, N'["Python","SQL"]', N'["TensorFlow","AWS"]', GETUTCDATE()),
(NEWID(), '991dd3e4-8abc-4366-9bff-5406a7e3698c', 80, 4, 3, N'["Python","SQL","Communication"]', N'["Problem Solving"]', GETUTCDATE()),
(NEWID(), '7d7ad027-e613-4e96-adff-c60a76f89cf2', 88, 4, 3, N'["React","JavaScript","REST API"]', N'["TypeScript"]', GETUTCDATE()),
(NEWID(), '9ed71c70-a99a-4ed6-b594-2c44f7d6a4cf', 65, 3, 2, N'["Communication","Problem Solving"]', N'["Figma"]', GETUTCDATE()),
(NEWID(), '4593d91a-a2ad-44de-9f37-aeef98a1fbaa', 95, 4, 4, N'["AWS","Docker","Kubernetes","Terraform"]', N'[]', GETUTCDATE()),
(NEWID(), 'a9bdfa50-1264-4d1d-a6f9-84cb09bda0aa', 55, 4, 2, N'["Communication","SQL"]', N'["Problem Solving","Customer Service"]', GETUTCDATE());