using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace IotPipeline.Platform.Migrations
{
    /// <inheritdoc />
    public partial class Optimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelemetryRecords_Timestamp",
                table: "TelemetryRecords");

            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "TelemetryRecords",
                type: "vector(384)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryRecords_DeviceId_Timestamp",
                table: "TelemetryRecords",
                columns: new[] { "DeviceId", "Timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryRecords_Embedding",
                table: "TelemetryRecords",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelemetryRecords_DeviceId_Timestamp",
                table: "TelemetryRecords");

            migrationBuilder.DropIndex(
                name: "IX_TelemetryRecords_Embedding",
                table: "TelemetryRecords");

            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "TelemetryRecords",
                type: "vector",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(384)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryRecords_Timestamp",
                table: "TelemetryRecords",
                column: "Timestamp");
        }
    }
}
