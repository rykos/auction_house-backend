using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace ah_backend.Migrations
{
    public partial class finishedAuctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Auctions",
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "BuyDateTime",
                table: "Auctions",
                nullable: true)
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "BuyerId",
                table: "Auctions",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "BuyDateTime",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Auctions");
        }
    }
}
