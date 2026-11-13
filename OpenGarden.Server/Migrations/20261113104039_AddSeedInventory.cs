using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenGarden.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeedInventory",
                schema: "public",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    PlantTypeId = table.Column<short>(type: "smallint", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedInventory", x => new { x.PlayerId, x.PlantTypeId });
                    table.CheckConstraint("CK_SeedInventory_Quantity", "\"Quantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_SeedInventory_PlantTypes_PlantTypeId",
                        column: x => x.PlantTypeId,
                        principalSchema: "public",
                        principalTable: "PlantTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeedInventory_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "public",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeedInventory_PlantTypeId",
                schema: "public",
                table: "SeedInventory",
                column: "PlantTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeedInventory",
                schema: "public");
        }
    }
}
