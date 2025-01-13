using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBase.Backend.Migrations.ProfileDb
{
    /// <inheritdoc />
    public partial class addedTheImageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "UserProfileImages",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "UserProfileImages");
        }
    }
}
