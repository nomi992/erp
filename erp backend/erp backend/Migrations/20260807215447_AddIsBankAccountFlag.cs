using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBankAccountFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBankAccount",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBankAccount",
                table: "Accounts");
        }
    }
}
