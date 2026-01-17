import { useState, useCallback } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAppSelector } from '../../../store/hooks';
import { useTokenRefresh } from '../../../hooks/useTokenRefresh';
import { Sidebar } from '../../../components/Sidebar';
import { Dashboard } from '../Dashboard';
import { Login } from '../Auth';
import { ProductCatalog, ProductDetails, ProductCreate } from '../../@inventory/ProductCatalog';
import { StockLevels } from '../../@inventory/StockLevels';
import { CycleCount } from '../../@inventory/CycleCount';
import { Adjustments } from '../../@inventory/Adjustments';
import { Transfers } from '../../@inventory/Transfers';
import { Picking, PickTaskExecution } from '../../@outbound/Picking';
import { Shipping } from '../../@outbound/Shipping';
import { Packing, PackingCreate, PackingTaskExecution } from '../../@outbound/Packing';
import { Deliveries } from '../../@outbound/Deliveries';
import { PurchaseOrders } from '../../@inbound/PurchaseOrders';
import { Receiving, ReceivingCreate, ReceivingExecution } from '../../@inbound/Receiving';
import { Putaway, PutawayCreate, PutawayTaskExecution } from '../../@inbound/Putaway';
import { Returns, ReturnsCreate } from '../../@inbound/Returns';
import { LocationsList, LocationDetails, LocationCreate } from '../../@warehouse/Locations';
import { ZonesList, ZoneDetails, ZoneCreate } from '../../@warehouse/Zones';
import { Equipment, EquipmentCreate, EquipmentDetails } from '../../@warehouse/Equipment';
import { WarehousesList, WarehouseDetails, WarehouseCreate } from '../../@warehouse/Warehouses';
import { SitesList, SiteDetails, SiteCreate } from '../../@config/Sites';
import { UsersList, UserDetails, UserCreate } from '../../@config/Users';
import { RolesList, RoleDetails, RoleCreate } from '../../@config/Roles';
import { Barcodes, BarcodeCreate, BarcodeDetails } from '../../@config/Barcodes';
import { Carriers, CarrierCreate, CarrierDetails } from '../../@config/Carriers';
import { ReasonCodes, ReasonCodeCreate, ReasonCodeDetails } from '../../@config/ReasonCodes';
import { Notifications, NotificationCreate, NotificationDetails } from '../../@config/Notifications';
import { Integrations, IntegrationCreate, IntegrationDetails } from '../../@config/Integrations';
import { Automation, AutomationCreate, AutomationDetails } from '../../@config/Automation';
import { SyncStatus } from '../../@monitoring/SyncStatus';
import { ActivityLog } from '../../@monitoring/ActivityLog';
import { Sessions } from '../../@monitoring/Sessions';
import { Reports } from '../../@reports/Reports';
import { OperationHub, Tasks, Productivity } from '../../@operations';
import {
  SalesOrders as OrdersSalesOrders,
  SalesOrderCreate as OrdersSalesOrderCreate,
  SalesOrderDetails as OrdersSalesOrderDetails,
  PurchaseOrders as OrdersPurchaseOrders,
  PurchaseOrderCreate as OrdersPurchaseOrderCreate,
  PurchaseOrderDetails as OrdersPurchaseOrderDetails,
  Customers,
  CustomerCreate,
  CustomerDetails,
  Suppliers,
  SupplierCreate,
  SupplierDetails,
} from '../../@orders';
import {
  AUTH,
  DASHBOARD,
  ROOT,
  ORDERS,
  INVENTORY,
  OUTBOUND,
  INBOUND,
  WAREHOUSE,
  CONFIG,
  MONITORING,
  REPORTS,
  OPERATIONS,
} from '../../../constants/routes';

// Build timestamp injected by Vite at build time
declare const __BUILD_TIME__: string;

function DevBuildInfo() {
  if (import.meta.env.PROD) return null;

  return (
    <div
      style={{
        position: 'fixed',
        bottom: 8,
        right: 8,
        background: 'rgba(0, 0, 0, 0.8)',
        color: '#0f0',
        padding: '4px 8px',
        borderRadius: 4,
        fontSize: 11,
        fontFamily: 'monospace',
        zIndex: 99999,
        pointerEvents: 'none',
      }}
    >
      DEV | {__BUILD_TIME__}
    </div>
  );
}

/**
 * Main Application Component
 *
 * Sets up routing and the main layout structure.
 */
