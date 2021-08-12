using Microsoft.EntityFrameworkCore.Migrations;

namespace ClinicQueue.Web.Migrations
{
    public partial class AddNotesAndRowVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "QueueEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "QueueEntries",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "QueueEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "QueueEntries");
        }
    }
}
