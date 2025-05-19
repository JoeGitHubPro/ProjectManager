using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class SellerOfOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerId",
                table: "ProjectOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOrders_SellerId",
                table: "ProjectOrders",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectOrders_Users_SellerId",
                table: "ProjectOrders",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectOrders_Users_SellerId",
                table: "ProjectOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProjectOrders_SellerId",
                table: "ProjectOrders");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "ProjectOrders");
        }
    }
}
