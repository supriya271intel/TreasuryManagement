using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FinancialSystem.Infrastructure.Migrations
{
    public partial class AddCbsFieldsToTrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CbsProcessedAt",
                table: "Trades",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CbsTransactionId",
                table: "Trades",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCbsProcessed",
                table: "Trades",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CbsProcessedAt",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "CbsTransactionId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "IsCbsProcessed",
                table: "Trades");
        }
    }
}
