using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenGarden.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlantedById",
                schema: "public",
                table: "Plants",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plants_PlantedById",
                schema: "public",
                table: "Plants",
                column: "PlantedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_AspNetUsers_PlantedById",
                schema: "public",
                table: "Plants",
                column: "PlantedById",
                principalSchema: "identity",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_AspNetUsers_PlantedById",
                schema: "public",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlantedById",
                schema: "public",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PlantedById",
                schema: "public",
                table: "Plants");
        }
    }
}
