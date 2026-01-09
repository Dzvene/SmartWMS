using SmartWMS.API.Modules.OperationHub.Models;

namespace SmartWMS.API.Modules.OperationHub.DTOs;

#region Session DTOs

public class OperatorSessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    // Device info
    public string? DeviceId { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set; }

    // Session
    public OperatorSessionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }

    // Current work
    public string? CurrentTaskType { get; set; }
    public Guid? CurrentTaskId { get; set; }
    public string? CurrentZone { get; set; }
    public string? CurrentLocation { get; set; }

    // Shift
    public string? ShiftCode { get; set; }
    public DateTime? ShiftStart { get; set; }
    public DateTime? ShiftEnd { get; set; }

    // Calculated
    public int SessionDurationMinutes { get; set; }
    public int IdleMinutes { get; set; }
}

public class StartSessionRequest
{
    public Guid WarehouseId { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set; }
    public string? ShiftCode { get; set; }
}

public class UpdateSessionStatusRequest
{
    public OperatorSessionStatus Status { get; set; }
    public string? CurrentZone { get; set; }
    public string? CurrentLocation { get; set; }
}

#endregion

#region Unified Task DTOs

public class UnifiedTaskDto
{
    public Guid Id { get; set; }
    public string TaskType { get; set; } = string.Empty; // Pick, Pack, Putaway, CycleCount
    public string TaskNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }

    // Location info
    public string? SourceLocation { get; set; }
    public string? DestinationLocation { get; set; }
    public string? Zone { get; set; }
    public string? Aisle { get; set; }

    // Product info (for pick/putaway)
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public decimal? Quantity { get; set; }
    public string? Uom { get; set; }

    // Container info
    public string? ContainerId { get; set; }
    public string? LpnNumber { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueBy { get; set; }

    // Parent reference
    public string? OrderNumber { get; set; }
    public string? BatchNumber { get; set; }
}

