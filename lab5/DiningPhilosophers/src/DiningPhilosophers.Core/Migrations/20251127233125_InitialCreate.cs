using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DiningPhilosophers.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimulationRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPhilosophers = table.Column<int>(type: "integer", nullable: false),
                    TotalForks = table.Column<int>(type: "integer", nullable: false),
                    Strategy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForkSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SimulationRunId = table.Column<int>(type: "integer", nullable: false),
                    ForkId = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HeldByPhilosopherName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ElapsedSeconds = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForkSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForkSnapshots_SimulationRuns_SimulationRunId",
                        column: x => x.SimulationRunId,
                        principalTable: "SimulationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhilosopherSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SimulationRunId = table.Column<int>(type: "integer", nullable: false),
                    PhilosopherName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastAction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MealsEaten = table.Column<int>(type: "integer", nullable: false),
                    ElapsedSeconds = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhilosopherSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhilosopherSnapshots_SimulationRuns_SimulationRunId",
                        column: x => x.SimulationRunId,
                        principalTable: "SimulationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForkSnapshots_SimulationRunId_Timestamp",
                table: "ForkSnapshots",
                columns: new[] { "SimulationRunId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PhilosopherSnapshots_SimulationRunId_Timestamp",
                table: "PhilosopherSnapshots",
                columns: new[] { "SimulationRunId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SimulationRuns_RunId",
                table: "SimulationRuns",
                column: "RunId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForkSnapshots");

            migrationBuilder.DropTable(
                name: "PhilosopherSnapshots");

            migrationBuilder.DropTable(
                name: "SimulationRuns");
        }
    }
}
