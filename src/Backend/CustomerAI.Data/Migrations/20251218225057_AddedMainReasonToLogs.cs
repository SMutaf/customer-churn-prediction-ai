using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerAI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedMainReasonToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainReason",
                table: "AiPredictionLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainReason",
                table: "AiPredictionLogs");
        }
    }
}
