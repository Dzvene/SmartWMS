# Racking (Storage Structures) Architecture

## Problem

Current `Location` model handles **addressing** (where to put/pick items), but doesn't capture **physical structure** of racking:
- Rack type and characteristics
- Weight capacity per level
- Physical dimensions
- Maintenance/inspection tracking

## Solution: Separate Concerns

```
┌─────────────────────────────────────────────────────────────┐
│                      LOGICAL LAYER                          │
│                   (Addressing System)                        │
│                                                              │
│   Zone ──► Location (A-01-02-03)                            │
│            - Where to put/pick                               │
│            - Stock levels                                    │
│            - Pick/putaway sequences                          │
└─────────────────────────┬───────────────────────────────────┘
                          │ References
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                     PHYSICAL LAYER                          │
│                  (Storage Structure)                         │
│                                                              │
│   StorageStructure (Rack/Shelving)                          │
│   - Physical characteristics                                 │
│   - Capacity limits                                          │
│   - Maintenance schedule                                     │
└─────────────────────────────────────────────────────────────┘
```

## Data Model

### StorageStructure (Rack)

```csharp
public enum StorageStructureType
{
    // Pallet Storage
    SelectivePalletRack,    // Standard single-deep pallet rack
    DoublDeepRack,          // Double-deep pallet rack
    DriveInRack,            // Drive-in/drive-through
    PushBackRack,           // Push-back system
    PalletFlowRack,         // Gravity flow (FIFO)

    // Shelving
    StaticShelving,         // Standard shelving
    MobileShelving,         // Compactus/mobile shelving
    MultiTierShelving,      // Mezzanine with shelving

    // Specialized
    CantileverRack,         // For long items (pipes, lumber)
    CartonFlowRack,         // Carton-level gravity flow
    VLM,                    // Vertical Lift Module
    Carousel,               // Horizontal/vertical carousel

    // Floor
    FloorStorage,           // Block stacking on floor
    StagingArea             // No racking, open area
}

public enum StructureStatus
{
    Active,
    Maintenance,
    Damaged,
    Decommissioned
}

public class StorageStructure
{
    public Guid Id { get; set; }
    public string Code { get; set; }              // "RACK-A01", "SHELF-B03"
    public string? Name { get; set; }
    public StorageStructureType Type { get; set; }
    public StructureStatus Status { get; set; }

    // Hierarchy
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }
    public Guid? ZoneId { get; set; }
    public Zone? Zone { get; set; }
    public string? Aisle { get; set; }            // Which aisle

    // Physical Dimensions
    public int? Bays { get; set; }                // Number of bays (horizontal sections)
    public int? Levels { get; set; }              // Number of levels (vertical)
    public decimal? TotalHeightMm { get; set; }   // Total height in mm
    public decimal? TotalWidthMm { get; set; }    // Total width in mm
    public decimal? TotalDepthMm { get; set; }    // Total depth in mm

    // Capacity (per position/cell)
    public decimal? MaxWeightPerLevel { get; set; }   // kg per level
    public decimal? MaxWeightTotal { get; set; }      // Total rack capacity kg
    public int? PositionsPerLevel { get; set; }       // Positions per level

    // Beam/Shelf dimensions
    public decimal? BeamHeightMm { get; set; }        // Clear height between beams
    public decimal? BeamWidthMm { get; set; }         // Width between uprights
    public decimal? BeamDepthMm { get; set; }         // Depth of beam

    // Installation & Maintenance
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? InstallationDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDate { get; set; }
    public string? InspectionNotes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Location> Locations { get; set; }
}
```

### Updated Location Model

```csharp
public class Location
{
    // ... existing fields ...

    // NEW: Link to physical structure
    public Guid? StorageStructureId { get; set; }
    public StorageStructure? StorageStructure { get; set; }

    // Position within structure
    public int? BayNumber { get; set; }           // Which bay (1, 2, 3...)
    public int? LevelNumber { get; set; }         // Which level (1, 2, 3...)
    public int? PositionNumber { get; set; }      // Which position on level

    // Override structure defaults if needed
    public decimal? OverrideMaxWeight { get; set; }
    public decimal? OverrideHeight { get; set; }
}
```

### StorageStructureInspection (Maintenance Tracking)

