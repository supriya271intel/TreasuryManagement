using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FinancialSystem.Infrastructure.Migrations
{
    public partial class AddGSecTradeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FdMaturityAmount",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FdMaturityDate",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FdRoi",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IsinReportDate",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GSecTrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Isin = table.Column<string>(type: "TEXT", nullable: true),
                    ScriptCode = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityName = table.Column<string>(type: "TEXT", nullable: true),
                    MaturityDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    TradeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Yield = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    DealValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BrokenPeriod = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GSecTrades", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GSecTrades");

            migrationBuilder.DropColumn(
                name: "FdMaturityAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FdMaturityDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FdRoi",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsinReportDate",
                table: "Orders");
        }
    }
}
