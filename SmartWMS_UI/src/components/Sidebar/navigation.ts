import {
  DASHBOARD,
  ORDERS,
  INBOUND,
  OUTBOUND,
  INVENTORY,
  WAREHOUSE,
  CONFIG,
  MONITORING,
  REPORTS,
  OPERATIONS,
} from '../../constants/routes';
import type { NavGroup } from './types';

/**
 * Main Navigation Structure
 * Organized by functional domain for WMS operations
 */
export const navigationGroups: NavGroup[] = [
  {
    id: 'main',
    label: '',
    items: [
      {
        id: 'dashboard',
        label: 'nav.dashboard',
        path: DASHBOARD,
      },
    ],
  },
  {
    id: 'orders',
    label: 'nav.orders',
    items: [
      {
        id: 'sales-orders',
        label: 'nav.orders.salesOrders',
        path: ORDERS.SALES_ORDERS,
      },
      {
        id: 'purchase-orders',
        label: 'nav.orders.purchaseOrders',
        path: ORDERS.PURCHASE_ORDERS,
      },
      {
        id: 'customers',
        label: 'nav.orders.customers',
        path: ORDERS.CUSTOMERS,
      },
      {
        id: 'suppliers',
        label: 'nav.orders.suppliers',
        path: ORDERS.SUPPLIERS,
      },
    ],
  },
  {
    id: 'inbound',
    label: 'nav.inbound',
    items: [
      {
        id: 'purchase-orders',
        label: 'nav.inbound.purchaseOrders',
        path: INBOUND.PURCHASE_ORDERS,
      },
      {
        id: 'receiving',
        label: 'nav.inbound.receiving',
        path: INBOUND.RECEIVING,
      },
      {
        id: 'putaway',
        label: 'nav.inbound.putaway',
        path: INBOUND.PUTAWAY,
      },
      {
        id: 'returns',
        label: 'nav.inbound.returns',
        path: INBOUND.RETURNS,
      },
    ],
  },
  {
    id: 'outbound',
    label: 'nav.outbound',
    items: [
      {
        id: 'sales-orders',
        label: 'nav.outbound.salesOrders',
        path: OUTBOUND.SALES_ORDERS,
      },
      {
        id: 'picking',
        label: 'nav.outbound.picking',
        path: OUTBOUND.PICKING,
      },
      {
        id: 'packing',
        label: 'nav.outbound.packing',
        path: OUTBOUND.PACKING,
      },
      {
        id: 'shipping',
        label: 'nav.outbound.shipping',
        path: OUTBOUND.SHIPPING,
      },
      {
        id: 'deliveries',
        label: 'nav.outbound.deliveries',
        path: OUTBOUND.DELIVERIES,
      },
    ],
  },
  {
    id: 'inventory',
    label: 'nav.inventory',
    items: [
      {
        id: 'catalog',
        label: 'nav.inventory.catalog',
        path: INVENTORY.SKU_CATALOG,
      },
      {
        id: 'stock-levels',
        label: 'nav.inventory.stockLevels',
        path: INVENTORY.STOCK_LEVELS,
      },
      {
        id: 'cycle-count',
        label: 'nav.inventory.cycleCount',
        path: INVENTORY.CYCLE_COUNT,
      },
      {
        id: 'adjustments',
        label: 'nav.inventory.adjustments',
        path: INVENTORY.ADJUSTMENTS,
      },
      {
        id: 'transfers',
        label: 'nav.inventory.transfers',
        path: INVENTORY.TRANSFERS,
      },
    ],
  },
  {
    id: 'warehouse',
    label: 'nav.warehouse',
    items: [
      {
        id: 'warehouses',
        label: 'nav.warehouse.warehouses',
        path: WAREHOUSE.WAREHOUSES,
      },
      {
        id: 'zones',
        label: 'nav.warehouse.zones',
        path: WAREHOUSE.ZONES,
      },
      {
        id: 'locations',
        label: 'nav.warehouse.locations',
        path: WAREHOUSE.LOCATIONS,
      },
      {
        id: 'equipment',
        label: 'nav.warehouse.equipment',
        path: WAREHOUSE.EQUIPMENT,
      },
    ],
  },
  {
    id: 'config',
    label: 'nav.config',
    items: [
      {
        id: 'sites',
        label: 'nav.config.sites',
        path: CONFIG.SITES,
      },
      {
        id: 'users',
        label: 'nav.config.users',
        path: CONFIG.USERS,
      },
      {
        id: 'roles',
        label: 'nav.config.roles',
        path: CONFIG.ROLES,
      },
      {
        id: 'integrations',
        label: 'nav.config.integrations',
        path: CONFIG.INTEGRATIONS,
      },
      {
        id: 'barcodes',
        label: 'nav.config.barcodes',
        path: CONFIG.BARCODES,
      },
      {
        id: 'carriers',
        label: 'nav.config.carriers',
        path: CONFIG.CARRIERS,
      },
      {
        id: 'reason-codes',
        label: 'nav.config.reasonCodes',
        path: CONFIG.REASON_CODES,
      },
      {
        id: 'notifications',
        label: 'nav.config.notifications',
        path: CONFIG.NOTIFICATIONS,
      },
      {
        id: 'automation',
        label: 'nav.config.automation',
        path: CONFIG.AUTOMATION,
      },
    ],
  },
  {
    id: 'operations',
    label: 'nav.operations',
    items: [
      {
        id: 'operation-hub',
        label: 'nav.operations.hub',
        path: OPERATIONS.HUB,
      },
      {
        id: 'tasks',
        label: 'nav.operations.tasks',
        path: OPERATIONS.TASKS,
      },
      {
        id: 'productivity',
        label: 'nav.operations.productivity',
        path: OPERATIONS.PRODUCTIVITY,
      },
    ],
  },
  {
    id: 'reports',
    label: 'nav.reports',
    items: [
      {
        id: 'analytics',
        label: 'nav.reports.analytics',
        path: REPORTS.ANALYTICS,
      },
    ],
  },
  {
    id: 'monitoring',
    label: 'nav.monitoring',
    items: [
      {
        id: 'sync-status',
        label: 'nav.monitoring.syncStatus',
        path: MONITORING.SYNC_STATUS,
      },
      {
        id: 'activity-log',
        label: 'nav.monitoring.activityLog',
        path: MONITORING.ACTIVITY_LOG,
      },
      {
        id: 'sessions',
        label: 'nav.monitoring.sessions',
        path: MONITORING.SESSIONS,
      },
    ],
  },
];
