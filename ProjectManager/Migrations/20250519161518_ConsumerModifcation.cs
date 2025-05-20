using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class ConsumerModifcation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Consumers",
                newName: "LocationMap");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Consumers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleSellerId",
                table: "Consumers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Consumers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_LocationId",
                table: "Consumers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_ResponsibleSellerId",
                table: "Consumers",
                column: "ResponsibleSellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumers_Locations_LocationId",
                table: "Consumers",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Consumers_Users_ResponsibleSellerId",
                table: "Consumers",
                column: "ResponsibleSellerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consumers_Locations_LocationId",
                table: "Consumers");

            migrationBuilder.DropForeignKey(
                name: "FK_Consumers_Users_ResponsibleSellerId",
                table: "Consumers");

            migrationBuilder.DropIndex(
                name: "IX_Consumers_LocationId",
                table: "Consumers");

            migrationBuilder.DropIndex(
                name: "IX_Consumers_ResponsibleSellerId",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "ResponsibleSellerId",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Consumers");

            migrationBuilder.RenameColumn(
                name: "LocationMap",
                table: "Consumers",
                newName: "Location");
        }
    }
}
