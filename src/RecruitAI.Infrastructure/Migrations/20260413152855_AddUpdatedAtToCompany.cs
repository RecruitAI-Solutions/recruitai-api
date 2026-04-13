using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Companies",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Companies");
        }
    }
}
