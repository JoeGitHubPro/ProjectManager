using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class UniqueConsumer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consumers_Phone",
                table: "Consumers");

            migrationBuilder.AlterColumn<string>(
                name: "CommercialRegisterNo",
                table: "Consumers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Consumers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Consumers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_Phone_CommercialRegisterNo",
                table: "Consumers",
                columns: new[] { "Phone", "CommercialRegisterNo" },
                unique: true,
                filter: "[CommercialRegisterNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consumers_Phone_CommercialRegisterNo",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Consumers");

            migrationBuilder.AlterColumn<string>(
                name: "CommercialRegisterNo",
                table: "Consumers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_Phone",
                table: "Consumers",
                column: "Phone",
                unique: true);
        }
    }
}
