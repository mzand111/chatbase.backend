using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBase.Backend.Migrations
{
    /// <inheritdoc />
    public partial class chatContextInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ToUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2500)", maxLength: 2500, nullable: false),
                    MetaData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SendTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReceiveTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ViewTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReplyToID = table.Column<int>(type: "int", nullable: true),
                    ForwardID = table.Column<int>(type: "int", nullable: true),
                    ForwardDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForwardedFromGroup = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");
        }
    }
}
