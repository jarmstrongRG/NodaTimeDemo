using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NodaTimeDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNodaTimeInstantColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimezoneConversions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalTimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetTimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConvertedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalLocalDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConvertedLocalDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalInstant = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConvertedInstant = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OriginalUtcDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsesNodaTimeInstant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimezoneConversions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimezoneConversions");
        }
    }
}
