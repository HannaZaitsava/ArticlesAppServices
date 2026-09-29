using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyTableConfigRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleArticleCategory_ArticleCategories_CategoryId1",
                table: "ArticleArticleCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleArticleCategory_Articles_ArticleId1",
                table: "ArticleArticleCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleTag_Articles_ArticleId1",
                table: "ArticleTag");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleTag_Tags_TagId1",
                table: "ArticleTag");

            migrationBuilder.DropIndex(
                name: "IX_ArticleTag_ArticleId1",
                table: "ArticleTag");

            migrationBuilder.DropIndex(
                name: "IX_ArticleTag_TagId1",
                table: "ArticleTag");

            migrationBuilder.DropIndex(
                name: "IX_ArticleArticleCategory_ArticleId1",
                table: "ArticleArticleCategory");

            migrationBuilder.DropIndex(
                name: "IX_ArticleArticleCategory_CategoryId1",
                table: "ArticleArticleCategory");

            migrationBuilder.DropColumn(
                name: "ArticleId1",
                table: "ArticleTag");

            migrationBuilder.DropColumn(
                name: "TagId1",
                table: "ArticleTag");

            migrationBuilder.DropColumn(
                name: "ArticleId1",
                table: "ArticleArticleCategory");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "ArticleArticleCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ArticleId1",
                table: "ArticleTag",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TagId1",
                table: "ArticleTag",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ArticleId1",
                table: "ArticleArticleCategory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId1",
                table: "ArticleArticleCategory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTag_ArticleId1",
                table: "ArticleTag",
                column: "ArticleId1");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTag_TagId1",
                table: "ArticleTag",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleArticleCategory_ArticleId1",
                table: "ArticleArticleCategory",
                column: "ArticleId1");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleArticleCategory_CategoryId1",
                table: "ArticleArticleCategory",
                column: "CategoryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleArticleCategory_ArticleCategories_CategoryId1",
                table: "ArticleArticleCategory",
                column: "CategoryId1",
                principalTable: "ArticleCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleArticleCategory_Articles_ArticleId1",
                table: "ArticleArticleCategory",
                column: "ArticleId1",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleTag_Articles_ArticleId1",
                table: "ArticleTag",
                column: "ArticleId1",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleTag_Tags_TagId1",
                table: "ArticleTag",
                column: "TagId1",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
