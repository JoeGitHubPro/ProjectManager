using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class CapitalManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "CashFlows",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BusinessAccountModelAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlows", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_CashFlows_BusinessAccounts_BusinessAccountModelAccountId",
                        column: x => x.BusinessAccountModelAccountId,
                        principalTable: "BusinessAccounts",
                        principalColumn: "AccountId");
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    LoanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AmountLoaned = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsRepaid = table.Column<bool>(type: "bit", nullable: false),
                    BusinessAccountModelAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.LoanId);
                    table.ForeignKey(
                        name: "FK_Loans_BusinessAccounts_BusinessAccountModelAccountId",
                        column: x => x.BusinessAccountModelAccountId,
                        principalTable: "BusinessAccounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_Loans_Users_LenderId",
                        column: x => x.LenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashFlows_BusinessAccountModelAccountId",
                table: "CashFlows",
                column: "BusinessAccountModelAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BusinessAccountModelAccountId",
                table: "Loans",
                column: "BusinessAccountModelAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LenderId",
                table: "Loans",
                column: "LenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashFlows");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "BusinessAccounts");
        }
    }
}
