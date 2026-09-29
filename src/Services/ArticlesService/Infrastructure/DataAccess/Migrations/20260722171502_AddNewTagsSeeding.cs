using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArticlesApp.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTagsSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Color", "Label" },
                values: new object[,]
                {
                    { new Guid("50dc578c-e857-4f2d-ae4f-d17c62b90673"), "#778899", "GEO" },
                    { new Guid("50dc578c-e857-4f2d-ae4f-d17c62b90674"), "#778899", "MATH" },
                    { new Guid("50dc578c-e857-4f2d-ae4f-d17c62b90675"), "#778899", "DIY" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("50dc578c-e857-4f2d-ae4f-d17c62b90673"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("50dc578c-e857-4f2d-ae4f-d17c62b90674"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("50dc578c-e857-4f2d-ae4f-d17c62b90675"));
        }
    }
}
