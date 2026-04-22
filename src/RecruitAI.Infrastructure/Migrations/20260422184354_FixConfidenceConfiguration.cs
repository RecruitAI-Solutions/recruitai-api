using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixConfidenceConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CVAnalysisResult_Skills_SkillId",
                table: "CVAnalysisResult");

            migrationBuilder.RenameIndex(
                name: "IX_CVAnalysisResult_SkillId",
                table: "CVAnalysisResult",
                newName: "IX_CVAnalysisResults_SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_CVAnalysisResult_CVId",
                table: "CVAnalysisResult",
                newName: "IX_CVAnalysisResults_CVId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CVAnalysisResult",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_CVAnalysisResults_CVId_SkillId",
                table: "CVAnalysisResult",
                columns: new[] { "CVId", "SkillId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CVAnalysisResult_Skills_SkillId",
                table: "CVAnalysisResult",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CVAnalysisResult_Skills_SkillId",
                table: "CVAnalysisResult");

            migrationBuilder.DropIndex(
                name: "IX_CVAnalysisResults_CVId_SkillId",
                table: "CVAnalysisResult");

            migrationBuilder.RenameIndex(
                name: "IX_CVAnalysisResults_SkillId",
                table: "CVAnalysisResult",
                newName: "IX_CVAnalysisResult_SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_CVAnalysisResults_CVId",
                table: "CVAnalysisResult",
                newName: "IX_CVAnalysisResult_CVId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CVAnalysisResult",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_CVAnalysisResult_Skills_SkillId",
                table: "CVAnalysisResult",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
