using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationActionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationActionTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TriggerType = table.Column<int>(type: "integer", nullable: false),
                    TriggerEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TriggerEvent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConditionsJson = table.Column<string>(type: "jsonb", nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    ActionConfigJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    MaxExecutionsPerDay = table.Column<int>(type: "integer", nullable: true),
                    CooldownSeconds = table.Column<int>(type: "integer", nullable: true),
                    TotalExecutions = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulExecutions = table.Column<int>(type: "integer", nullable: false),
                    FailedExecutions = table.Column<int>(type: "integer", nullable: false),
                    LastExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Logic = table.Column<int>(type: "integer", nullable: false),
                    Field = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleConditions_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TriggerEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggerEventData = table.Column<string>(type: "jsonb", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConditionsMet = table.Column<bool>(type: "boolean", nullable: false),
                    ResultData = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "text", nullable: true),
                    CreatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRuleExecutions_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationScheduledJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationScheduledJobs_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationActionTemplates_TenantId_Code",
                table: "AutomationActionTemplates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleConditions_RuleId",
                table: "AutomationRuleConditions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleExecutions_RuleId",
                table: "AutomationRuleExecutions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleExecutions_StartedAt",
                table: "AutomationRuleExecutions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleExecutions_Status",
                table: "AutomationRuleExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleExecutions_TenantId",
                table: "AutomationRuleExecutions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleExecutions_TenantId_StartedAt",
                table: "AutomationRuleExecutions",
                columns: new[] { "TenantId", "StartedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_IsActive",
                table: "AutomationRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_TenantId",
                table: "AutomationRules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_TenantId_NextScheduledAt",
                table: "AutomationRules",
                columns: new[] { "TenantId", "NextScheduledAt" },
                filter: "\"IsActive\" = true AND \"TriggerType\" = 5");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_TenantId_TriggerType_IsActive",
                table: "AutomationRules",
                columns: new[] { "TenantId", "TriggerType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationScheduledJobs_RuleId",
                table: "AutomationScheduledJobs",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationScheduledJobs_ScheduledFor",
                table: "AutomationScheduledJobs",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationScheduledJobs_Status",
                table: "AutomationScheduledJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationScheduledJobs_Status_ScheduledFor",
                table: "AutomationScheduledJobs",
                columns: new[] { "Status", "ScheduledFor" },
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationActionTemplates");

            migrationBuilder.DropTable(
                name: "AutomationRuleConditions");

            migrationBuilder.DropTable(
                name: "AutomationRuleExecutions");

            migrationBuilder.DropTable(
                name: "AutomationScheduledJobs");

            migrationBuilder.DropTable(
                name: "AutomationRules");
        }
    }
}
