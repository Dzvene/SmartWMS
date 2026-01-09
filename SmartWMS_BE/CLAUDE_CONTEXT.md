# SmartWMS Backend - Claude Code Context Document

> **IMPORTANT FOR AI ASSISTANTS**: This is the PRIMARY context document for this project.
> Read this file FIRST before making any changes to understand the project structure,
> conventions, and source of truth for API contracts.

## Project Overview

**SmartWMS_BE** is a .NET 8 Web API backend for a modern Warehouse Management System.
This project is being developed with AI assistance (Claude Code) and follows strict
documentation and logging practices.

## Critical Information

### Source of Truth

| Aspect | Source | Path |
|--------|--------|------|
| **API Contracts & Types** | SmartWMS_UI | `/mnt/d/Projects/WarhausMS/SmartWMS_UI/src/types/` |
| **API Endpoints** | SmartWMS_UI | `/mnt/d/Projects/WarhausMS/SmartWMS_UI/src/api/endpoints/` |


### Terminology Rules

**ALWAYS USE** (from SmartWMS_UI):
- `Location` - storage position in warehouse (NOT "Bin")
- `Zone` - grouping of locations
- `Product` - SKU/item (NOT "StockCode" or "Item")
- `FulfillmentBatch` - wave/batch for order processing (NOT "DeliveryGroup" or "Proposal")
- `PickTask` - individual picking assignment
- `StockLevel` - inventory quantity at location
- `StockMovement` - inventory transaction

## Project Structure

```
SmartWMS_BE/
├── CLAUDE_CONTEXT.md          # THIS FILE - Read first!
├── DEVELOPMENT_LOG.md         # Chronological development log
├── API_CONTRACTS.md           # API specifications from SmartWMS_UI
├── ARCHITECTURE.md            # Architecture decisions and patterns
├── TODO.md                    # Current tasks and roadmap
├── SmartWMS.sln               # Solution file
├── SmartWMS.API.csproj        # Project file
├── Program.cs                 # Application entry point
├── appsettings.json           # Configuration
│
├── Common/                    # Shared code
│   ├── Models/                # Base entities, responses
│   ├── Attributes/            # Custom attributes
│   ├── Enums/                 # Domain enums
│   └── Extensions/            # Extension methods
│
├── Infrastructure/            # Cross-cutting concerns
│   ├── Data/                  # DbContext, configurations
│   ├── Identity/              # ASP.NET Identity customization
│   └── Services/              # Shared services
│
├── Modules/                   # Feature modules (vertical slices)
│   ├── Auth/                  # Authentication & authorization
│   ├── Companies/             # Multi-tenancy (Company/Tenant)
│   ├── Users/                 # User management
│   ├── Warehouse/             # Warehouse, Zone, Location
│   ├── Inventory/             # Product, StockLevel, StockMovement
│   ├── Orders/                # SalesOrder, PurchaseOrder
│   └── Fulfillment/           # FulfillmentBatch, PickTask, Shipment
│
└── SmartWMS.API.Tests/        # Test project
    ├── Infrastructure/        # Test utilities and base classes
    ├── Unit/                  # Unit tests (per module)
    └── Integration/           # Integration tests (per module)
```

## Module Structure Convention

Each module follows this structure:
```
ModuleName/
├── Controllers/
│   └── {Entity}Controller.cs
├── Services/
│   ├── I{Entity}Service.cs
│   └── {Entity}Service.cs
├── Models/
│   └── {Entity}.cs
├── DTOs/
│   ├── {Entity}Response.cs
│   ├── Create{Entity}Request.cs
│   └── Update{Entity}Request.cs
├── Configurations/
│   └── {Entity}Configuration.cs    # EF Core configuration
└── ModuleExtensions.cs             # DI registration
```

## API Conventions

### URL Structure
```
/api/v1/auth/...                           # Public auth endpoints
/api/v1/tenant/{tenantId}/...              # Tenant-scoped endpoints
/api/v1/tenant/{tenantId}/site/{siteId}/...  # Site-scoped (if needed)
```

### Response Format
```json
{
  "success": true,
  "message": "Operation completed",
  "data": { ... },
  "errors": []
}
```

### Pagination
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 25,
  "totalCount": 100,
  "totalPages": 4
}
```

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | PostgreSQL | 16+ |
| Auth | JWT + ASP.NET Identity | - |
| API Docs | Swagger/OpenAPI | - |
| Validation | FluentValidation | - |
| Testing | xUnit + FluentAssertions + Moq | - |

## Development Guidelines

### Commit Message Format
```
<type>: <description>

<body>

