using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FinancialSystem.Infrastructure.Migrations
{
    public partial class AddFixedDepositTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FixedDeposits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FdName = table.Column<string>(type: "TEXT", nullable: true),
                    FdNumber = table.Column<string>(type: "TEXT", nullable: true),
                    DepositDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaturityDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NumberOfDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Principal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestFrequency = table.Column<int>(type: "INTEGER", nullable: false),
                    Tds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedDeposits", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FixedDeposits");
        }
    }
}
