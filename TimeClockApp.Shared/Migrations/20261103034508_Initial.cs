using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimeClockApp.Shared.Migrations;

/// <inheritdoc />
public partial class _20261103034508_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Config",
            columns: table => new
            {
                ConfigId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                StringValue = table.Column<string>(type: "TEXT", nullable: true),
                IntValue = table.Column<int>(type: "INTEGER", nullable: true),
                Hint = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Config", x => x.ConfigId);
            });

        migrationBuilder.CreateTable(
            name: "Employee",
            columns: table => new
            {
                EmployeeId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Employee_Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Employee_PayRate = table.Column<double>(type: "REAL", nullable: false),
                Employee_Employed = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                JobTitle = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employee", x => x.EmployeeId);
            });

        migrationBuilder.CreateTable(
            name: "ExpenseType",
            columns: table => new
            {
                ExpenseTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CategoryName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpenseType", x => x.ExpenseTypeId);
            });

        migrationBuilder.CreateTable(
            name: "Phase",
            columns: table => new
            {
                PhaseId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                PhaseTitle = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Phase", x => x.PhaseId);
            });

        migrationBuilder.CreateTable(
            name: "Project",
            columns: table => new
            {
                ProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                ProjectDate = table.Column<DateOnly>(type: "date", nullable: false),
                ProjectDateEnd = table.Column<DateOnly>(type: "date", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Project", x => x.ProjectId);
            });

        migrationBuilder.CreateTable(
            name: "Expense",
            columns: table => new
            {
                ExpenseId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                PhaseId = table.Column<int>(type: "INTEGER", nullable: false),
                ExpenseTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                ExpenseTypeCategoryName = table.Column<string>(type: "TEXT", nullable: true),
                ExpenseDate = table.Column<DateOnly>(type: "date", nullable: false),
                Memo = table.Column<string>(type: "TEXT", nullable: true),
                Amount = table.Column<double>(type: "REAL", nullable: false),
                IsRecent = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                ExpenseProject = table.Column<string>(type: "TEXT", nullable: true),
                ExpensePhase = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Expense", x => x.ExpenseId);
                table.ForeignKey(
                    name: "FK_Expense_ExpenseType_ExpenseTypeId",
                    column: x => x.ExpenseTypeId,
                    principalTable: "ExpenseType",
                    principalColumn: "ExpenseTypeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Expense_Phase_PhaseId",
                    column: x => x.PhaseId,
                    principalTable: "Phase",
                    principalColumn: "PhaseId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Expense_Project_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Project",
                    principalColumn: "ProjectId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TimeCard",
            columns: table => new
            {
                TimeCardId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                ProjectId = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                PhaseId = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                TimeCard_EmployeeName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                TimeCard_Status = table.Column<int>(type: "INTEGER", nullable: false),
                TimeCard_DateTime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now', 'localtime')"),
                TimeCard_Date = table.Column<DateOnly>(type: "date", nullable: false),
                TimeCard_StartTime = table.Column<TimeOnly>(type: "time", precision: 0, nullable: false),
                TimeCard_EndTime = table.Column<TimeOnly>(type: "time", precision: 0, nullable: false),
                TimeCard_WorkHours = table.Column<double>(type: "REAL", nullable: false, computedColumnSql: "round((strftime('%s', [TimeCard_EndTime]) - strftime('%s', [TimeCard_StartTime])) / 3600.0, 2)", stored: true),
                TimeCard_EmployeePayRate = table.Column<double>(type: "REAL", nullable: false),
                TimeCard_bReadOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                ProjectName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                PhaseTitle = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TimeCard", x => x.TimeCardId);
                table.ForeignKey(
                    name: "FK_TimeCard_Employee_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TimeCard_Phase_PhaseId",
                    column: x => x.PhaseId,
                    principalTable: "Phase",
                    principalColumn: "PhaseId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TimeCard_Project_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Project",
                    principalColumn: "ProjectId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "Config",
            columns: new[] { "ConfigId", "Hint", "IntValue", "Name", "StringValue" },
            values: new object[,]
            {
                { 1, "The business entity name", null, "Company", "Alexander Builder" },
                { 2, "Date this app was 1st ran. For internal use only", null, "FirstRun", "11/2/2026" },
                { 3, "Current Project to default to", 1, "CurrentProject", null },
                { 4, "Current Phase to default to", 1, "CurrentPhase", null },
                { 5, "Worker Comp Rate per $100 remuneration", null, "WorkCompRate", "0.171118" },
                { 6, null, null, "EstimateEmployerTaxes", "0.1019" },
                { 7, null, null, "ProfitRate", "0.1" },
                { 8, null, null, "OverHeadRate", "0.08" },
                { 9, "Enables dangerous timecard edits", 0, "IsAdmin", null },
                { 10, "Number of Days in a Pay Period (weekly=7,biweekly=14,etc) (Default 7)", 7, "DaysInPayPeriod", null },
                { 11, "Day of week that is the end of the pay period (0=Sunday...3=Wednesday...5=Friday,6=Saturday)(Default 5)", 5, "PayDayOfWeek", null },
                { 12, "Application Database version", null, "Version", "2.2" },
                { 13, "Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)", 2, "AppTheme", null },
                { 14, "Enable or disable scheduled notifications for Lunch Breaks, End of Shift. (0=Disabled, 1=Enabled)", 0, "UseNotifications", null },
                { 15, "Amount of time in hours for notifications. In increments of a tenth of a hour. (1.5 = 1 hour & 30 min)", null, "NotificationHours", "4.0" }
            });

        migrationBuilder.InsertData(
            table: "Employee",
            columns: new[] { "EmployeeId", "Employee_Name", "Employee_PayRate", "JobTitle" },
            values: new object[,]
            {
                { 1, "John Doe", 25.0, "Job Title" },
                { 2, "Jane Doe", 25.0, "Job Title" }
            });

        migrationBuilder.InsertData(
            table: "ExpenseType",
            columns: new[] { "ExpenseTypeId", "CategoryName" },
            values: new object[,]
            {
                { 1, "Deleted" },
                { 2, "Income" },
                { 3, "Payroll" },
                { 4, "WorkersComp" },
                { 5, "Materials" },
                { 6, "Toll.Gas" },
                { 7, "Misc" },
                { 8, "Refund" },
                { 9, "Subcontractor" },
                { 10, "Taxes" },
                { 11, "Dumps" },
                { 12, "Overhead" },
                { 13, "Permit" }
            });

        migrationBuilder.InsertData(
            table: "Phase",
            columns: new[] { "PhaseId", "PhaseTitle" },
            values: new object[,]
            {
                { 1, ".Misc" },
                { 2, "Administrative" },
                { 3, "Bathroom" },
                { 4, "Blueprints" },
                { 5, "Building Paper" },
                { 6, "Cabinets" },
                { 7, "Cement" },
                { 8, "Cement-Forms" },
                { 9, "Clean Up" },
                { 10, "Countertop" },
                { 11, "Crown Molding" },
                { 12, "Data/Comm/AV" },
                { 13, "Deck" },
                { 14, "Demo" },
                { 15, "Doors" },
                { 16, "Drywall" },
                { 17, "Drywall-Tape+Mud" },
                { 18, "Drywall-Texture" },
                { 19, "Dumps" },
                { 20, "Electric-Finish" },
                { 21, "Electrical" },
                { 22, "Excavation" },
                { 23, "Fence" },
                { 24, "Finish Hardware" },
                { 25, "Flooring" },
                { 26, "Framing" },
                { 27, "Gas Line" },
                { 28, "HVAC" },
                { 29, "Inspection" },
                { 30, "Insulation" },
                { 31, "Irrigation" },
                { 32, "Kitchen" },
                { 33, "Landscaping" },
                { 34, "Light Fixtures" },
                { 35, "Masonry/Brick" },
                { 36, "Moving" },
                { 37, "Painting" },
                { 38, "Plumbing" },
                { 39, "Plumbing-Rough" },
                { 40, "Prep-Painting" },
                { 41, "Roofing" },
                { 42, "Siding" },
                { 43, "Stairs" },
                { 44, "Stucco" },
                { 45, "Stucco-Lath" },
                { 46, "Subfloor" },
                { 47, "Tile" },
                { 48, "Trim/Baseboards" },
                { 49, "Water Heater" },
                { 50, "Windows" }
            });

        migrationBuilder.InsertData(
            table: "Project",
            columns: new[] { "ProjectId", "Name", "ProjectDate", "ProjectDateEnd", "Status" },
            values: new object[,]
            {
                { 1, ".None", new DateOnly(2026, 11, 2), null, 1 },
                { 2, "Sample", new DateOnly(2026, 11, 2), null, 1 }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Employee_Employee_Name",
            table: "Employee",
            column: "Employee_Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Expense_ExpenseTypeId",
            table: "Expense",
            column: "ExpenseTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Expense_PhaseId",
            table: "Expense",
            column: "PhaseId");

        migrationBuilder.CreateIndex(
            name: "IX_Expense_ProjectId",
            table: "Expense",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_ExpenseType_CategoryName",
            table: "ExpenseType",
            column: "CategoryName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Phase_PhaseTitle",
            table: "Phase",
            column: "PhaseTitle",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TimeCard_EmployeeId",
            table: "TimeCard",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_TimeCard_PhaseId",
            table: "TimeCard",
            column: "PhaseId");

        migrationBuilder.CreateIndex(
            name: "IX_TimeCard_ProjectId",
            table: "TimeCard",
            column: "ProjectId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Config");

        migrationBuilder.DropTable(
            name: "Expense");

        migrationBuilder.DropTable(
            name: "TimeCard");

        migrationBuilder.DropTable(
            name: "ExpenseType");

        migrationBuilder.DropTable(
            name: "Employee");

        migrationBuilder.DropTable(
            name: "Phase");

        migrationBuilder.DropTable(
            name: "Project");
    }
}
