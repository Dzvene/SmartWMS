import { useIntl } from 'react-intl';
import {
  useGetDashboardOverviewQuery,
  useGetQuickStatsQuery,
  useGetActivityFeedQuery,
  useGetAlertsQuery,
} from '@/api/modules/dashboard';
import './Dashboard.scss';

/**
 * Dashboard Module
 *
 * Main landing page displaying KPIs and operational overview.
 */
export function Dashboard() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const { data: overviewResponse, isLoading: overviewLoading } = useGetDashboardOverviewQuery();
  const { data: quickStatsResponse, isLoading: statsLoading } = useGetQuickStatsQuery();
  const { data: activityResponse } = useGetActivityFeedQuery({ limit: 10 });
  const { data: alertsResponse } = useGetAlertsQuery();

  const overview = overviewResponse?.data;
  const quickStats = quickStatsResponse?.data;
  const activities = activityResponse?.data?.items || [];
  const alerts = alertsResponse?.data;

  const isLoading = overviewLoading || statsLoading;

  const getTrendClass = (value: number, positive: boolean = true) => {
    if (value > 0) return positive ? 'dashboard__card-trend--positive' : 'dashboard__card-trend--negative';
    if (value < 0) return positive ? 'dashboard__card-trend--negative' : 'dashboard__card-trend--positive';
    return 'dashboard__card-trend--neutral';
  };

  return (
    <div className="dashboard">
      <header className="dashboard__header">
        <div className="dashboard__title-section">
          <h1 className="dashboard__title">{t('dashboard.title', 'Dashboard')}</h1>
          <p className="dashboard__subtitle">{t('dashboard.welcome', 'Welcome to SmartWMS')}</p>
        </div>
      </header>

      {/* Quick Stats */}
      <div className="dashboard__grid">
        <div className="dashboard__card dashboard__card--metric">
          <div className="dashboard__card-header">
            <span className="dashboard__card-title">{t('dashboard.ordersToday', 'Orders Today')}</span>
          </div>
          <div className="dashboard__card-value">
            {isLoading ? '...' : overview?.orders.ordersToday ?? 0}
          </div>
          <div className={`dashboard__card-trend ${getTrendClass(overview?.orders.ordersThisWeek || 0)}`}>
            {overview?.orders.ordersThisWeek ?? 0} this week
          </div>
        </div>

        <div className="dashboard__card dashboard__card--metric">
          <div className="dashboard__card-header">
            <span className="dashboard__card-title">{t('dashboard.pendingShipments', 'Pending Shipments')}</span>
          </div>
          <div className="dashboard__card-value">
            {isLoading ? '...' : quickStats?.ordersToShip ?? 0}
          </div>
          <div className="dashboard__card-trend dashboard__card-trend--neutral">
            {overview?.fulfillment.shipmentsToday ?? 0} shipped today
          </div>
        </div>

        <div className="dashboard__card dashboard__card--metric">
          <div className="dashboard__card-header">
            <span className="dashboard__card-title">{t('dashboard.lowStock', 'Low Stock Items')}</span>
          </div>
          <div className={`dashboard__card-value ${(quickStats?.lowStockItems ?? 0) > 0 ? 'dashboard__card-value--warning' : ''}`}>
            {isLoading ? '...' : quickStats?.lowStockItems ?? 0}
          </div>
          <div className="dashboard__card-trend dashboard__card-trend--neutral">
            {overview?.inventory.outOfStockProducts ?? 0} out of stock
          </div>
        </div>

        <div className="dashboard__card dashboard__card--metric">
          <div className="dashboard__card-header">
            <span className="dashboard__card-title">{t('dashboard.pickingPerformance', 'Pick Accuracy')}</span>
          </div>
          <div className="dashboard__card-value">
            {isLoading ? '...' : `${overview?.fulfillment.pickAccuracyRate?.toFixed(1) ?? '--'}%`}
          </div>
          <div className="dashboard__card-trend dashboard__card-trend--neutral">
            {overview?.fulfillment.completedToday ?? 0} tasks completed
          </div>
        </div>
      </div>

      {/* Detailed Stats */}
      <div className="dashboard__sections">
        {/* Orders Section */}
        <div className="dashboard__section">
          <div className="dashboard__card dashboard__card--list">
            <div className="dashboard__card-header">
              <span className="dashboard__card-title">{t('dashboard.ordersOverview', 'Orders Overview')}</span>
            </div>
            <div className="dashboard__card-content">
              {overview ? (
                <div className="dashboard__stats-list">
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.salesOrders', 'Sales Orders')}</span>
                    <span className="dashboard__stat-value">{overview.orders.totalSalesOrders}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.pendingSalesOrders', 'Pending')}</span>
                    <span className="dashboard__stat-value dashboard__stat-value--highlight">{overview.orders.pendingSalesOrders}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.purchaseOrders', 'Purchase Orders')}</span>
                    <span className="dashboard__stat-value">{overview.orders.totalPurchaseOrders}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.pendingPO', 'Pending POs')}</span>
                    <span className="dashboard__stat-value dashboard__stat-value--highlight">{overview.orders.pendingPurchaseOrders}</span>
                  </div>
                </div>
              ) : (
                <p className="dashboard__empty">{t('common.loading', 'Loading...')}</p>
              )}
            </div>
          </div>
        </div>

        {/* Fulfillment Section */}
        <div className="dashboard__section">
          <div className="dashboard__card dashboard__card--list">
            <div className="dashboard__card-header">
              <span className="dashboard__card-title">{t('dashboard.fulfillmentStatus', 'Fulfillment Status')}</span>
            </div>
            <div className="dashboard__card-content">
              {overview ? (
                <div className="dashboard__stats-list">
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.pendingPicks', 'Pending Pick Tasks')}</span>
                    <span className="dashboard__stat-value dashboard__stat-value--warning">{overview.fulfillment.pendingPickTasks}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.inProgressPicks', 'In Progress')}</span>
                    <span className="dashboard__stat-value">{overview.fulfillment.inProgressPickTasks}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.pendingPacks', 'Pending Pack Tasks')}</span>
                    <span className="dashboard__stat-value">{overview.fulfillment.pendingPackTasks}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.pendingShipments', 'Pending Shipments')}</span>
                    <span className="dashboard__stat-value">{overview.fulfillment.shipmentsPending}</span>
                  </div>
                </div>
              ) : (
                <p className="dashboard__empty">{t('common.loading', 'Loading...')}</p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Warehouse & Activity Row */}
      <div className="dashboard__sections">
        {/* Warehouse Stats */}
        <div className="dashboard__section">
          <div className="dashboard__card dashboard__card--list">
            <div className="dashboard__card-header">
              <span className="dashboard__card-title">{t('dashboard.warehouseStatus', 'Warehouse Status')}</span>
            </div>
            <div className="dashboard__card-content">
              {overview ? (
                <div className="dashboard__stats-list">
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.totalLocations', 'Total Locations')}</span>
                    <span className="dashboard__stat-value">{overview.warehouse.totalLocations}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.utilization', 'Utilization')}</span>
                    <span className="dashboard__stat-value">{overview.warehouse.locationUtilization?.toFixed(1)}%</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.totalProducts', 'Total Products')}</span>
                    <span className="dashboard__stat-value">{overview.inventory.totalProducts}</span>
                  </div>
                  <div className="dashboard__stat-item">
                    <span className="dashboard__stat-label">{t('dashboard.activeProducts', 'Active Products')}</span>
                    <span className="dashboard__stat-value">{overview.inventory.activeProducts}</span>
                  </div>
                </div>
              ) : (
                <p className="dashboard__empty">{t('common.loading', 'Loading...')}</p>
              )}
            </div>
          </div>
        </div>

        {/* Recent Activity */}
        <div className="dashboard__section">
          <div className="dashboard__card dashboard__card--list">
            <div className="dashboard__card-header">
              <span className="dashboard__card-title">{t('dashboard.recentActivity', 'Recent Activity')}</span>
            </div>
            <div className="dashboard__card-content">
              {activities.length > 0 ? (
                <ul className="dashboard__activity-list">
                  {activities.slice(0, 5).map((activity) => (
                    <li key={activity.id} className="dashboard__activity-item">
                      <span className="dashboard__activity-icon">{activity.icon || '📝'}</span>
                      <div className="dashboard__activity-content">
                        <span className="dashboard__activity-title">{activity.title}</span>
                        <span className="dashboard__activity-time">
                          {new Date(activity.createdAt).toLocaleTimeString()}
                        </span>
                      </div>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="dashboard__empty">{t('dashboard.noRecentActivity', 'No recent activity')}</p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Alerts Section */}
      {alerts && alerts.totalCount > 0 && (
        <div className="dashboard__alerts">
          <h3 className="dashboard__alerts-title">{t('dashboard.alerts', 'Alerts')}</h3>
          <div className="dashboard__alerts-list">
            {alerts.critical.map((alert, index) => (
              <div key={`critical-${index}`} className="dashboard__alert dashboard__alert--critical">
                <span className="dashboard__alert-icon">🔴</span>
                <div className="dashboard__alert-content">
                  <span className="dashboard__alert-title">{alert.title}</span>
                  {alert.description && <span className="dashboard__alert-desc">{alert.description}</span>}
                </div>
              </div>
            ))}
            {alerts.warning.map((alert, index) => (
              <div key={`warning-${index}`} className="dashboard__alert dashboard__alert--warning">
                <span className="dashboard__alert-icon">🟠</span>
                <div className="dashboard__alert-content">
                  <span className="dashboard__alert-title">{alert.title}</span>
                  {alert.description && <span className="dashboard__alert-desc">{alert.description}</span>}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

export default Dashboard;
