using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPhotoBooth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoritePhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoritePhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritePhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoritePhotos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritePhotos_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UserId_UploadedAt",
                table: "Photos",
                columns: new[] { "UserId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePhotos_PhotoId",
                table: "FavoritePhotos",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePhotos_UserId",
                table: "FavoritePhotos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePhotos_UserId_PhotoId",
                table: "FavoritePhotos",
                columns: new[] { "UserId", "PhotoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_AspNetUsers_UserId",
                table: "GroupMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_AspNetUsers_UserId",
                table: "GroupMembers");

            migrationBuilder.DropTable(
                name: "FavoritePhotos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_UserId_UploadedAt",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers");
        }
    }
}
