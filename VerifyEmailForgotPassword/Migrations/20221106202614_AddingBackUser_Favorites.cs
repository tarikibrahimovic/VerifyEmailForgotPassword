using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerifyEmailForgotPassword.Migrations
{
    public partial class AddingBackUser_Favorites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoritesUser");

            migrationBuilder.CreateTable(
                name: "User_Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FavoritesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Favorites_Favorites_FavoritesId",
                        column: x => x.FavoritesId,
                        principalTable: "Favorites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Favorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Favorites_FavoritesId",
                table: "User_Favorites",
                column: "FavoritesId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Favorites_UserId",
                table: "User_Favorites",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User_Favorites");

            migrationBuilder.CreateTable(
                name: "FavoritesUser",
                columns: table => new
                {
                    FavoritesId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritesUser", x => new { x.FavoritesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_FavoritesUser_Favorites_FavoritesId",
                        column: x => x.FavoritesId,
                        principalTable: "Favorites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritesUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoritesUser_UsersId",
                table: "FavoritesUser",
                column: "UsersId");
        }
    }
}
