using CustomerAI.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerAI.Data.Migrations
{
    [DbContext(typeof(CustomerAiDbContext))]
    [Migration("20260515140000_ExtendAiPredictionLogRiskProfile")]
    public partial class ExtendAiPredictionLogRiskProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CoreRiskScore",
                table: "AiPredictionLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FinalRiskScore",
                table: "AiPredictionLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ModelExplanationsJson",
                table: "AiPredictionLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MlChurnProbability",
                table: "AiPredictionLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Segment",
                table: "AiPredictionLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TriggeredRulesJson",
                table: "AiPredictionLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoreRiskScore",
                table: "AiPredictionLogs");

            migrationBuilder.DropColumn(
                name: "FinalRiskScore",
                table: "AiPredictionLogs");

            migrationBuilder.DropColumn(
                name: "ModelExplanationsJson",
                table: "AiPredictionLogs");

            migrationBuilder.DropColumn(
                name: "MlChurnProbability",
                table: "AiPredictionLogs");

            migrationBuilder.DropColumn(
                name: "Segment",
                table: "AiPredictionLogs");

            migrationBuilder.DropColumn(
                name: "TriggeredRulesJson",
                table: "AiPredictionLogs");
        }
    }
}
