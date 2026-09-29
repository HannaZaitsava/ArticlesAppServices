using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRootCommentToComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RootCommentId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_RootCommentId",
                table: "Comments",
                column: "RootCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_RootCommentId",
                table: "Comments",
                column: "RootCommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_RootCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_RootCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "RootCommentId",
                table: "Comments");
        }
    }
}
