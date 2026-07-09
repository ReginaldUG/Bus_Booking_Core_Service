using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBooking.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Schedules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Schedules");
        }
    }
}
