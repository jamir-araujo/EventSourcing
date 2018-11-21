using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventSourcing.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(nullable: false),
                    AggregateType = table.Column<string>(nullable: false),
                    Body = table.Column<string>(nullable: false),
                    Version = table.Column<long>(nullable: false),
                    EventType = table.Column<string>(nullable: false),
                    Metadata = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => new { x.AggregateId, x.Version });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_AggregateId_AggregateType",
                table: "Events",
                columns: new[] { "AggregateId", "AggregateType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
