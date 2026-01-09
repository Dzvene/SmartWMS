# Warehouse Automation - Future Implementation

## Overview

This document outlines the future architecture for warehouse automation integration in SmartWMS.

## Automation Levels

### Level 1: Basic Mechanization (Current)
- Manual forklifts
- Hand scanners
- Label printers
- Basic conveyor

### Level 2: Semi-Automation (Phase 2)
- Pick-to-light / Put-to-light
- Voice picking
- Automated conveyors with routing
- Automated sortation

### Level 3: Full Automation (Phase 3)
- AS/RS (Automated Storage & Retrieval Systems)
- AGV/AMR (Autonomous Mobile Robots)
- Goods-to-person systems
- Robotic picking

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      ERP / OMS                               │
│              (SAP, Oracle, Shopify, etc.)                    │
└─────────────────────────┬───────────────────────────────────┘
                          │ Orders, Products, Customers
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    SmartWMS (WMS Layer)                      │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐    │
│  │  Inventory  │ │   Orders    │ │  Fulfillment Logic  │    │
│  │ Management  │ │ Management  │ │  (Wave, Batch, etc) │    │
│  └─────────────┘ └─────────────┘ └─────────────────────┘    │
└─────────────────────────┬───────────────────────────────────┘
                          │ Tasks, Instructions
                          ▼
┌─────────────────────────────────────────────────────────────┐
│              WES (Warehouse Execution System)                │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐    │
│  │    Task     │ │  Resource   │ │    Optimization     │    │
│  │   Queue     │ │ Allocation  │ │  (Routing, Batching)│    │
│  └─────────────┘ └─────────────┘ └─────────────────────┘    │
└─────────────────────────┬───────────────────────────────────┘
                          │ Equipment Commands
                          ▼
┌─────────────────────────────────────────────────────────────┐
│              WCS (Warehouse Control System)                  │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐    │
│  │     PLC     │ │   Device    │ │     Real-time       │    │
│  │ Integration │ │  Protocols  │ │    Monitoring       │    │
│  └─────────────┘ └─────────────┘ └─────────────────────┘    │
└───────┬─────────────────┬─────────────────┬─────────────────┘
        │                 │                 │
        ▼                 ▼                 ▼
   ┌─────────┐      ┌──────────┐      ┌──────────┐
   │ AS/RS   │      │ Conveyor │      │ AGV/AMR  │
   │ Systems │      │ Sortation│      │ Fleet    │
   └─────────┘      └──────────┘      └──────────┘
```

---

## Data Models for Automation

### Equipment Base Model (Extended)

```csharp
public enum EquipmentCategory
{
    Manual,           // Forklifts, pallet jacks (human operated)
    SemiAutomated,    // Pick-to-light, voice picking terminals
    FullyAutomated    // AS/RS, AGV, robots
}

public enum EquipmentStatus
{
    Offline,
    Idle,
    Working,
    Error,
    Maintenance,
    Charging
}

