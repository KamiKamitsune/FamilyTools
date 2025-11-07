using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTools.Data.Migrations
{
    /// <inheritdoc />
    public partial class V01_AddOperationTypeForAccountEnters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OperationType",
                table: "AccountEnters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperationType",
                table: "AccountEnters");
        }
    }
}
