using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllupPraktika.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderColumnToSlidesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Slides",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Slides");
        }
    }
}