🤖 Generated with Claude Code (claude-opus-4-5-20251101)
Co-Authored-By: Claude <noreply@anthropic.com>
```

Types: `feat`, `fix`, `refactor`, `docs`, `chore`, `test`

### Before Making Changes

1. Check `DEVELOPMENT_LOG.md` for recent changes
2. Verify types match `SmartWMS_UI/src/types/`
3. Update `TODO.md` if adding new tasks
4. Log significant changes in `DEVELOPMENT_LOG.md`

### When Creating New Modules

1. Copy structure from existing module
2. Create types matching SmartWMS_UI exactly
3. Register in `Program.cs`
4. Add to `ARCHITECTURE.md`
5. Log in `DEVELOPMENT_LOG.md`
6. **Add tests** (see Testing section below)

## Key SmartWMS_UI Type Files

Reference these files for correct type definitions:

| Domain | File Path |
|--------|-----------|
| Warehouse | `SmartWMS_UI/src/types/warehouse/location.types.ts` |
| Inventory | `SmartWMS_UI/src/types/inventory/product.types.ts` |
| Stock | `SmartWMS_UI/src/types/inventory/stock.types.ts` |
| Sales Orders | `SmartWMS_UI/src/types/orders/sales-order.types.ts` |
| Purchase Orders | `SmartWMS_UI/src/types/orders/purchase-order.types.ts` |
| Fulfillment | `SmartWMS_UI/src/types/orders/fulfillment.types.ts` |
| Common | `SmartWMS_UI/src/models/common.ts` |

## Testing

### Test Project Structure

```
SmartWMS.API.Tests/
├── Infrastructure/
│   ├── CustomWebApplicationFactory.cs  # WebApplicationFactory with InMemory DB
│   ├── IntegrationTestBase.cs          # Base class for integration tests
│   └── TestDataSeeder.cs               # Seed test data (users, products, etc.)
├── Unit/{ModuleName}/
│   └── {ServiceName}Tests.cs           # Unit tests for services
└── Integration/{ModuleName}/
    └── {ControllerName}Tests.cs        # Integration tests for controllers
```

### Testing Strategy

| Test Type | Purpose | Tools | Coverage |
|-----------|---------|-------|----------|
| **Unit** | Business logic in isolation | xUnit, Moq, FluentAssertions | Services |
| **Integration** | Full HTTP pipeline | WebApplicationFactory, InMemory DB | Controllers |

### Running Tests

```bash
# All tests
dotnet test SmartWMS.API.Tests

# Unit tests only
dotnet test SmartWMS.API.Tests --filter "FullyQualifiedName~Unit"

# Integration tests only
dotnet test SmartWMS.API.Tests --filter "FullyQualifiedName~Integration"

# Specific module
dotnet test SmartWMS.API.Tests --filter "FullyQualifiedName~Auth"
```

### Test Coverage by Module

| Module | Unit Tests | Integration Tests | Status |
|--------|------------|-------------------|--------|
| Auth | AuthServiceTests (11) | AuthControllerTests (10) | ✅ Done |
| Products | ProductsServiceTests (27) | ProductsControllerTests (18) | ✅ Done |
| Users | - | - | ⏳ Pending |
| Sites | - | - | ⏳ Pending |
| Warehouses | - | - | ⏳ Pending |
| Zones | - | - | ⏳ Pending |
| Locations | - | - | ⏳ Pending |
| ProductCategories | - | - | ⏳ Pending |

### Writing Tests for New Modules

**Unit Tests** (`Unit/{Module}/{Service}Tests.cs`):
```csharp
public class {Service}Tests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly {Service} _service;

    public {Service}Tests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new {Service}(_context);
        SeedTestData();
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        // Act
        // Assert using FluentAssertions
    }
}
```

**Integration Tests** (`Integration/{Module}/{Controller}Tests.cs`):
```csharp
public class {Controller}Tests : IntegrationTestBase
{
    public {Controller}Tests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Endpoint_Scenario_ExpectedResult()
    {
        await AuthenticateAsync(); // Get JWT token
        var response = await Client.GetAsync(TenantUrl("/endpoint"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### What to Test

**Unit Tests (Services):**
- CRUD operations (Create, Read, Update, Delete)
- Validation logic (duplicate SKU, invalid category, etc.)
- Business rules (stock calculations, permissions)
- Edge cases (empty results, not found, wrong tenant)

**Integration Tests (Controllers):**
- Authentication required (401 without token)
- Authorization (403 for wrong tenant)
- Success cases (200, 201)
- Error cases (400, 404)
- Pagination and filtering

### What NOT to Test

- EF Core configurations (trust the framework)
- Simple DTOs without logic
- Every parameter combination (test representative cases)
- Third-party libraries

## Future Enhancements (Planned)

These are architected but not yet implemented:

1. **Wave Optimization** - Algorithm for optimal pick path
2. **Putaway Rules Engine** - Automatic location assignment
3. **Replenishment** - Auto-replenish pick locations
4. **Labor Management** - Track worker productivity
5. **Carrier Integration** - DHL, FedEx, UPS APIs
6. **Real-time Updates** - SignalR for live dashboards

## Contact & Support

- **Project Owner**: [Your Name]
- **AI Assistant**: Claude Code (Anthropic)
- **Model**: claude-opus-4-5-20251101

---

*Last Updated: December 15, 2025*
*Document Version: 1.1* - Added Testing section
