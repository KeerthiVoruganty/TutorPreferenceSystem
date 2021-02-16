using Microsoft.EntityFrameworkCore.Migrations;

namespace SessionalAllocation.Migrations
{
    public partial class RemoveIsAvailableTimeAvailability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "TimeAvailability");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "TimeAvailability",
                nullable: false,
                defaultValue: false);
        }
    }
}
