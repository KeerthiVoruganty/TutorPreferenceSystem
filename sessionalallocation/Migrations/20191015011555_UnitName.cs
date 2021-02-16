using Microsoft.EntityFrameworkCore.Migrations;

namespace SessionalAllocation.Migrations
{
    public partial class UnitName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "Class",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "Class");
        }
    }
}