function App() {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);

  // Auto-refresh token when user is active
  useTokenRefresh();

  const toggleSidebar = useCallback(() => {
    setSidebarCollapsed((prev) => !prev);
  }, []);

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return (
      <Routes>
        <Route path={AUTH.LOGIN} element={<Login />} />
        <Route path="*" element={<Navigate to={AUTH.LOGIN} replace />} />
      </Routes>
    );
  }

  return (
    <>
      <DevBuildInfo />
      <Routes>
        {/* Redirect to dashboard if already authenticated and on login page */}
        <Route path={AUTH.LOGIN} element={<Navigate to={DASHBOARD} replace />} />

      {/* Protected routes with layout */}
      <Route
        path="*"
        element={
          <div className="layout">
            <Sidebar collapsed={sidebarCollapsed} onToggle={toggleSidebar} />
            <main className="layout__main">
              <div className="layout__content">
                <Routes>
                  <Route path={ROOT} element={<Navigate to={DASHBOARD} replace />} />
                  <Route path={DASHBOARD} element={<Dashboard />} />

                  {/* Orders */}
                  <Route path={ORDERS.SALES_ORDERS} element={<OrdersSalesOrders />} />
                  <Route path={ORDERS.SALES_ORDER_CREATE} element={<OrdersSalesOrderCreate />} />
                  <Route path={ORDERS.SALES_ORDER_DETAILS} element={<OrdersSalesOrderDetails />} />
                  <Route path={ORDERS.PURCHASE_ORDERS} element={<OrdersPurchaseOrders />} />
                  <Route path={ORDERS.PURCHASE_ORDER_CREATE} element={<OrdersPurchaseOrderCreate />} />
                  <Route path={ORDERS.PURCHASE_ORDER_DETAILS} element={<OrdersPurchaseOrderDetails />} />
                  <Route path={ORDERS.CUSTOMERS} element={<Customers />} />
                  <Route path={ORDERS.CUSTOMER_CREATE} element={<CustomerCreate />} />
                  <Route path={ORDERS.CUSTOMER_DETAILS} element={<CustomerDetails />} />
                  <Route path={ORDERS.SUPPLIERS} element={<Suppliers />} />
                  <Route path={ORDERS.SUPPLIER_CREATE} element={<SupplierCreate />} />
                  <Route path={ORDERS.SUPPLIER_DETAILS} element={<SupplierDetails />} />

                  {/* Inventory */}
                  <Route path={INVENTORY.SKU_CATALOG} element={<ProductCatalog />} />
                  <Route path={`${INVENTORY.SKU_CATALOG}/new`} element={<ProductCreate />} />
                  <Route path={`${INVENTORY.SKU_CATALOG}/:id`} element={<ProductDetails />} />
                  <Route path={INVENTORY.STOCK_LEVELS} element={<StockLevels />} />
                  <Route path={INVENTORY.CYCLE_COUNT} element={<CycleCount />} />
                  <Route path={INVENTORY.ADJUSTMENTS} element={<Adjustments />} />
                  <Route path={INVENTORY.TRANSFERS} element={<Transfers />} />

                  {/* Outbound */}
                  <Route path={OUTBOUND.SALES_ORDERS} element={<OrdersSalesOrders />} />
                  <Route path={OUTBOUND.PICKING} element={<Picking />} />
                  <Route path={OUTBOUND.PICK_EXECUTION} element={<PickTaskExecution />} />
                  <Route path={OUTBOUND.PACKING} element={<Packing />} />
                  <Route path={OUTBOUND.PACKING_CREATE} element={<PackingCreate />} />
                  <Route path={OUTBOUND.PACK_EXECUTION} element={<PackingTaskExecution />} />
                  <Route path={OUTBOUND.SHIPPING} element={<Shipping />} />
                  <Route path={OUTBOUND.DELIVERIES} element={<Deliveries />} />

                  {/* Inbound */}
                  <Route path={INBOUND.PURCHASE_ORDERS} element={<PurchaseOrders />} />
                  <Route path={INBOUND.RECEIVING} element={<Receiving />} />
                  <Route path={INBOUND.RECEIVING_CREATE} element={<ReceivingCreate />} />
                  <Route path={INBOUND.RECEIVING_EXECUTION} element={<ReceivingExecution />} />
                  <Route path={INBOUND.PUTAWAY} element={<Putaway />} />
                  <Route path={INBOUND.PUTAWAY_CREATE} element={<PutawayCreate />} />
                  <Route path={INBOUND.PUTAWAY_EXECUTION} element={<PutawayTaskExecution />} />
                  <Route path={INBOUND.RETURNS} element={<Returns />} />
                  <Route path={INBOUND.RETURNS_CREATE} element={<ReturnsCreate />} />

                  {/* Warehouse */}
                  <Route path={WAREHOUSE.WAREHOUSES} element={<WarehousesList />} />
                  <Route path={`${WAREHOUSE.WAREHOUSES}/new`} element={<WarehouseCreate />} />
                  <Route path={`${WAREHOUSE.WAREHOUSES}/:id`} element={<WarehouseDetails />} />
                  <Route path={WAREHOUSE.ZONES} element={<ZonesList />} />
                  <Route path={`${WAREHOUSE.ZONES}/new`} element={<ZoneCreate />} />
                  <Route path={`${WAREHOUSE.ZONES}/:id`} element={<ZoneDetails />} />
                  <Route path={WAREHOUSE.LOCATIONS} element={<LocationsList />} />
                  <Route path={`${WAREHOUSE.LOCATIONS}/new`} element={<LocationCreate />} />
                  <Route path={`${WAREHOUSE.LOCATIONS}/:id`} element={<LocationDetails />} />
                  <Route path={WAREHOUSE.EQUIPMENT} element={<Equipment />} />
                  <Route path={WAREHOUSE.EQUIPMENT_CREATE} element={<EquipmentCreate />} />
                  <Route path={WAREHOUSE.EQUIPMENT_DETAILS} element={<EquipmentDetails />} />

                  {/* Configuration */}
                  <Route path={CONFIG.SITES} element={<SitesList />} />
                  <Route path={`${CONFIG.SITES}/new`} element={<SiteCreate />} />
                  <Route path={`${CONFIG.SITES}/:id`} element={<SiteDetails />} />
                  <Route path={CONFIG.USERS} element={<UsersList />} />
                  <Route path={`${CONFIG.USERS}/new`} element={<UserCreate />} />
                  <Route path={`${CONFIG.USERS}/:id`} element={<UserDetails />} />
                  <Route path={CONFIG.ROLES} element={<RolesList />} />
                  <Route path={`${CONFIG.ROLES}/new`} element={<RoleCreate />} />
                  <Route path={`${CONFIG.ROLES}/:id`} element={<RoleDetails />} />
                  <Route path={CONFIG.INTEGRATIONS} element={<Integrations />} />
                  <Route path={CONFIG.BARCODES} element={<Barcodes />} />
                  <Route path={CONFIG.BARCODE_CREATE} element={<BarcodeCreate />} />
                  <Route path={CONFIG.BARCODE_DETAILS} element={<BarcodeDetails />} />
                  <Route path={CONFIG.CARRIERS} element={<Carriers />} />
                  <Route path={CONFIG.CARRIER_CREATE} element={<CarrierCreate />} />
                  <Route path={CONFIG.CARRIER_DETAILS} element={<CarrierDetails />} />
                  <Route path={CONFIG.REASON_CODES} element={<ReasonCodes />} />
                  <Route path={CONFIG.REASON_CODE_CREATE} element={<ReasonCodeCreate />} />
                  <Route path={CONFIG.REASON_CODE_DETAILS} element={<ReasonCodeDetails />} />
                  <Route path={CONFIG.NOTIFICATIONS} element={<Notifications />} />
                  <Route path={CONFIG.NOTIFICATION_CREATE} element={<NotificationCreate />} />
                  <Route path={CONFIG.NOTIFICATION_DETAILS} element={<NotificationDetails />} />
                  <Route path={CONFIG.INTEGRATIONS} element={<Integrations />} />
                  <Route path={CONFIG.INTEGRATION_CREATE} element={<IntegrationCreate />} />
                  <Route path={CONFIG.INTEGRATION_DETAILS} element={<IntegrationDetails />} />
                  <Route path={CONFIG.AUTOMATION} element={<Automation />} />
                  <Route path={CONFIG.AUTOMATION_CREATE} element={<AutomationCreate />} />
                  <Route path={CONFIG.AUTOMATION_DETAILS} element={<AutomationDetails />} />

                  {/* Reports */}
                  <Route path={REPORTS.ANALYTICS} element={<Reports />} />

                  {/* Operations Hub */}
                  <Route path={OPERATIONS.HUB} element={<OperationHub />} />
                  <Route path={OPERATIONS.TASKS} element={<Tasks />} />
                  <Route path={OPERATIONS.PRODUCTIVITY} element={<Productivity />} />

                  {/* Monitoring */}
                  <Route path={MONITORING.SYNC_STATUS} element={<SyncStatus />} />
                  <Route path={MONITORING.ACTIVITY_LOG} element={<ActivityLog />} />
                  <Route path={MONITORING.SESSIONS} element={<Sessions />} />

                  <Route path="*" element={<NotFound />} />
                </Routes>
              </div>
            </main>
          </div>
        }
      />
      </Routes>
    </>
  );
}

function NotFound() {
  return (
    <div className="page">
      <h1>404 - Page Not Found</h1>
      <p>The requested page does not exist.</p>
    </div>
  );
}

export default App;
