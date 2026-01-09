using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperatorProductivity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PickTasksCompleted = table.Column<int>(type: "integer", nullable: false),
                    PackTasksCompleted = table.Column<int>(type: "integer", nullable: false),
                    PutawayTasksCompleted = table.Column<int>(type: "integer", nullable: false),
                    CycleCountsCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalUnitsPicked = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalUnitsPacked = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalUnitsPutaway = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalLocationsVisited = table.Column<int>(type: "integer", nullable: false),
                    TotalWorkMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalIdleMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalBreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalScans = table.Column<int>(type: "integer", nullable: false),
                    CorrectScans = table.Column<int>(type: "integer", nullable: false),
                    ErrorScans = table.Column<int>(type: "integer", nullable: false),
                    AccuracyRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PicksPerHour = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    UnitsPerHour = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TasksPerHour = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorProductivity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperatorSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentTaskType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CurrentTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentZone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CurrentLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShiftCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShiftStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScanLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Barcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScanType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Context = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedSku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolvedLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaskType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductSku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskActionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorProductivity_TenantId_UserId_Date",
                table: "OperatorProductivity",
                columns: new[] { "TenantId", "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperatorProductivity_TenantId_WarehouseId_Date",
                table: "OperatorProductivity",
                columns: new[] { "TenantId", "WarehouseId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSessions_TenantId_UserId_Status",
                table: "OperatorSessions",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSessions_TenantId_WarehouseId_Status",
                table: "OperatorSessions",
                columns: new[] { "TenantId", "WarehouseId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_TenantId_Barcode",
                table: "ScanLogs",
                columns: new[] { "TenantId", "Barcode" });

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_TenantId_SessionId",
                table: "ScanLogs",
                columns: new[] { "TenantId", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_TenantId_UserId_ScannedAt",
                table: "ScanLogs",
                columns: new[] { "TenantId", "UserId", "ScannedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ScanLogs_TenantId_WarehouseId_ScannedAt",
                table: "ScanLogs",
                columns: new[] { "TenantId", "WarehouseId", "ScannedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskActionLogs_TenantId_TaskType_TaskId",
                table: "TaskActionLogs",
                columns: new[] { "TenantId", "TaskType", "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskActionLogs_TenantId_UserId_ActionAt",
                table: "TaskActionLogs",
                columns: new[] { "TenantId", "UserId", "ActionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskActionLogs_TenantId_WarehouseId_ActionAt",
                table: "TaskActionLogs",
                columns: new[] { "TenantId", "WarehouseId", "ActionAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatorProductivity");

            migrationBuilder.DropTable(
                name: "OperatorSessions");

            migrationBuilder.DropTable(
                name: "ScanLogs");

            migrationBuilder.DropTable(
                name: "TaskActionLogs");
        }
    }
}
