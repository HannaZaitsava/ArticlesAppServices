using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForCursorPaginationToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedOn_Id",
                table: "Comments",
                columns: new[] { "CreatedOn", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_CreatedOn_Id",
                table: "Comments");
        }
    }
}
