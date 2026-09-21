using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeClockApp.Shared.Migrations;

/// <inheritdoc />
public partial class _20260912004603_Update1 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateOnly>(
            name: "ProjectDateEnd",
            table: "Project",
            type: "date",
            nullable: true);

        migrationBuilder.UpdateData(
            table: "Config",
            keyColumn: "ConfigId",
            keyValue: 2,
            column: "StringValue",
            value: "9/11/2026");

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 1,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 2,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 3,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 4,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 5,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 6,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 7,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 8,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 9,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 10,
            columns: new[] { "ProjectDate", "ProjectDateEnd" },
            values: new object[] { new DateOnly(2026, 9, 11), null });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProjectDateEnd",
            table: "Project");

        migrationBuilder.UpdateData(
            table: "Config",
            keyColumn: "ConfigId",
            keyValue: 2,
            column: "StringValue",
            value: "8/1/2026");

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 1,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 2,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 3,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 4,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 5,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 6,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 7,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 8,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 9,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));

        migrationBuilder.UpdateData(
            table: "Project",
            keyColumn: "ProjectId",
            keyValue: 10,
            column: "ProjectDate",
            value: new DateOnly(2026, 8, 1));
    }
}
