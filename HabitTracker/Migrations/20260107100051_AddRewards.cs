using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CostPoints = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RewardUnlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RewardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardUnlocks_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Completions_Date",
                table: "Completions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_RewardUnlocks_RewardId",
                table: "RewardUnlocks",
                column: "RewardId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RewardUnlocks");

            migrationBuilder.DropTable(
                name: "Rewards");

            migrationBuilder.DropIndex(
                name: "IX_Completions_Date",
                table: "Completions");
        }
    }
}
