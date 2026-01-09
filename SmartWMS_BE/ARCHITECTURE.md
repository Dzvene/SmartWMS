# SmartWMS Backend - Architecture Document

> **Purpose**: Document architectural decisions, patterns, and design rationale.

---

## 1. High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              SmartWMS System                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │  SmartWMS   │    │  SmartWMS   │    │   Mobile    │    │  External   │  │
│  │     UI      │    │   Backend   │    │    Apps     │    │   Systems   │  │
│  │  (React)    │    │  (.NET 8)   │    │  (Future)   │    │  (ERP/TMS)  │  │
│  └──────┬──────┘    └──────┬──────┘    └──────┬──────┘    └──────┬──────┘  │
│         │                  │                  │                  │          │
│         └──────────────────┴──────────────────┴──────────────────┘          │
│                                    │                                        │
│                           ┌────────┴────────┐                               │
│                           │   REST API      │                               │
│                           │   (JSON/JWT)    │                               │
│                           └────────┬────────┘                               │
│                                    │                                        │
│  ┌─────────────────────────────────┴─────────────────────────────────────┐  │
│  │                         SmartWMS.API                                   │  │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐          │  │
│  │  │Controllers│  │ Services  │  │  Models   │  │   Data    │          │  │
│  │  └─────┬─────┘  └─────┬─────┘  └─────┬─────┘  └─────┬─────┘          │  │
│  │        └──────────────┴──────────────┴──────────────┘                 │  │
│  └───────────────────────────────────┬───────────────────────────────────┘  │
│                                      │                                      │
│                             ┌────────┴────────┐                             │
│                             │   PostgreSQL    │                             │
│                             │    Database     │                             │
│                             └─────────────────┘                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Module Architecture (Vertical Slices)

Each business domain is a self-contained module:

```
Module/
├── Controllers/           # HTTP endpoints
│   └── EntityController.cs
├── Services/              # Business logic
│   ├── IEntityService.cs  # Interface
│   └── EntityService.cs   # Implementation
├── Models/                # Domain entities (EF Core)
│   └── Entity.cs
├── DTOs/                  # Data transfer objects
│   ├── EntityResponse.cs
│   ├── CreateEntityRequest.cs
│   └── UpdateEntityRequest.cs
├── Configurations/        # EF Core configurations
│   └── EntityConfiguration.cs
├── Validators/            # FluentValidation (optional)
│   └── CreateEntityValidator.cs
└── ModuleExtensions.cs    # DI registration
```

### Benefits:
- **Cohesion**: Related code stays together
- **Isolation**: Modules don't depend on each other directly
- **Scalability**: Easy to extract to microservice later
- **Maintainability**: Changes localized to module

---

## 3. Multi-Tenancy Architecture

### Tenant Hierarchy
```
Company (Tenant)
    └── Site(s)
        └── Warehouse(s)
            └── Zone(s)
                └── Location(s)
```

### Implementation
```csharp
// Every tenant-scoped entity has:
public Guid TenantId { get; set; }

// URL pattern:
/api/v1/tenant/{tenantId}/...

// Middleware validates:
1. JWT contains TenantId claim
2. Route tenantId matches claim
3. All queries filtered by TenantId
```

### Data Isolation
- **Row-level**: All tables have TenantId column
- **Query filter**: EF Core global query filter
- **Validation**: Middleware blocks cross-tenant access

---

## 4. Authentication & Authorization

### Authentication Flow
```
┌────────┐     ┌─────────┐     ┌──────────┐     ┌──────────┐
│ Client │────>│  Login  │────>│ Validate │────>│  Issue   │
│        │     │ Request │     │  Creds   │     │   JWT    │
└────────┘     └─────────┘     └──────────┘     └────┬─────┘
                                                     │
┌────────┐     ┌─────────┐     ┌──────────┐         │
│ Client │<────│   JWT   │<────│  Return  │<────────┘
│        │     │  Token  │     │  Token   │
└────────┘     └─────────┘     └──────────┘
```

### JWT Claims
```json
{
  "sub": "user-guid",
  "email": "user@example.com",
  "tenant_id": "tenant-guid",
  "warehouse_id": "warehouse-guid",
  "roles": ["Admin", "WarehouseManager"],
  "permissions": ["Products.View", "Products.Edit", ...]
}
```

### Authorization Layers
1. **Route-level**: `[Authorize]` attribute
2. **Tenant-level**: `[RequireTenant]` attribute
3. **Permission-level**: `[RequirePermission("Products.Edit")]`

---

## 5. Database Design Principles

