using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ClinicQueue.Web.Migrations
{
    public partial class AddTicketNumbers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LookupCode",
                table: "QueueEntries",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QueueDate",
                table: "QueueEntries",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TicketNumber",
                table: "QueueEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows so the unique index below can be created.
            migrationBuilder.Sql(
                "UPDATE QueueEntries SET QueueDate = CAST(CheckedInAt AS date), TicketNumber = Id WHERE TicketNumber = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_LookupCode",
                table: "QueueEntries",
                column: "LookupCode",
                unique: true,
                filter: "[LookupCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_QueueDate_TicketNumber",
                table: "QueueEntries",
                columns: new[] { "QueueDate", "TicketNumber" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_LookupCode",
                table: "QueueEntries");

            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_QueueDate_TicketNumber",
                table: "QueueEntries");

            migrationBuilder.DropColumn(
                name: "LookupCode",
                table: "QueueEntries");

            migrationBuilder.DropColumn(
                name: "QueueDate",
                table: "QueueEntries");

            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "QueueEntries");
        }
    }
}
