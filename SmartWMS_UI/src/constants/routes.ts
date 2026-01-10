/**
 * SmartWMS Application Routes
 * Organized by functional domain
 */

// ============================================
// Authentication
// ============================================
export const AUTH = {
  LOGIN: '/login',
  LOGOUT: '/logout',
  UNAUTHORIZED: '/unauthorized',
} as const;

// ============================================
// Main Navigation
// ============================================
export const ROOT = '/';
export const DASHBOARD = '/dashboard';

// ============================================
// Orders Management
// ============================================
export const ORDERS = {
  ROOT: '/orders',
  SALES_ORDERS: '/orders/sales',
  SALES_ORDER_CREATE: '/orders/sales/new',
  SALES_ORDER_DETAILS: '/orders/sales/:id',
  PURCHASE_ORDERS: '/orders/purchase',
  PURCHASE_ORDER_CREATE: '/orders/purchase/new',
  PURCHASE_ORDER_DETAILS: '/orders/purchase/:id',
  CUSTOMERS: '/orders/customers',
  CUSTOMER_CREATE: '/orders/customers/new',
  CUSTOMER_DETAILS: '/orders/customers/:id',
  SUPPLIERS: '/orders/suppliers',
} as const;

// ============================================
// Inbound Operations
// ============================================
export const INBOUND = {
  ROOT: '/inbound',
  PURCHASE_ORDERS: '/inbound/purchase-orders',
  RECEIVING: '/inbound/receiving',
  RECEIVING_EXECUTION: '/inbound/receiving/:id',
  PUTAWAY: '/inbound/putaway',
  PUTAWAY_EXECUTION: '/inbound/putaway/:id',
  RETURNS: '/inbound/returns',
} as const;

// ============================================
// Outbound Operations
// ============================================
export const OUTBOUND = {
  ROOT: '/outbound',
  SALES_ORDERS: '/outbound/sales-orders',
  PICKING: '/outbound/picking',
  PICK_EXECUTION: '/outbound/picking/:id',
  PACKING: '/outbound/packing',
  PACK_EXECUTION: '/outbound/packing/:id',
  SHIPPING: '/outbound/shipping',
  DELIVERIES: '/outbound/deliveries',
} as const;

// ============================================
// Inventory Management
// ============================================
export const INVENTORY = {
  ROOT: '/inventory',
  SKU_CATALOG: '/inventory/catalog',
  STOCK_LEVELS: '/inventory/stock-levels',
  CYCLE_COUNT: '/inventory/cycle-count',
  ADJUSTMENTS: '/inventory/adjustments',
  TRANSFERS: '/inventory/transfers',
} as const;

// ============================================
// Warehouse Structure
// ============================================
export const WAREHOUSE = {
  ROOT: '/warehouse',
  WAREHOUSES: '/warehouse/warehouses',
  ZONES: '/warehouse/zones',
  LOCATIONS: '/warehouse/locations',
  EQUIPMENT: '/warehouse/equipment',
} as const;

// ============================================
// System Configuration
// ============================================
export const CONFIG = {
  ROOT: '/config',
  SITES: '/config/sites',
  USERS: '/config/users',
  ROLES: '/config/roles',
  INTEGRATIONS: '/config/integrations',
  BARCODES: '/config/barcodes',
  CARRIERS: '/config/carriers',
  REASON_CODES: '/config/reason-codes',
  NOTIFICATIONS: '/config/notifications',
  AUTOMATION: '/config/automation',
} as const;

// ============================================
// Monitoring & Reports
// ============================================
export const MONITORING = {
  ROOT: '/monitoring',
  SYNC_STATUS: '/monitoring/sync-status',
  ACTIVITY_LOG: '/monitoring/activity-log',
  SESSIONS: '/monitoring/sessions',
} as const;

// ============================================
// Reports & Analytics
// ============================================
export const REPORTS = {
  ROOT: '/reports',
  ANALYTICS: '/reports/analytics',
} as const;

// ============================================
// Operation Hub
// ============================================
export const OPERATIONS = {
  ROOT: '/operations',
  HUB: '/operations/hub',
  TASKS: '/operations/tasks',
  PRODUCTIVITY: '/operations/productivity',
} as const;

// ============================================
// Route Utilities
// ============================================

/**
 * Build route with parameters
 */
export function buildRoute(
  template: string,
  params: Record<string, string | number>
): string {
  let route = template;
  for (const [key, value] of Object.entries(params)) {
    route = route.replace(`:${key}`, String(value));
  }
  return route;
}

/**
 * Check if current path matches route
 */
export function matchRoute(path: string, route: string): boolean {
  const pathParts = path.split('/').filter(Boolean);
  const routeParts = route.split('/').filter(Boolean);

  if (pathParts.length !== routeParts.length) return false;

  return routeParts.every((part, i) => {
    if (part.startsWith(':')) return true;
    return part === pathParts[i];
  });
}