public class Equipment
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public EquipmentType Type { get; set; }
    public EquipmentCategory Category { get; set; }
    public EquipmentStatus Status { get; set; }

    // Assignment
    public Guid WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }
    public Guid? AssignedUserId { get; set; }

    // For mobile equipment (AGV, forklifts)
    public Guid? CurrentLocationId { get; set; }
    public decimal? CurrentX { get; set; }  // For precise positioning
    public decimal? CurrentY { get; set; }

    // Automation integration
    public string? ControllerAddress { get; set; }  // IP, PLC address
    public string? Protocol { get; set; }           // OPC-UA, Modbus, REST, MQTT
    public string? DeviceId { get; set; }           // Unique ID in control system

    // Telemetry
    public int? BatteryLevel { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public JsonDocument? Telemetry { get; set; }    // Flexible telemetry data

    // Maintenance
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int? OperatingHours { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Equipment Task Queue

```csharp
public enum TaskType
{
    // Manual tasks
    Pick,
    Putaway,
    Replenishment,
    Transfer,
    Count,

    // Automated tasks
    Retrieve,       // AS/RS retrieve
    Store,          // AS/RS store
    Transport,      // AGV move
    Sort,           // Sortation
    Sequence        // Sequencing buffer
}

public enum TaskStatus
{
    Pending,
    Assigned,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public class EquipmentTask
{
    public Guid Id { get; set; }
    public string TaskNumber { get; set; }
    public TaskType Type { get; set; }
    public TaskStatus Status { get; set; }
    public int Priority { get; set; }  // 1-10, higher = more urgent

    // Assignment
    public Guid? EquipmentId { get; set; }
    public Guid? UserId { get; set; }

    // What to do
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? LicensePlateId { get; set; }  // Pallet/container ID
    public decimal? Quantity { get; set; }

    // Related documents
    public Guid? SalesOrderId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? FulfillmentOrderId { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueAt { get; set; }

    // Execution details
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public JsonDocument? ExecutionLog { get; set; }
}
```

### Equipment Type Definitions

```csharp
public class EquipmentTypeDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public EquipmentCategory Category { get; set; }

    // Capabilities
    public bool CanPick { get; set; }
    public bool CanPutaway { get; set; }
    public bool CanTransport { get; set; }
    public bool CanLift { get; set; }

    // Constraints
    public decimal? MaxWeight { get; set; }
    public decimal? MaxHeight { get; set; }
    public decimal? MaxWidth { get; set; }

    // Schema for type-specific properties
    public JsonDocument? PropertiesSchema { get; set; }
}
```

---

## Integration Protocols

### Supported Protocols (Future)

| Protocol | Use Case | Equipment Types |
|----------|----------|-----------------|
| OPC-UA | Industrial standard | AS/RS, Conveyors, PLCs |
| Modbus TCP | Legacy PLCs | Older equipment |
| REST API | Modern systems | AGVs, AMRs, Cloud systems |
| MQTT | Real-time telemetry | IoT sensors, trackers |
| VDA 5050 | AGV standard | AGVs, AMRs |

### Integration Points

```csharp
public interface IEquipmentController
{
    Task<bool> ConnectAsync(Equipment equipment);
    Task<bool> DisconnectAsync(Equipment equipment);
    Task<EquipmentStatus> GetStatusAsync(Equipment equipment);
    Task<bool> SendCommandAsync(Equipment equipment, EquipmentCommand command);
    Task<TelemetryData> GetTelemetryAsync(Equipment equipment);
}

public class EquipmentCommand
{
    public string CommandType { get; set; }  // "MOVE", "PICK", "STORE", etc.
    public JsonDocument Parameters { get; set; }
    public int TimeoutSeconds { get; set; }
}
```

---

## Real-time Monitoring

### Event Types

```csharp
public enum EquipmentEventType
{
    StatusChanged,
    TaskStarted,
    TaskCompleted,
    TaskFailed,
    PositionUpdated,
    ErrorOccurred,
    MaintenanceRequired,
    BatteryLow
}

public class EquipmentEvent
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public EquipmentEventType EventType { get; set; }
    public string Message { get; set; }
    public JsonDocument? Data { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### SignalR Hub (Real-time updates)

```csharp
public interface IEquipmentHub
{
    Task OnStatusChanged(Guid equipmentId, EquipmentStatus status);
    Task OnPositionUpdated(Guid equipmentId, decimal x, decimal y);
    Task OnTaskCompleted(Guid equipmentId, Guid taskId);
    Task OnError(Guid equipmentId, string errorMessage);
    Task OnTelemetryUpdate(Guid equipmentId, TelemetryData data);
}
```

---

## Implementation Phases

### Phase 1: Foundation (Current)
- [ ] Basic Equipment CRUD
- [ ] Equipment types (manual)
- [ ] Assignment to warehouse/zone

### Phase 2: Task Management
- [ ] Equipment task queue
- [ ] Task assignment (manual)
- [ ] Task tracking and history

### Phase 3: Semi-Automation
- [ ] Pick-to-light integration
- [ ] Voice picking support
- [ ] Basic conveyor control

### Phase 4: Full Automation
- [ ] AS/RS integration (OPC-UA)
- [ ] AGV fleet management (VDA 5050)
- [ ] Real-time monitoring dashboard
- [ ] Predictive maintenance

---

## References

- VDA 5050: AGV Communication Interface
- OPC UA: Open Platform Communications Unified Architecture
- MQTT: Message Queuing Telemetry Transport
- Material Handling Industry (MHI) Standards
