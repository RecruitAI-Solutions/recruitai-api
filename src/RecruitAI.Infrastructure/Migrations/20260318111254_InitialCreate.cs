using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecruitAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Aliases = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContextKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tests__3214EC071C177EA5", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PermissionCodes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    ProviderUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProviderEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthProviders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CVs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CVs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecruiterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SalaryMin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryMax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    EmploymentType = table.Column<int>(type: "int", nullable: true),
                    ExperienceLevel = table.Column<int>(type: "int", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Benefits = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    Applications = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Users_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetToken_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TokenType = table.Column<int>(type: "int", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Aliases", "Category", "ContextKeywords", "CreatedAt", "CreatedBy", "IsActive", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "CSharp,C Sharp", "Programming Language", "c#,csharp,c sharp", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "C#", null, null },
                    { 2, null, "Programming Language", "java,java 8,java 11", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Java", null, null },
                    { 3, "py", "Programming Language", "python,py,django", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Python", null, null },
                    { 4, "js", "Programming Language", "javascript,js,ecmascript", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "JavaScript", null, null },
                    { 5, "ts", "Programming Language", "typescript,ts", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "TypeScript", null, null },
                    { 6, "structured query language", "Programming Language", "sql,tsql,plsql", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "SQL", null, null },
                    { 7, "golang", "Programming Language", "go,golang", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Go", null, null },
                    { 8, null, "Programming Language", "rust", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Rust", null, null },
                    { 9, null, "Programming Language", "php,laravel", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "PHP", null, null },
                    { 10, null, "Programming Language", "swift,ios", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Swift", null, null },
                    { 11, null, "Programming Language", "kotlin,android", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Kotlin", null, null },
                    { 12, null, "Programming Language", "ruby,rails", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Ruby", null, null },
                    { 13, "dotnet core", "Framework", ".net core,asp.net core", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, ".NET Core", null, null },
                    { 14, "reactjs", "Framework", "react,reactjs,react.js", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "React", null, null },
                    { 15, "angularjs", "Framework", "angular,angular 2+", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Angular", null, null },
                    { 16, "vue", "Framework", "vue,vuejs", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Vue.js", null, null },
                    { 17, "spring", "Framework", "spring,spring boot", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Spring Boot", null, null },
                    { 18, "node", "Framework", "node,nodejs", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Node.js", null, null },
                    { 19, null, "Framework", "django,python web", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Django", null, null },
                    { 20, null, "Framework", "flask", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Flask", null, null },
                    { 21, "express", "Framework", "express,expressjs", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Express.js", null, null },
                    { 22, "ef", "Framework", "entity framework,ef core", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Entity Framework", null, null },
                    { 23, null, "Framework", "hibernate,jpa", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Hibernate", null, null },
                    { 24, "mssql", "Database", "sql server,mssql", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "SQL Server", null, null },
                    { 25, null, "Database", "mysql", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "MySQL", null, null },
                    { 26, "postgres", "Database", "postgresql,postgres", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "PostgreSQL", null, null },
                    { 27, "mongo", "Database", "mongodb,mongo", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "MongoDB", null, null },
                    { 28, null, "Database", "redis,cache", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Redis", null, null },
                    { 29, "es", "Database", "elasticsearch,es", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Elasticsearch", null, null },
                    { 30, null, "Database", "oracle", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Oracle", null, null },
                    { 31, null, "Database", "cassandra", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Cassandra", null, null },
                    { 32, "dynamo", "Database", "dynamodb,aws dynamo", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "DynamoDB", null, null },
                    { 33, "microsoft azure", "Cloud", "azure,azure devops", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Azure", null, null },
                    { 34, "amazon web services", "Cloud", "aws,ec2,s3,lambda", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "AWS", null, null },
                    { 35, "gcp", "Cloud", "gcp,google cloud", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Google Cloud", null, null },
                    { 36, null, "DevOps", "docker,container", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Docker", null, null },
                    { 37, "k8s", "DevOps", "kubernetes,k8s", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Kubernetes", null, null },
                    { 38, null, "DevOps", "jenkins,ci/cd", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Jenkins", null, null },
                    { 39, null, "DevOps", "git,github,gitlab", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Git", null, null },
                    { 40, null, "DevOps", "terraform,iac", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Terraform", null, null },
                    { 41, null, "DevOps", "ansible", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Ansible", null, null },
                    { 42, "collaboration", "Soft Skill", "teamwork,team work", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Teamwork", null, null },
                    { 43, null, "Soft Skill", "communication,verbal,written", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Communication", null, null },
                    { 44, "analytical", "Soft Skill", "problem solving,analytical", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Problem Solving", null, null },
                    { 45, null, "Soft Skill", "leadership,lead", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Leadership", null, null },
                    { 46, null, "Soft Skill", "time management,organize", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Time Management", null, null },
                    { 47, null, "Language", "english,ielts,toeic", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "English", null, null },
                    { 48, "tiếng việt", "Language", "vietnamese,tieng viet", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Vietnamese", null, null },
                    { 49, "日本語", "Language", "japanese,nihongo,jlpt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Japanese", null, null },
                    { 50, "mandarin", "Language", "chinese,mandarin", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Chinese", null, null },
                    { 51, "한국어", "Language", "korean,topik", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, "Korean", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthProviders_Provider_ProviderUserId",
                table: "AuthProviders",
                columns: new[] { "Provider", "ProviderUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthProviders_UserId",
                table: "AuthProviders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CVs_Status",
                table: "CVs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CVs_UserId",
                table: "CVs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CreatedAt",
                table: "Jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_EmploymentType",
                table: "Jobs",
                column: "EmploymentType");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ExperienceLevel",
                table: "Jobs",
                column: "ExperienceLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ExpirationDate",
                table: "Jobs",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_IsDeleted",
                table: "Jobs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Location",
                table: "Jobs",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RecruiterId",
                table: "Jobs",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Title",
                table: "Jobs",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_UserId",
                table: "PasswordResetToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_ExpiryDate",
                table: "PasswordResetToken",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetToken",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpireAt",
                table: "RefreshTokens",
                column: "ExpireAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Category",
                table: "Skills",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_IsActive",
                table: "Skills",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthProviders");

            migrationBuilder.DropTable(
                name: "CVs");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "PasswordResetToken");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
