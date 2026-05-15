using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaqApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAiSearchHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiSearchHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SearchKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AiAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHelpful = table.Column<bool>(type: "bit", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSearchHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiSearchHistorySources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AiSearchHistoryId = table.Column<int>(type: "int", nullable: false),
                    FaqId = table.Column<int>(type: "int", nullable: false),
                    FaqTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSearchHistorySources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiSearchHistorySources_AiSearchHistories_AiSearchHistoryId",
                        column: x => x.AiSearchHistoryId,
                        principalTable: "AiSearchHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiSearchHistorySources_Faqs_FaqId",
                        column: x => x.FaqId,
                        principalTable: "Faqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchHistorySources_AiSearchHistoryId",
                table: "AiSearchHistorySources",
                column: "AiSearchHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchHistorySources_FaqId",
                table: "AiSearchHistorySources",
                column: "FaqId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSearchHistorySources");

            migrationBuilder.DropTable(
                name: "AiSearchHistories");
        }
    }
}
