using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerifyEmailForgotPassword.Migrations
{
    public partial class Itinalize4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Favorites_Favorites_FavoriteId",
                table: "User_Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Favorites_Users_UserId",
                table: "User_Favorites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User_Favorites",
                table: "User_Favorites");

            migrationBuilder.RenameTable(
                name: "User_Favorites",
                newName: "UserFavorites");

            migrationBuilder.RenameIndex(
                name: "IX_User_Favorites_UserId",
                table: "UserFavorites",
                newName: "IX_UserFavorites_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_User_Favorites_FavoriteId",
                table: "UserFavorites",
                newName: "IX_UserFavorites_FavoriteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_Favorites_FavoriteId",
                table: "UserFavorites",
                column: "FavoriteId",
                principalTable: "Favorites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_Users_UserId",
                table: "UserFavorites",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_Favorites_FavoriteId",
                table: "UserFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_Users_UserId",
                table: "UserFavorites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites");

            migrationBuilder.RenameTable(
                name: "UserFavorites",
                newName: "User_Favorites");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorites_UserId",
                table: "User_Favorites",
                newName: "IX_User_Favorites_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorites_FavoriteId",
                table: "User_Favorites",
                newName: "IX_User_Favorites_FavoriteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User_Favorites",
                table: "User_Favorites",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Favorites_Favorites_FavoriteId",
                table: "User_Favorites",
                column: "FavoriteId",
                principalTable: "Favorites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Favorites_Users_UserId",
                table: "User_Favorites",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
