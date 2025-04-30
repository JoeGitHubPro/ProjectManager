using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBusinessBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashFlows_BusinessAccounts_BusinessAccountModelAccountId",
                table: "CashFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_BusinessAccounts_BusinessAccountModelAccountId",
                table: "Loans");

            migrationBuilder.DropTable(
                name: "BusinessAccounts");

            migrationBuilder.RenameColumn(
                name: "BusinessAccountModelAccountId",
                table: "Loans",
                newName: "BusinessBalanceModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_BusinessAccountModelAccountId",
                table: "Loans",
                newName: "IX_Loans_BusinessBalanceModelId");

            migrationBuilder.RenameColumn(
                name: "BusinessAccountModelAccountId",
                table: "CashFlows",
                newName: "BusinessBalanceModelId");

            migrationBuilder.RenameIndex(
                name: "IX_CashFlows_BusinessAccountModelAccountId",
                table: "CashFlows",
                newName: "IX_CashFlows_BusinessBalanceModelId");

            migrationBuilder.CreateTable(
                name: "BusinessBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashOnHand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOutstandingLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSalesIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmergencyReserve = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessBalances", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CashFlows_BusinessBalances_BusinessBalanceModelId",
                table: "CashFlows",
                column: "BusinessBalanceModelId",
                principalTable: "BusinessBalances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_BusinessBalances_BusinessBalanceModelId",
                table: "Loans",
                column: "BusinessBalanceModelId",
                principalTable: "BusinessBalances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashFlows_BusinessBalances_BusinessBalanceModelId",
                table: "CashFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_BusinessBalances_BusinessBalanceModelId",
                table: "Loans");

            migrationBuilder.DropTable(
                name: "BusinessBalances");

            migrationBuilder.RenameColumn(
                name: "BusinessBalanceModelId",
                table: "Loans",
                newName: "BusinessAccountModelAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_BusinessBalanceModelId",
                table: "Loans",
                newName: "IX_Loans_BusinessAccountModelAccountId");

            migrationBuilder.RenameColumn(
                name: "BusinessBalanceModelId",
                table: "CashFlows",
                newName: "BusinessAccountModelAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_CashFlows_BusinessBalanceModelId",
                table: "CashFlows",
                newName: "IX_CashFlows_BusinessAccountModelAccountId");

            migrationBuilder.CreateTable(
                name: "BusinessAccounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmergencyReserve = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAccounts", x => x.AccountId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CashFlows_BusinessAccounts_BusinessAccountModelAccountId",
                table: "CashFlows",
                column: "BusinessAccountModelAccountId",
                principalTable: "BusinessAccounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_BusinessAccounts_BusinessAccountModelAccountId",
                table: "Loans",
                column: "BusinessAccountModelAccountId",
                principalTable: "BusinessAccounts",
                principalColumn: "AccountId");
        }
    }
}
