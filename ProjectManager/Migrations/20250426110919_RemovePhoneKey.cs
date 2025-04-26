using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhoneKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectOrders_Consumers_ConsumerPhone",
                table: "ProjectOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProjectOrders_ConsumerPhone",
                table: "ProjectOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Consumers",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "ConsumerPhone",
                table: "ProjectOrders");

            migrationBuilder.AddColumn<int>(
                name: "ConsumerId",
                table: "ProjectOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Consumers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Consumers",
                table: "Consumers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOrders_ConsumerId",
                table: "ProjectOrders",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_Phone",
                table: "Consumers",
                column: "Phone",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectOrders_Consumers_ConsumerId",
                table: "ProjectOrders",
                column: "ConsumerId",
                principalTable: "Consumers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectOrders_Consumers_ConsumerId",
                table: "ProjectOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProjectOrders_ConsumerId",
                table: "ProjectOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Consumers",
                table: "Consumers");

            migrationBuilder.DropIndex(
                name: "IX_Consumers_Phone",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "ConsumerId",
                table: "ProjectOrders");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Consumers");

            migrationBuilder.AddColumn<string>(
                name: "ConsumerPhone",
                table: "ProjectOrders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Consumers",
                table: "Consumers",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOrders_ConsumerPhone",
                table: "ProjectOrders",
                column: "ConsumerPhone");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectOrders_Consumers_ConsumerPhone",
                table: "ProjectOrders",
                column: "ConsumerPhone",
                principalTable: "Consumers",
                principalColumn: "Phone",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
