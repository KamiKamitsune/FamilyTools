using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTools.Data.Migrations
{
    /// <inheritdoc />
    public partial class V02_AddPaymentDones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDone_AccountPages_PageId",
                table: "PaymentDone");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDone_Users_UserId",
                table: "PaymentDone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDone",
                table: "PaymentDone");

            migrationBuilder.RenameTable(
                name: "PaymentDone",
                newName: "PaymentDones");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDone_UserId",
                table: "PaymentDones",
                newName: "IX_PaymentDones_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDone_PageId",
                table: "PaymentDones",
                newName: "IX_PaymentDones_PageId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "PaymentDones",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDones",
                table: "PaymentDones",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDones_AccountPages_PageId",
                table: "PaymentDones",
                column: "PageId",
                principalTable: "AccountPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDones_Users_UserId",
                table: "PaymentDones",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDones_AccountPages_PageId",
                table: "PaymentDones");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDones_Users_UserId",
                table: "PaymentDones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDones",
                table: "PaymentDones");

            migrationBuilder.RenameTable(
                name: "PaymentDones",
                newName: "PaymentDone");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDones_UserId",
                table: "PaymentDone",
                newName: "IX_PaymentDone_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDones_PageId",
                table: "PaymentDone",
                newName: "IX_PaymentDone_PageId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "PaymentDone",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDone",
                table: "PaymentDone",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDone_AccountPages_PageId",
                table: "PaymentDone",
                column: "PageId",
                principalTable: "AccountPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDone_Users_UserId",
                table: "PaymentDone",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
