# SmartWMS Backend - API Contracts

> **Source**: All contracts derived from SmartWMS_UI (`/src/types/` and `/src/api/endpoints/`)

---

## Base URL Structure

```
Production:  https://api.smartwms.com/api/v1
Development: http://localhost:5000/api/v1
```

## Authentication Endpoints

**Base**: `/api/v1/auth`

| Method | Endpoint | Request | Response | Description |
|--------|----------|---------|----------|-------------|
| POST | `/login` | `LoginRequest` | `LoginResponse` | User authentication |
| POST | `/logout` | - | `ApiResponse` | Invalidate token |
| POST | `/refresh` | `RefreshRequest` | `LoginResponse` | Refresh JWT token |
| GET | `/validate` | - | `UserInfo` | Validate current token |
| PUT | `/change-password` | `ChangePasswordRequest` | `ApiResponse` | Change password |

### Types
```typescript
LoginRequest {
  email: string;
  password: string;
}

LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

UserInfo {
  id: string;
  email: string;
  fullName: string;
  tenantId: string;
  warehouseId: string;
  roles: string[];
  permissions: string[];
}
```

---

## Tenant-Scoped Endpoints

**Base**: `/api/v1/tenant/{tenantId}`

### Products API

**Path**: `/products`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List products (paginated) |
| GET | `/{id}` | Get product by ID |
| GET | `/sku/{sku}` | Get product by SKU |
| GET | `/barcode/{barcode}` | Get product by barcode |
| GET | `/categories` | List product categories |
| GET | `/search?q={term}` | Quick search |
| POST | `/` | Create product |
| PUT | `/{id}` | Update product |
| DELETE | `/{id}` | Delete product |
| POST | `/import` | Import from file |
| GET | `/export` | Export to file |

#### Product Type (from SmartWMS_UI)
```typescript
interface Product {
  id: number;
  sku: string;
  name: string;
  description?: string;
  category?: string;
  barcode?: string;
  alternativeBarcodes?: string[];
  dimensions?: Dimensions;
  weight?: Weight;
  unitOfMeasure: string;
  unitsPerCase?: number;
  casesPerPallet?: number;
  isActive: boolean;
  isBatchTracked: boolean;
  isSerialTracked: boolean;
  hasExpiryDate: boolean;
  minStockLevel?: number;
  maxStockLevel?: number;
  reorderPoint?: number;
  supplierId?: number;
  imageUrl?: string;
}
```

---

### Locations API

**Path**: `/locations`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List locations (paginated) |
| GET | `/{id}` | Get location by ID |
| GET | `/code/{code}` | Get location by code |
| POST | `/` | Create location |
| PUT | `/{id}` | Update location |
| DELETE | `/{id}` | Delete location |
| GET | `/pick/{productId}` | Get pick locations for product |
| GET | `/suggest-putaway` | Suggest putaway location |

#### Location Type (from SmartWMS_UI)
```typescript
interface Location {
  id: number;
  code: string;
  name?: string;
  warehouseId: string;
  warehouseCode: string;
  zoneId?: string;
  zoneCode?: string;
  aisle?: string;
  rack?: string;
  level?: string;
  position?: string;
  locationType: LocationType;
  dimensions?: Dimensions;
  maxWeight?: number;
  maxVolume?: number;
  isActive: boolean;
  isPickLocation: boolean;
  isPutawayLocation: boolean;
  isReceivingDock: boolean;
  isShippingDock: boolean;
  pickSequence?: number;
  putawaySequence?: number;
}

enum LocationType {
  BULK, PICK, STAGING, RECEIVING, SHIPPING, RETURNS, QUARANTINE, RESERVE
}
```

---

### Zones API

**Path**: `/zones`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List zones |
| GET | `/{id}` | Get zone by ID |
| POST | `/` | Create zone |
| PUT | `/{id}` | Update zone |
| DELETE | `/{id}` | Delete zone |
| GET | `/{id}/locations` | Get locations in zone |

---

### Stock API

**Path**: `/stock`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List stock levels (paginated) |
| GET | `/product/{productId}` | Stock for specific product |
| GET | `/location/{locationId}` | Stock at specific location |
| GET | `/movements` | Stock movement history |
| POST | `/adjustments` | Create stock adjustment |
| POST | `/transfer` | Transfer between locations |
| POST | `/cycle-count` | Record cycle count |
| GET | `/alerts/low` | Low stock alerts |

#### StockLevel Type
```typescript
interface StockLevel {
  id: number;
  sku: string;
  productName: string;
  locationId: string;
  locationCode: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  batchNumber?: string;
  serialNumber?: string;
  expiryDate?: string;
  lastMovementAt?: string;
}
```

---

### Sales Orders API

**Path**: `/sales-orders`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List sales orders (paginated) |
| GET | `/{id}` | Get order by ID |
| GET | `/{id}/lines` | Get order lines |
| POST | `/` | Create order |
| PATCH | `/{id}/status` | Update status |
| POST | `/{id}/allocate` | Allocate stock |
| POST | `/{id}/cancel` | Cancel order |

