using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class EditConsumerValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consumers_Phone_CommercialRegisterNo",
                table: "Consumers");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Consumers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Consumers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_Phone_CommercialRegisterNo",
                table: "Consumers",
                columns: new[] { "Phone", "CommercialRegisterNo" },
                unique: true,
                filter: "[Phone] IS NOT NULL AND [CommercialRegisterNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consumers_Phone_CommercialRegisterNo",
                table: "Consumers");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Consumers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Consumers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_Phone_CommercialRegisterNo",
                table: "Consumers",
                columns: new[] { "Phone", "CommercialRegisterNo" },
                unique: true,
                filter: "[CommercialRegisterNo] IS NOT NULL");
        }
    }
}