public class TaskQueueQueryParams
{
    public Guid? WarehouseId { get; set; }
    public string? TaskType { get; set; } // Pick, Pack, Putaway, CycleCount, All
    public string? Status { get; set; } // Pending, InProgress, All
    public string? Zone { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public bool UnassignedOnly { get; set; }
    public string? SortBy { get; set; } // Priority, DueBy, CreatedAt
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AssignTaskRequest
{
    public Guid TaskId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class StartTaskRequest
{
    public Guid TaskId { get; set; }
    public string TaskType { get; set; } = string.Empty;
}

public class CompleteTaskRequest
{
    public Guid TaskId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public decimal? ActualQuantity { get; set; }
    public string? Notes { get; set; }
    public string? ReasonCode { get; set; } // For short picks, etc.
}

public class PauseTaskRequest
{
    public Guid TaskId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

#endregion

#region Scan DTOs

public class ScanRequest
{
    public string Barcode { get; set; } = string.Empty;
    public ScanType ScanType { get; set; } = ScanType.Barcode;
    public ScanContext Context { get; set; }

    // Task context (optional)
    public string? TaskType { get; set; }
    public Guid? TaskId { get; set; }

    // Expected values for validation
    public string? ExpectedSku { get; set; }
    public string? ExpectedLocation { get; set; }
    public decimal? ExpectedQuantity { get; set; }

    // Device info
    public string? DeviceId { get; set; }
}

public class ScanResponse
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    // What was scanned
    public string EntityType { get; set; } = string.Empty; // Product, Location, Container, LPN
    public Guid? EntityId { get; set; }

    // Resolved info
    public string? ResolvedSku { get; set; }
    public string? ResolvedProductName { get; set; }
    public string? ResolvedLocation { get; set; }
    public string? ResolvedLpn { get; set; }

    // Additional info
    public decimal? AvailableQuantity { get; set; }
    public string? Uom { get; set; }

    // Validation result
    public bool MatchesExpected { get; set; }
    public string? ValidationMessage { get; set; }

    // Next action hint
    public string? NextAction { get; set; }
}

public class ScanLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? SessionId { get; set; }

    public string Barcode { get; set; } = string.Empty;
    public ScanType ScanType { get; set; }
    public ScanContext Context { get; set; }

    public string? EntityType { get; set; }
    public string? ResolvedSku { get; set; }
    public string? ResolvedLocation { get; set; }

    public string? TaskType { get; set; }
    public Guid? TaskId { get; set; }

    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public string? DeviceId { get; set; }
    public DateTime ScannedAt { get; set; }
}

public class ScanLogQueryParams
{
    public Guid? WarehouseId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public ScanContext? Context { get; set; }
    public bool? SuccessOnly { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

#endregion

#region Productivity DTOs

public class OperatorProductivityDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime Date { get; set; }

    // Tasks completed
    public int PickTasksCompleted { get; set; }
    public int PackTasksCompleted { get; set; }
    public int PutawayTasksCompleted { get; set; }
    public int CycleCountsCompleted { get; set; }
    public int TotalTasksCompleted { get; set; }

    // Quantities
    public decimal TotalUnitsPicked { get; set; }
    public decimal TotalUnitsPacked { get; set; }
    public decimal TotalUnitsPutaway { get; set; }
    public int TotalLocationsVisited { get; set; }

    // Time metrics
    public int TotalWorkMinutes { get; set; }
    public int TotalIdleMinutes { get; set; }
    public int TotalBreakMinutes { get; set; }
    public decimal ProductiveTimePercent { get; set; }

    // Accuracy
    public int TotalScans { get; set; }
    public int CorrectScans { get; set; }
    public int ErrorScans { get; set; }
    public decimal AccuracyRate { get; set; }

    // Speed metrics
    public decimal PicksPerHour { get; set; }
    public decimal UnitsPerHour { get; set; }
    public decimal TasksPerHour { get; set; }
}

public class ProductivityQueryParams
{
    public Guid? WarehouseId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? GroupBy { get; set; } // Day, Week, Month
}

public class ProductivitySummaryDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalOperators { get; set; }
    public int TotalWorkDays { get; set; }

    // Aggregated metrics
    public int TotalTasksCompleted { get; set; }
    public decimal TotalUnitsProcessed { get; set; }
    public int TotalWorkMinutes { get; set; }

    // Averages
    public decimal AvgTasksPerOperatorPerDay { get; set; }
    public decimal AvgUnitsPerOperatorPerDay { get; set; }
    public decimal AvgAccuracyRate { get; set; }
    public decimal AvgPicksPerHour { get; set; }

    // Top performers
    public List<TopPerformerDto> TopPerformers { get; set; } = new();
}

public class TopPerformerDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public decimal UnitsProcessed { get; set; }
    public decimal AccuracyRate { get; set; }
    public decimal AvgTasksPerHour { get; set; }
}

#endregion

#region Task Action Log DTOs

public class TaskActionLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }

    public string TaskType { get; set; } = string.Empty;
    public Guid TaskId { get; set; }
    public string TaskNumber { get; set; } = string.Empty;

    public TaskAction Action { get; set; }
    public DateTime ActionAt { get; set; }

    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? LocationCode { get; set; }
    public string? ProductSku { get; set; }
    public decimal? Quantity { get; set; }

    public int? DurationSeconds { get; set; }
    public string? Notes { get; set; }
    public string? ReasonCode { get; set; }
}

public class TaskActionLogQueryParams
{
    public Guid? WarehouseId { get; set; }
    public Guid? UserId { get; set; }
    public string? TaskType { get; set; }
    public Guid? TaskId { get; set; }
    public TaskAction? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

#endregion

#region Operator Stats DTOs

public class OperatorStatsDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }

    // Current session
    public OperatorSessionDto? CurrentSession { get; set; }
    public bool IsOnline { get; set; }
    public int MinutesSinceLastActivity { get; set; }

    // Today's stats
    public TodayStatsDto Today { get; set; } = new();

    // Current task
    public UnifiedTaskDto? CurrentTask { get; set; }
}

public class TodayStatsDto
{
    public int TasksCompleted { get; set; }
    public decimal UnitsProcessed { get; set; }
    public int WorkMinutes { get; set; }
    public int IdleMinutes { get; set; }
    public decimal AccuracyRate { get; set; }
    public decimal TasksPerHour { get; set; }
}

public class WarehouseOperatorsOverviewDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;

    // Summary
    public int TotalOperators { get; set; }
    public int OnlineOperators { get; set; }
    public int OnBreakOperators { get; set; }
    public int IdleOperators { get; set; }

    // Today's totals
    public int TotalTasksCompletedToday { get; set; }
    public decimal TotalUnitsProcessedToday { get; set; }
    public decimal AvgAccuracyToday { get; set; }

    // Active sessions
    public List<OperatorSessionDto> ActiveSessions { get; set; } = new();
}

#endregion
