using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Migrations
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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Question = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Answer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModelName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsedExternalAi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WasHelpful = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSearchHistories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AiSearchReferences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AiSearchHistoryId = table.Column<long>(type: "bigint", nullable: false),
                    FaqId = table.Column<int>(type: "int", nullable: false),
                    FaqTitle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSearchReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiSearchReferences_AiSearchHistories_AiSearchHistoryId",
                        column: x => x.AiSearchHistoryId,
                        principalTable: "AiSearchHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchHistories_CreatedAt",
                table: "AiSearchHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchHistories_IsSuccess",
                table: "AiSearchHistories",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchHistories_WasHelpful",
                table: "AiSearchHistories",
                column: "WasHelpful");

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchReferences_AiSearchHistoryId",
                table: "AiSearchReferences",
                column: "AiSearchHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchReferences_AiSearchHistoryId_DisplayOrder",
                table: "AiSearchReferences",
                columns: new[] { "AiSearchHistoryId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiSearchReferences_FaqId",
                table: "AiSearchReferences",
                column: "FaqId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSearchReferences");

            migrationBuilder.DropTable(
                name: "AiSearchHistories");
        }
    }
}