```csharp
public enum InspectionResult
{
    Passed,
    MinorIssues,
    MajorIssues,
    Failed
}

public class StorageStructureInspection
{
    public Guid Id { get; set; }
    public Guid StorageStructureId { get; set; }
    public StorageStructure StorageStructure { get; set; }

    public DateTime InspectionDate { get; set; }
    public string InspectorName { get; set; }
    public InspectionResult Result { get; set; }

    // Checklist results
    public bool? UprightsOk { get; set; }
    public bool? BeamsOk { get; set; }
    public bool? BracingOk { get; set; }
    public bool? AnchoringOk { get; set; }
    public bool? SafetyPinsOk { get; set; }
    public bool? LabelsOk { get; set; }

    public string? Notes { get; set; }
    public string? ActionRequired { get; set; }
    public DateTime? ActionDueDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

## Relationships

```
Warehouse (1) ──────────────────────► (*) StorageStructure
     │                                        │
     │                                        │
     ▼                                        ▼
Zone (1) ──► (*) Location (*) ◄──────── (1) StorageStructure
                   │
                   ▼
            StockLevel (*)
```

## Benefits

1. **Capacity Management**
   - Know actual weight limits before putaway
   - Prevent overloading
   - Plan cube utilization

2. **Maintenance Compliance**
   - Track inspection schedules (OSHA/HSE requirements)
   - Document damage and repairs
   - Audit trail

3. **Warehouse Layout**
   - Visualize physical layout
   - Plan expansions
   - Optimize slotting based on rack type

4. **Reporting**
   - Rack utilization %
   - Capacity by zone/type
   - Maintenance costs

## Example Usage

### Creating a Pallet Rack Section

```csharp
var rackSection = new StorageStructure
{
    Code = "RACK-A01",
    Name = "Aisle A, Section 1",
    Type = StorageStructureType.SelectivePalletRack,
    WarehouseId = warehouseId,
    ZoneId = bulkZoneId,
    Aisle = "A",

    Bays = 10,                    // 10 bays wide
    Levels = 4,                   // 4 levels high
    PositionsPerLevel = 3,        // 3 pallet positions per level

    TotalHeightMm = 8000,         // 8 meters tall
    TotalWidthMm = 27500,         // 27.5 meters wide
    TotalDepthMm = 1100,          // 1.1 meter deep

    BeamHeightMm = 1800,          // 1.8m clear height
    BeamWidthMm = 2700,           // 2.7m beam span

    MaxWeightPerLevel = 3000,     // 3000 kg per level
    MaxWeightTotal = 120000,      // 120 tons total

    Manufacturer = "Mecalux",
    InstallationDate = new DateTime(2023, 1, 15)
};
```

### Auto-generate Locations from Structure

```csharp
public async Task GenerateLocationsForStructure(StorageStructure structure)
{
    for (int bay = 1; bay <= structure.Bays; bay++)
    {
        for (int level = 1; level <= structure.Levels; level++)
        {
            for (int pos = 1; pos <= structure.PositionsPerLevel; pos++)
            {
                var location = new Location
                {
                    Code = $"{structure.Aisle}-{bay:D2}-{level:D2}-{pos:D2}",
                    WarehouseId = structure.WarehouseId,
                    ZoneId = structure.ZoneId,
                    StorageStructureId = structure.Id,

                    Aisle = structure.Aisle,
                    Rack = bay.ToString("D2"),
                    Level = level.ToString("D2"),
                    Position = pos.ToString("D2"),

                    BayNumber = bay,
                    LevelNumber = level,
                    PositionNumber = pos,

                    // Inherit from structure
                    MaxWeight = structure.MaxWeightPerLevel / structure.PositionsPerLevel,
                    HeightMm = structure.BeamHeightMm,
                    WidthMm = structure.BeamWidthMm / structure.PositionsPerLevel,
                    DepthMm = structure.BeamDepthMm,

                    // Set type based on level
                    LocationType = level == 1
                        ? LocationType.Pick
                        : LocationType.Bulk,

                    IsActive = true
                };

                await _context.Locations.AddAsync(location);
            }
        }
    }

    await _context.SaveChangesAsync();
}
```

## Implementation Priority

### Phase 1 (MVP) - Current
- [x] Location with Aisle/Rack/Level/Position
- [x] Basic capacity (MaxWeight, MaxVolume)
- [x] Location types

### Phase 2 (Recommended Next)
- [ ] StorageStructure entity
- [ ] Link Location to StorageStructure
- [ ] Auto-generate locations from structure
- [ ] Structure management UI

### Phase 3 (Future)
- [ ] Inspection tracking
- [ ] Maintenance scheduling
- [ ] Visual layout editor
- [ ] Capacity utilization reports
