using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerifyEmailForgotPassword.Migrations
{
    public partial class LinkDateAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "Link",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Link");
        }
    }
}
