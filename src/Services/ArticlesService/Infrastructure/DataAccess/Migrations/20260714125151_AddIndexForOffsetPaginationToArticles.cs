using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForOffsetPaginationToArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Articles_CreatedOn_Id",
                table: "Articles",
                columns: new[] { "CreatedOn", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Title_Id",
                table: "Articles",
                columns: new[] { "Title", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Articles_CreatedOn_Id",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_Title_Id",
                table: "Articles");
        }
    }
}
