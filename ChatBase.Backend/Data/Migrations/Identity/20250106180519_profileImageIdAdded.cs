using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBase.Backend.Migrations.Identity
{
    /// <inheritdoc />
    public partial class profileImageIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentProfileImageId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentProfileImageId",
                table: "AspNetUsers");
        }
    }
}
