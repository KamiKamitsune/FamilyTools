using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTools.Data.Migrations
{
    /// <inheritdoc />
    public partial class V03_AddIsDisabledForEnter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "AccountEnters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "AccountEnters");
        }
    }
}
