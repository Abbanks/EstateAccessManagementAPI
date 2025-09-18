using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateAccessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldToAccessCodesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "AccessCodes");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeprecated",
                table: "AccessCodes",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AccessCodes",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentUses",
                table: "AccessCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AccessCodes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "AccessCodes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AccessCodes",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AccessCodes_CodeHash",
                table: "AccessCodes",
                column: "CodeHash");

            migrationBuilder.CreateIndex(
                name: "IX_AccessCodes_ExpiresAt",
                table: "AccessCodes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AccessCodes_IsActive",
                table: "AccessCodes",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccessCodes_CodeHash",
                table: "AccessCodes");

            migrationBuilder.DropIndex(
                name: "IX_AccessCodes_ExpiresAt",
                table: "AccessCodes");

            migrationBuilder.DropIndex(
                name: "IX_AccessCodes_IsActive",
                table: "AccessCodes");

            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "AccessCodes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AccessCodes");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeprecated",
                table: "AccessCodes",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AccessCodes",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentUses",
                table: "AccessCodes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AccessCodes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AccessCodes",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");
        }
    }
}
