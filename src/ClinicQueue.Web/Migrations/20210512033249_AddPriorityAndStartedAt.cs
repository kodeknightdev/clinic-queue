using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ClinicQueue.Web.Migrations
{
    public partial class AddPriorityAndStartedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPriority",
                table: "QueueEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "QueueEntries",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPriority",
                table: "QueueEntries");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "QueueEntries");
        }
    }
}
