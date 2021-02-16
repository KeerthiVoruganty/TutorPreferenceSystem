using Microsoft.EntityFrameworkCore.Migrations;

namespace SessionalAllocation.Migrations
{
    public partial class AddFKTimeAvailability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicantNavigationId",
                table: "TimeAvailability",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeAvailability_ApplicantNavigationId",
                table: "TimeAvailability",
                column: "ApplicantNavigationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeAvailability_AspNetUsers_ApplicantNavigationId",
                table: "TimeAvailability",
                column: "ApplicantNavigationId",
                principalSchema: "dbo",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeAvailability_AspNetUsers_ApplicantNavigationId",
                table: "TimeAvailability");

            migrationBuilder.DropIndex(
                name: "IX_TimeAvailability_ApplicantNavigationId",
                table: "TimeAvailability");

            migrationBuilder.DropColumn(
                name: "ApplicantNavigationId",
                table: "TimeAvailability");
        }
    }
}
