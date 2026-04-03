using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class jobmathTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CVId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplications_CV",
                        column: x => x.CVId,
                        principalTable: "CVs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_Job",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchPercentage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RequiredSkillCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MatchedSkillCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MatchedSkillsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: true),
                    MissingSkillsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationMatches", x => x.Id);
                    table.CheckConstraint("CK_MatchPercentage_Range", "[MatchPercentage] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_JobApplicationMatches_Application",
                        column: x => x.ApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationMatches_AppId_MatchPct",
                table: "JobApplicationMatches",
                columns: new[] { "ApplicationId", "MatchPercentage" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationMatches_ApplicationId",
                table: "JobApplicationMatches",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationMatches_CalculatedAt",
                table: "JobApplicationMatches",
                column: "CalculatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationMatches_MatchPercentage",
                table: "JobApplicationMatches",
                column: "MatchPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_AppliedAt",
                table: "JobApplications",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CVId",
                table: "JobApplications",
                column: "CVId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CVId_Status",
                table: "JobApplications",
                columns: new[] { "CVId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId",
                table: "JobApplications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_Status",
                table: "JobApplications",
                columns: new[] { "JobId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Status",
                table: "JobApplications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobApplicationMatches");

            migrationBuilder.DropTable(
                name: "JobApplications");
        }
    }
}
