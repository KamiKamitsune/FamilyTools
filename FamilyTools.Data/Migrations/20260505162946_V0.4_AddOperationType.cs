using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTools.Data.Migrations
{
    /// <inheritdoc />
    public partial class V04_AddOperationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OperationType",
                table: "AccountEnters",
                newName: "OperationTypeId");

            migrationBuilder.CreateTable(
                name: "OperationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountEnters_OperationTypeId",
                table: "AccountEnters",
                column: "OperationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountEnters_OperationType_OperationTypeId",
                table: "AccountEnters",
                column: "OperationTypeId",
                principalTable: "OperationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountEnters_OperationType_OperationTypeId",
                table: "AccountEnters");

            migrationBuilder.DropTable(
                name: "OperationType");

            migrationBuilder.DropIndex(
                name: "IX_AccountEnters_OperationTypeId",
                table: "AccountEnters");

            migrationBuilder.RenameColumn(
                name: "OperationTypeId",
                table: "AccountEnters",
                newName: "OperationType");
        }
    }
}