#### SalesOrder Type
```typescript
interface SalesOrder {
  id: number;
  orderNumber: string;
  externalReference?: string;
  customerId: string;
  customerName: string;
  warehouseId: string;
  status: SalesOrderStatus;
  priority: OrderPriority;
  orderDate: string;
  requiredDate?: string;
  shippingAddress: PostalAddress;
  totalLines: number;
  totalQuantity: number;
  pickedQuantity: number;
  shippedQuantity: number;
}

enum SalesOrderStatus {
  DRAFT, PENDING, CONFIRMED, ALLOCATED, PARTIALLY_PICKED,
  PICKED, PACKED, SHIPPED, DELIVERED, CANCELLED, ON_HOLD
}
```

---

### Purchase Orders API

**Path**: `/purchase-orders`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List purchase orders |
| GET | `/{id}` | Get order by ID |
| GET | `/{id}/lines` | Get order lines |
| POST | `/` | Create order |
| PATCH | `/{id}/status` | Update status |
| POST | `/{id}/lines/{lineId}/receive` | Receive line items |
| POST | `/{id}/cancel` | Cancel order |

#### PurchaseOrder Type
```typescript
interface PurchaseOrder {
  id: number;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  warehouseId: string;
  status: PurchaseOrderStatus;
  orderDate: string;
  expectedDate?: string;
  totalLines: number;
  totalQuantity: number;
  receivedQuantity: number;
}

enum PurchaseOrderStatus {
  DRAFT, PENDING, CONFIRMED, PARTIALLY_RECEIVED, RECEIVED, CLOSED, CANCELLED
}
```

---

### Fulfillment API (KEY NEW MODULE!)

**Path**: `/fulfillment`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List fulfillment batches |
| GET | `/{id}` | Get batch details |
| POST | `/` | Create batch from orders |
| POST | `/{id}/release` | Release for picking |
| POST | `/{id}/cancel` | Cancel batch |
| GET | `/{id}/tasks` | Get pick tasks |
| POST | `/{id}/tasks/{taskId}/complete` | Complete pick task |
| POST | `/{id}/tasks/{taskId}/short-pick` | Report short pick |

#### FulfillmentBatch Type
```typescript
interface FulfillmentBatch {
  id: number;
  batchNumber: string;
  name?: string;
  warehouseId: string;
  warehouseCode: string;
  status: FulfillmentStatus;
  batchType: BatchType;
  orderCount: number;
  lineCount: number;
  totalQuantity: number;
  pickedQuantity: number;
  startedAt?: string;
  completedAt?: string;
  assignedTo?: UserRef;
  priority: number;
}

enum FulfillmentStatus {
  CREATED, RELEASED, IN_PROGRESS, PARTIALLY_COMPLETE, COMPLETE, CANCELLED
}

enum BatchType {
  SINGLE, MULTI, ZONE, WAVE
}
```

#### PickTask Type
```typescript
interface PickTask {
  id: number;
  taskNumber: string;
  batchId?: string;
  orderId: string;
  orderNumber: string;
  lineId: string;
  sku: string;
  productName: string;
  fromLocationId: string;
  fromLocationCode: string;
  toLocationId?: string;
  quantityRequired: number;
  quantityPicked: number;
  status: TaskStatus;
  priority: number;
  sequence: number;
  assignedTo?: UserRef;
}

enum TaskStatus {
  PENDING, ASSIGNED, IN_PROGRESS, COMPLETE, SHORT_PICKED, CANCELLED
}
```

---

### Users API

**Path**: `/users`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List users |
| GET | `/{id}` | Get user by ID |
| GET | `/me` | Get current user |
| POST | `/` | Create user |
| PUT | `/{id}` | Update user |
| POST | `/{id}/deactivate` | Deactivate user |
| POST | `/{id}/reset-password` | Reset password |

---

### Roles API

**Path**: `/roles`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List roles |
| GET | `/{id}` | Get role by ID |
| GET | `/permissions` | List all permissions |
| POST | `/` | Create role |
| PUT | `/{id}` | Update role |
| DELETE | `/{id}` | Delete role |

---

## Common Types

```typescript
// Pagination request
interface QueryParams {
  page?: number;        // Default: 1
  pageSize?: number;    // Default: 25, Max: 100
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  search?: string;
}

// Pagination response
interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Standard API response
interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

// Dimensions
interface Dimensions {
  widthMm: number;
  heightMm: number;
  depthMm: number;
}

// Weight
interface Weight {
  grossKg: number;
  netKg?: number;
}

// Address
interface PostalAddress {
  addressLine1: string;
  addressLine2?: string;
  city: string;
  postalCode: string;
  countryCode: string;
  region?: string;
}

// User reference
interface UserRef {
  userId: number;
  username: string;
  displayName?: string;
}
```

---

## Future Endpoints (Planned)

### Wave Optimization
```
POST /fulfillment/optimize          # Optimize pick sequence
GET  /fulfillment/{id}/pick-path    # Get optimized pick path
```

### Putaway Rules
```
GET  /putaway-rules                 # List rules
POST /putaway-rules                 # Create rule
GET  /locations/suggest-putaway     # Apply rules to suggest location
```

### Replenishment
```
GET  /replenishment/suggestions     # Get replenishment suggestions
POST /replenishment/execute         # Execute replenishment
```

### Carrier Integration
```
GET  /carriers                      # List configured carriers
POST /shipments/{id}/get-rates      # Get shipping rates
POST /shipments/{id}/create-label   # Create shipping label
GET  /shipments/{id}/track          # Track shipment
```

---

*Last Updated: December 13, 2025*
*Based on SmartWMS_UI version: 1.0*
