# SmartWMS Backend - Task Tracker

> **Purpose**: Track current tasks, priorities, and roadmap.
> Update this file as tasks are completed or added.

---

## Current Sprint

### In Progress
- [ ] **Initialize project structure** - Create .NET 8 project with modules

### Next Up
- [ ] Create Auth infrastructure (JWT, Identity)
- [ ] Create base models and response types
- [ ] Implement Warehouse module

---

## Phase 1: Foundation (MVP)

### Infrastructure
- [x] Initialize Git repository
- [x] Create documentation (CLAUDE_CONTEXT, DEVELOPMENT_LOG, API_CONTRACTS, ARCHITECTURE)
- [ ] Create .NET 8 Web API project
- [ ] Setup project structure (modules, infrastructure, common)
- [ ] Configure PostgreSQL connection
- [ ] Setup Swagger/OpenAPI

### Authentication & Multi-tenancy
- [ ] Create ApplicationUser model
- [ ] Create ApplicationRole model
- [ ] Implement JWT authentication
- [ ] Implement refresh token flow
- [ ] Create RequireTenant attribute
- [ ] Create RequirePermission attribute
- [ ] Setup permission enum

### Core Module: Companies
- [ ] Company model (Tenant)
- [ ] Site model
- [ ] CompaniesController
- [ ] SitesController

### Core Module: Users
- [ ] User DTOs
- [ ] UsersController
- [ ] RolesController
- [ ] User management service

---

## Phase 2: Warehouse & Inventory

### Warehouse Module
- [ ] Warehouse model
- [ ] Zone model (with ZoneType enum)
- [ ] Location model (with LocationType enum)
- [ ] WarehousesController
- [ ] ZonesController
- [ ] LocationsController
- [ ] Location suggestion service (putaway)

### Inventory Module
- [ ] Product model
- [ ] ProductCategory model
- [ ] StockLevel model
- [ ] StockMovement model (with MovementType enum)
- [ ] ProductsController
- [ ] StockController
- [ ] Stock adjustment service
- [ ] Stock transfer service

---

## Phase 3: Orders

### Orders Module
- [ ] SalesOrder model (with SalesOrderStatus enum)
- [ ] SalesOrderLine model
- [ ] PurchaseOrder model (with PurchaseOrderStatus enum)
- [ ] PurchaseOrderLine model
- [ ] Customer model
- [ ] Supplier model
- [ ] SalesOrdersController
- [ ] PurchaseOrdersController
- [ ] CustomersController
- [ ] SuppliersController
- [ ] Order allocation service

---

## Phase 4: Fulfillment (KEY!)

### Fulfillment Module
- [ ] FulfillmentBatch model (with FulfillmentStatus, BatchType enums)
- [ ] FulfillmentOrder model (linking batch to orders)
- [ ] PickTask model (with TaskStatus enum)
- [ ] Shipment model (with ShipmentStatus enum)
- [ ] FulfillmentController
- [ ] ShipmentsController
- [ ] Wave creation service
- [ ] Pick task generation service
- [ ] Pick completion service

---

## Phase 5: Advanced Features (Future)

### Wave Optimization
- [ ] Pick path optimization algorithm
- [ ] Zone-based wave planning
- [ ] Batch consolidation logic

### Putaway Rules
- [ ] Rule engine for location selection
- [ ] ABC velocity analysis
- [ ] Capacity-based assignment

### Replenishment
- [ ] Min/max level monitoring
- [ ] Replenishment task generation
- [ ] Bulk-to-pick transfer

### Integrations
- [ ] Carrier API integration (DHL, FedEx, etc.)
- [ ] ERP sync framework
- [ ] E-commerce connectors

### Real-time
- [ ] SignalR hub setup
- [ ] Dashboard live updates
- [ ] Task notifications

---

## Backlog (Ideas)

- Labor management / productivity tracking
- Yard management
- Cross-docking workflows
- Returns processing (RMA)
- Quality control holds
- Cycle counting scheduling
- Barcode label templates
- Report builder
- Mobile-optimized API endpoints
- Offline sync support

---

## Completed

| Date | Task | Notes |
|------|------|-------|
| 2025-12-13 | Git repository initialized | Fresh start |
| 2025-12-13 | Documentation created | CLAUDE_CONTEXT, DEVELOPMENT_LOG, API_CONTRACTS, ARCHITECTURE |

---

*Last Updated: December 13, 2025*
