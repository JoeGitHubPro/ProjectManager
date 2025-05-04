using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhoneAndCRNIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consumers_CommercialRegisterNo",
                table: "Consumers");

            migrationBuilder.AlterColumn<string>(
                name: "CommercialRegisterNo",
                table: "Consumers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CommercialRegisterNo",
                table: "Consumers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_CommercialRegisterNo",
                table: "Consumers",
                column: "CommercialRegisterNo",
                unique: true,
                filter: "[CommercialRegisterNo] IS NOT NULL");
        }
    }
}
