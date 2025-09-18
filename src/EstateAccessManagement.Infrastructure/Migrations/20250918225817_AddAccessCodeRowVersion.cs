using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateAccessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessCodeRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AccessCodes",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AccessCodes");
        }
    }
}
