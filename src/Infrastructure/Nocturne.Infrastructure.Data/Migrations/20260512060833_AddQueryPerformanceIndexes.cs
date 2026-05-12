using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nocturne.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_linked_records_record",
                table: "linked_records");

            migrationBuilder.AddColumn<DateTime>(
                name: "dismissed_at",
                table: "oauth_grants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_sensor_glucose_tenant_timestamp",
                table: "sensor_glucose",
                columns: new[] { "tenant_id", "timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_linked_records_non_primary_record",
                table: "linked_records",
                columns: new[] { "record_type", "record_id" },
                filter: "NOT is_primary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sensor_glucose_tenant_timestamp",
                table: "sensor_glucose");

            migrationBuilder.DropIndex(
                name: "ix_linked_records_non_primary_record",
                table: "linked_records");

            migrationBuilder.DropColumn(
                name: "dismissed_at",
                table: "oauth_grants");

            migrationBuilder.CreateIndex(
                name: "ix_linked_records_record",
                table: "linked_records",
                columns: new[] { "record_type", "record_id" });
        }
    }
}
