using Microsoft.EntityFrameworkCore.Migrations;
using System.Security.Cryptography;

#nullable disable

namespace RecruitAI.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class AddUserFieldsAndEnums : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "IsActive",
				table: "Users");

			migrationBuilder.AddColumn<string>(
				name: "AvatarUrl",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "DateOfBirth",
				table: "Users",
				type: "datetime2",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "Gender",
				table: "Users",
				type: "int",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "PhoneNumber",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "Status",
				table: "Users",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "TokenType",
				table: "RefreshTokens",
				type: "int",
				nullable: false,
				defaultValue: 0);

			// 1. Xóa index phụ thuộc vào cột Provider
			migrationBuilder.DropIndex(
				name: "IX_AuthProviders_Provider_ProviderUserId",
				table: "AuthProviders");

			// 1. Tạo cột tạm KIỂU STRING để giữ giá trị cũ
			migrationBuilder.AddColumn<string>(
				name: "ProviderTemp",
				table: "AuthProviders",
				type: "nvarchar(50)",
				nullable: true);

			// 2. Copy dữ liệu từ cột Provider cũ sang cột tạm
			migrationBuilder.Sql(@"
				UPDATE [AuthProviders] 
				SET [ProviderTemp] = [Provider]
			");

			// 3. Chuyển đổi dữ liệu từ string sang số dựa trên enum
			//    (Điều chỉnh giá trị cho khớp với enum của bạn)
			migrationBuilder.Sql(@"
				UPDATE [AuthProviders]
				SET [ProviderTemp] = CASE [ProviderTemp]
					WHEN 'email' THEN '1'
					WHEN 'facebook' THEN '2'
					WHEN 'google' THEN '3'
					WHEN 'github' THEN '4'
					WHEN 'microsoft' THEN '5'
					ELSE NULL
				END
			");

			// 4. Xóa cột Provider cũ
			migrationBuilder.DropColumn(
				name: "Provider",
				table: "AuthProviders");

			// 5. Đổi kiểu cột tạm thành int và đổi tên
			migrationBuilder.AlterColumn<int>(
				name: "ProviderTemp",
				table: "AuthProviders",
				type: "int",
				nullable: false,
				defaultValue: 0);

			// 6. Đổi tên cột tạm thành Provider
			migrationBuilder.RenameColumn(
				name: "ProviderTemp",
				table: "AuthProviders",
				newName: "Provider");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "AvatarUrl",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "DateOfBirth",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "Gender",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "PhoneNumber",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "Status",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "TokenType",
				table: "RefreshTokens");

			migrationBuilder.AddColumn<bool>(
				name: "IsActive",
				table: "Users",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AlterColumn<string>(
				name: "Provider",
				table: "AuthProviders",
				type: "nvarchar(50)",
				maxLength: 50,
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int",
				oldMaxLength: 50);

			migrationBuilder.DropIndex(
			   name: "IX_AuthProviders_Provider_ProviderUserId",
			   table: "AuthProviders");
			}
		
	}
}
