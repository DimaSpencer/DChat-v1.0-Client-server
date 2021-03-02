using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class RemovedColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Connected",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAdministrator",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Connected",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValueSql: "(CONVERT([bit],(0)))");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdministrator",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
