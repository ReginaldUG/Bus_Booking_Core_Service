using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BusBooking.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_CancelledBy_Condition",
                table: "Bookings");

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    Revoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokens_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "chk_CancelledBy_Condition",
                table: "Bookings",
                sql: "(\"IsCancelled\" = true AND \"CancelledBy\" IN ('customer', 'driver')) OR (\"IsCancelled\" = false AND \"CancelledBy\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CustomerId",
                table: "Tokens",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenHash",
                table: "Tokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "uq_owner_token",
                table: "Tokens",
                columns: new[] { "CustomerId", "TokenHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropCheckConstraint(
                name: "chk_CancelledBy_Condition",
                table: "Bookings");

            migrationBuilder.AddCheckConstraint(
                name: "chk_CancelledBy_Condition",
                table: "Bookings",
                sql: "(\"isCancelled\" = true AND \"CancelledBy\" IN ('customer', 'driver')) OR (\"isCancelled\" = false AND \"CancelledBy\" IS NULL)");
        }
    }
}
