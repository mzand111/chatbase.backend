using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBase.Backend.Migrations.ProfileDb
{
    /// <inheritdoc />
    public partial class IDFieldChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserProfileImages",
                newName: "ID");

            migrationBuilder.AlterColumn<Guid>(
                name: "ID",
                table: "UserProfileImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("Relational:ColumnOrder", 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "UserProfileImages",
                newName: "Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserProfileImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .OldAnnotation("Relational:ColumnOrder", 0);
        }
    }
}
