-- Seed Companies (15 công ty - Việt Nam & nước ngoài)
INSERT INTO [Companies] (Id, Name, Slug, Logo, Address, Website, CreatedAt, CreatedBy) VALUES
-- Công ty Việt Nam (7)
('11111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA', N'Tech Solutions Vietnam', 'tech-solutions-vietnam', NULL, N'Tòa nhà ABC, Quận 1, TP.HCM', 'https://techsolutions.vn', GETUTCDATE(), '33333333-3333-3333-3333-333333333333'),
('22222222-BBBB-BBBB-BBBB-BBBBBBBBBBBB', N'Data Solutions', 'data-solutions', NULL, N'Tầng 10, Tòa nhà XYZ, Quận 3, TP.HCM', 'https://datasolutions.vn', GETUTCDATE(), '44444444-4444-4444-4444-444444444444'),
('33333333-CCCC-CCCC-CCCC-CCCCCCCCCCCC', N'AI Startup', 'ai-startup', NULL, N'Tầng 5, Tòa nhà DEF, Quận 7, TP.HCM', 'https://aistartup.vn', GETUTCDATE(), '55555555-5555-5555-5555-555555555555'),
('44444444-DDDD-DDDD-DDDD-DDDDDDDDDDDD', N'FPT Software', 'fpt-software', NULL, N'Tòa nhà FPT, Quận 9, TP.HCM', 'https://fptsoftware.com', GETUTCDATE(), '33333333-3333-3333-3333-333333333333'),
('55555555-EEEE-EEEE-EEEE-EEEEEEEEEEEE', N'Viettel Solutions', 'viettel-solutions', NULL, N'Tòa nhà Viettel, Quận Cầu Giấy, Hà Nội', 'https://viettelsolutions.vn', GETUTCDATE(), '44444444-4444-4444-4444-444444444444'),
('66666666-FFFF-FFFF-FFFF-FFFFFFFFFFFF', N'VNG Corporation', 'vng-corp', NULL, N'Tòa nhà VNG, Quận 12, TP.HCM', 'https://vng.com.vn', GETUTCDATE(), '55555555-5555-5555-5555-555555555555'),
('77777777-1111-1111-1111-111111111111', N'Tiki Solutions', 'tiki-solutions', NULL, N'Tòa nhà Tiki, Quận 2, TP.HCM', 'https://tiki.vn', GETUTCDATE(), '33333333-3333-3333-3333-333333333333'),

-- Công ty nước ngoài (8)
('88888888-2222-2222-2222-222222222222', N'Google Vietnam', 'google-vietnam', NULL, N'Tòa nhà Landmark 81, Quận 1, TP.HCM', 'https://google.com.vn', GETUTCDATE(), '44444444-4444-4444-4444-444444444444'),
('99999999-3333-3333-3333-333333333333', N'Microsoft Vietnam', 'microsoft-vietnam', NULL, N'Tòa nhà E.Town Central, Quận 11, TP.HCM', 'https://microsoft.com.vn', GETUTCDATE(), '55555555-5555-5555-5555-555555555555'),
('AAAAAAAA-4444-4444-4444-444444444444', N'Amazon Web Services', 'aws-vietnam', NULL, N'Tòa nhà Deutsches Haus, Quận 1, TP.HCM', 'https://aws.amazon.com/vn', GETUTCDATE(), '33333333-3333-3333-3333-333333333333'),
('BBBBBBBB-5555-5555-5555-555555555555', N'Meta Vietnam', 'meta-vietnam', NULL, N'Tòa nhà Pearl Plaza, Quận Bình Thạnh, TP.HCM', 'https://meta.com/vn', GETUTCDATE(), '44444444-4444-4444-4444-444444444444'),
('CCCCCCCC-6666-6666-6666-666666666666', N'Apple Vietnam', 'apple-vietnam', NULL, N'Tòa nhà Sunwah, Quận 1, TP.HCM', 'https://apple.com/vn', GETUTCDATE(), '55555555-5555-5555-5555-555555555555'),
('DDDDDDDD-7777-7777-7777-777777777777', N'Netflix Vietnam', 'netflix-vietnam', NULL, N'Tòa nhà The Nexus, Quận 1, TP.HCM', 'https://netflix.com/vn', GETUTCDATE(), '33333333-3333-3333-3333-333333333333'),
('EEEEEEEE-8888-8888-8888-888888888888', N'Shopee Vietnam', 'shopee-vietnam', NULL, N'Tòa nhà Saigon Centre, Quận 1, TP.HCM', 'https://shopee.vn', GETUTCDATE(), '44444444-4444-4444-4444-444444444444'),
('FFFFFFFF-9999-9999-9999-999999999999', N'Lazada Vietnam', 'lazada-vietnam', NULL, N'Tòa nhà Harbour View, Quận 4, TP.HCM', 'https://lazada.vn', GETUTCDATE(), '55555555-5555-5555-5555-555555555555');