### Entity Base Class
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
}
```

### Soft Delete Pattern
```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}
```

### Audit Trail
```csharp
// Automatic via SaveChanges override
- Track all changes to entities
- Store in AuditLog table
- Include: Entity, Action, OldValues, NewValues, UserId, Timestamp
```

---

## 6. API Design Principles

### RESTful Conventions
```
GET    /resources          # List (with pagination)
GET    /resources/{id}     # Get single
POST   /resources          # Create
PUT    /resources/{id}     # Full update
PATCH  /resources/{id}     # Partial update
DELETE /resources/{id}     # Delete
```

### Response Consistency
```csharp
// Success
{
  "success": true,
  "message": "Product created successfully",
  "data": { ... }
}

// Error
{
  "success": false,
  "message": "Validation failed",
  "errors": ["SKU is required", "Name too long"]
}

// Paginated
{
  "items": [...],
  "page": 1,
  "pageSize": 25,
  "totalCount": 150,
  "totalPages": 6
}
```

### Versioning
- URL path versioning: `/api/v1/`, `/api/v2/`
- Allows breaking changes in new versions
- Old versions maintained for backwards compatibility

---

## 7. Core Domain Models

### Warehouse Domain
```
Warehouse ─────┬───── Zone ─────┬───── Location
               │                │
               │                └───── LocationType
               │                       (BULK, PICK, STAGING, etc.)
               │
               └───── ZoneType
                      (STORAGE, PICKING, PACKING, etc.)
```

### Inventory Domain
```
Product ───────┬───── StockLevel ───── Location
               │           │
               │           └───── Batch/Serial tracking
               │
               └───── StockMovement
                      (RECEIPT, ISSUE, TRANSFER, ADJUSTMENT)
```

### Order Domain
```
SalesOrder ────┬───── SalesOrderLine ───── Product
               │
               └───── Customer

PurchaseOrder ─┬───── PurchaseOrderLine ── Product
               │
               └───── Supplier
```

### Fulfillment Domain (KEY!)
```
FulfillmentBatch ──┬──── FulfillmentOrder ──── SalesOrder
                   │
                   └──── PickTask ────┬───── Product
                                      ├───── FromLocation
                                      └───── ToLocation (staging)

Shipment ──────────┬──── ShipmentLine
                   │
                   └──── Carrier
```

---

## 8. Future Architecture Considerations

### Wave Optimization Engine
```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Orders    │────>│    Wave     │────>│  Optimized  │
│   Queue     │     │  Planner    │     │   Tasks     │
└─────────────┘     └──────┬──────┘     └─────────────┘
                           │
                    ┌──────┴──────┐
                    │ Optimization │
                    │  Algorithm   │
                    │ (TSP-based)  │
                    └─────────────┘
```

### Putaway Rules Engine
```csharp
// Rule types:
- By Product Category → Zone
- By Velocity (ABC) → Location Type
- By Size/Weight → Location Capacity
- By Expiry → FEFO Zone
- By Customer Dedication → Reserved Locations
```

### Real-time Architecture (Future)
```
┌────────┐     ┌─────────┐     ┌──────────┐
│ Client │<───>│ SignalR │<───>│  Event   │
│  (UI)  │     │   Hub   │     │  Bus     │
└────────┘     └─────────┘     └────┬─────┘
                                    │
                              ┌─────┴─────┐
                              │  Domain   │
                              │  Events   │
                              └───────────┘
```

---

## 9. Performance Considerations

### Database
- Indexes on TenantId + frequently queried columns
- Composite indexes for common query patterns
- Consider partitioning for large tables (StockMovement)

### Caching (Future)
- Redis for session/token caching
- Response caching for reference data
- Query result caching for reports

### Scalability
- Stateless API design
- Horizontal scaling ready
- Database connection pooling

---

## 10. Security Measures

### Authentication
- JWT with short expiry (1 hour)
- Refresh tokens (7 days, single use)
- Password hashing (BCrypt)
- Account lockout after failed attempts

### Authorization
- Role-based access control (RBAC)
- Permission-based fine-grained control
- Tenant isolation enforced at all levels

### Data Protection
- HTTPS only
- Sensitive data encryption at rest
- PII handling compliance ready
- Audit logging for compliance

---

## 11. Decision Log

| Date | Decision | Rationale | Alternatives Considered |
|------|----------|-----------|------------------------|
| 2025-12-13 | Use Location (not Bin) | Standard WMS terminology | Bin (legacy) |
| 2025-12-13 | FulfillmentBatch for wave processing | Modern approach | DeliveryGroup (legacy) |
| 2025-12-13 | Vertical slice architecture | Better cohesion | Traditional layered |
| 2025-12-13 | PostgreSQL database | Open source, JSON support | SQL Server, MySQL |
| 2025-12-13 | JWT authentication | Stateless, scalable | Session-based |

---

*Last Updated: December 13, 2025*
*Architecture Version: 1.0*
