using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.OperationHub.Models;

/// <summary>
/// Operator session - tracks active operator work session
/// </summary>
public class OperatorSession : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid WarehouseId { get; set; }

    // Device info
    public string? DeviceId { get; set; }
    public string? DeviceType { get; set; } // Scanner, Mobile, Tablet, Desktop
    public string? DeviceName { get; set; }

    // Session
    public OperatorSessionStatus Status { get; set; } = OperatorSessionStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }

    // Current work
    public string? CurrentTaskType { get; set; } // Pick, Pack, Putaway, CycleCount
    public Guid? CurrentTaskId { get; set; }
    public string? CurrentZone { get; set; }
    public string? CurrentLocation { get; set; }

    // Shift
    public string? ShiftCode { get; set; }
    public DateTime? ShiftStart { get; set; }
    public DateTime? ShiftEnd { get; set; }
}

/// <summary>
/// Operator productivity - daily stats for an operator
/// </summary>
public class OperatorProductivity : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime Date { get; set; }

    // Tasks completed
    public int PickTasksCompleted { get; set; }
    public int PackTasksCompleted { get; set; }
    public int PutawayTasksCompleted { get; set; }
    public int CycleCountsCompleted { get; set; }

    // Quantities
    public decimal TotalUnitsPicked { get; set; }
    public decimal TotalUnitsPacked { get; set; }
    public decimal TotalUnitsPutaway { get; set; }
    public int TotalLocationsVisited { get; set; }

    // Time metrics (in minutes)
    public int TotalWorkMinutes { get; set; }
    public int TotalIdleMinutes { get; set; }
    public int TotalBreakMinutes { get; set; }

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

/// <summary>
/// Scan log - every barcode scan by operator
/// </summary>
public class ScanLog : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid WarehouseId { get; set; }

    // Scan info
    public string Barcode { get; set; } = string.Empty;
    public ScanType ScanType { get; set; }
    public ScanContext Context { get; set; }

    // What was scanned
    public string? EntityType { get; set; } // Product, Location, Container, LPN
    public Guid? EntityId { get; set; }
    public string? ResolvedSku { get; set; }
    public string? ResolvedLocation { get; set; }

    // Task context
    public string? TaskType { get; set; }
    public Guid? TaskId { get; set; }

    // Result
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    // Device
    public string? DeviceId { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Task action log - tracks operator actions on tasks
/// </summary>
public class TaskActionLog : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid WarehouseId { get; set; }

    // Task info
    public string TaskType { get; set; } = string.Empty; // Pick, Pack, Putaway, CycleCount
    public Guid TaskId { get; set; }
    public string TaskNumber { get; set; } = string.Empty;

    // Action
    public TaskAction Action { get; set; }
    public DateTime ActionAt { get; set; } = DateTime.UtcNow;

    // Details
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? LocationCode { get; set; }
    public string? ProductSku { get; set; }
    public decimal? Quantity { get; set; }

    // Duration (seconds from previous action)
    public int? DurationSeconds { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? ReasonCode { get; set; }
}

#region Enums

public enum OperatorSessionStatus
{
    Active,
    OnBreak,
    Idle,
    Offline,
    Ended
}

public enum ScanType
{
    Barcode,
    QRCode,
    RFID,
    Manual
}

public enum ScanContext
{
    Login,
    LocationVerify,
    ProductPick,
    ProductPutaway,
    ProductPack,
    ContainerScan,
    LPNScan,
    CycleCount,
    Transfer,
    Receiving,
    Shipping,
    Other
}

public enum TaskAction
{
    Assigned,
    Started,
    Paused,
    Resumed,
    LocationArrived,
    ProductScanned,
    QuantityConfirmed,
    ShortPicked,
    Completed,
    Cancelled,
    Reassigned,
    Error
}

#endregion
