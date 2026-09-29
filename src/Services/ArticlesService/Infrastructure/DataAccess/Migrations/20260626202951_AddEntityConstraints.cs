using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Tags",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.Sql(
                "UPDATE \"Tags\" " +
                "SET \"Color\" = '#D3D3D3' " +
                "WHERE \"Color\" IS NOT NULL AND \"Color\" !~* '^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$';"
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tag_Color_HexFormat",
                table: "Tags",
                sql: "\"Color\" IS NULL OR \"Color\" ~* '^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tag_Color_HexFormat",
                table: "Tags");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Tags",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7,
                oldNullable: true);
        }
    }
}
