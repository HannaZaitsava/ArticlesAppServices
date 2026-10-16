using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorToArticleAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_AspNetUsers_CreatedBy",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_CreatedBy",
                table: "Comments");          

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "AspNetUsers",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Comments",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_CreatedBy",
                table: "Comments",
                newName: "IX_Comments_CreatorId");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Articles",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_CreatedBy",
                table: "Articles",
                newName: "IX_Articles_CreatorId");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
                      
            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Articles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE \"Articles\" SET \"AuthorId\" = \"CreatorId\" WHERE \"AuthorId\" = '00000000-0000-0000-0000-000000000000';");
            migrationBuilder.Sql("UPDATE \"Comments\" SET \"AuthorId\" = \"CreatorId\" WHERE \"AuthorId\" = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId_CreatedOn",
                table: "Comments",
                columns: new[] { "AuthorId", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreatedOn",
                table: "AspNetUsers",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreatedOn_Id",
                table: "AspNetUsers",
                columns: new[] { "CreatedOn", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreatorId",
                table: "AspNetUsers",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DeletedOn",
                table: "AspNetUsers",
                column: "DeletedOn");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastName_FirstName",
                table: "AspNetUsers",
                columns: new[] { "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_AuthorId_CreatedOn",
                table: "Articles",
                columns: new[] { "AuthorId", "CreatedOn" });

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_AspNetUsers_AuthorId",
                table: "Articles",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_AuthorId",
                table: "Comments",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_AspNetUsers_AuthorId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_AuthorId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AuthorId_CreatedOn",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CreatedOn",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CreatedOn_Id",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CreatorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DeletedOn",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LastName_FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Articles_AuthorId_CreatedOn",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Articles");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Comments",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_CreatorId",
                table: "Comments",
                newName: "IX_Comments_CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Articles",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_CreatorId",
                table: "Articles",
                newName: "IX_Articles_CreatedBy");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_AspNetUsers_CreatedBy",
                table: "Articles",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_CreatedBy",
                table: "Comments",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
