using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RedisUsage.CqrsCore.Migrations
{
    public partial class EventSourcingDbContext_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSourcingDescription",
                columns: table => new
                {
                    EsdId = table.Column<Guid>(nullable: false),
                    AggregateId = table.Column<Guid>(nullable: false),
                    Version = table.Column<long>(nullable: false),
                    AggregateType = table.Column<string>(nullable: true),
                    EventType = table.Column<string>(nullable: true),
                    EventData = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSourcingDescription", x => x.EsdId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSourcingDescription");
        }
    }
}
