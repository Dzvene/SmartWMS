import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useGetInventorySummaryQuery } from '@/api/modules/reports';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import './InventorySummary.scss';

/**
 * Inventory Summary Report
 *
 * Displays inventory metrics, stock levels, and location utilization.
 */
export function InventorySummary() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [warehouseId, setWarehouseId] = useState<string>('');

  const { data: warehousesResponse } = useGetWarehousesQuery();
  const warehouses = warehousesResponse?.data?.items || [];

  const { data: reportResponse, isLoading, error } = useGetInventorySummaryQuery(
    warehouseId ? { warehouseId } : undefined
  );

  const report = reportResponse?.data;

  if (isLoading) {
    return (
      <div className="inventory-summary inventory-summary--loading">
        <div className="loading-spinner" />
        <p>{t('common.loading', 'Loading...')}</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="inventory-summary inventory-summary--error">
        <p>{t('common.error', 'Error loading report')}</p>
      </div>
    );
  }

  return (
    <div className="inventory-summary">
      <div className="inventory-summary__filters">
        <div className="inventory-summary__filter">
          <label>{t('reports.warehouse', 'Warehouse')}</label>
          <select
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            className="inventory-summary__select"
          >
            <option value="">{t('reports.allWarehouses', 'All Warehouses')}</option>
            {warehouses.map((wh) => (
              <option key={wh.id} value={wh.id}>
                {wh.name}
              </option>
            ))}
          </select>
        </div>
        {report?.generatedAt && (
          <div className="inventory-summary__generated">
            {t('reports.generatedAt', 'Generated')}: {new Date(report.generatedAt).toLocaleString()}
          </div>
        )}
      </div>

      {report && (
        <>
          {/* Product Metrics */}
          <div className="inventory-summary__section">
            <h3 className="inventory-summary__section-title">
              {t('reports.productMetrics', 'Product Metrics')}
            </h3>
            <div className="inventory-summary__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalProducts.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalProducts', 'Total Products')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.productsWithStock.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.productsWithStock', 'With Stock')}</div>
              </div>
              <div className="metric-card metric-card--danger">
                <div className="metric-card__value">{report.productsOutOfStock.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.productsOutOfStock', 'Out of Stock')}</div>
              </div>
              <div className="metric-card metric-card--warning">
                <div className="metric-card__value">{report.productsLowStock.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.productsLowStock', 'Low Stock')}</div>
              </div>
            </div>
          </div>

          {/* Stock Quantities */}
          <div className="inventory-summary__section">
            <h3 className="inventory-summary__section-title">
              {t('reports.stockQuantities', 'Stock Quantities')}
            </h3>
            <div className="inventory-summary__metrics">
              <div className="metric-card metric-card--large">
                <div className="metric-card__value">{report.totalQuantityOnHand.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.quantityOnHand', 'On Hand')}</div>
              </div>
              <div className="metric-card metric-card--large">
                <div className="metric-card__value">{report.totalQuantityReserved.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.quantityReserved', 'Reserved')}</div>
              </div>
              <div className="metric-card metric-card--large metric-card--success">
                <div className="metric-card__value">{report.totalQuantityAvailable.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.quantityAvailable', 'Available')}</div>
              </div>
            </div>
          </div>

          {/* Location Utilization */}
          <div className="inventory-summary__section">
            <h3 className="inventory-summary__section-title">
              {t('reports.locationUtilization', 'Location Utilization')}
            </h3>
            <div className="inventory-summary__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalLocations.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalLocations', 'Total Locations')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.occupiedLocations.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.occupiedLocations', 'Occupied')}</div>
              </div>
              <div className="metric-card">
                <div className="metric-card__value">{report.emptyLocations.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.emptyLocations', 'Empty')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{report.locationUtilizationPercent}%</div>
                <div className="metric-card__label">{t('reports.utilization', 'Utilization')}</div>
              </div>
            </div>
          </div>

          {/* Tables Row */}
          <div className="inventory-summary__tables">
            {/* Top Products */}
            <div className="inventory-summary__table-section">
              <h3 className="inventory-summary__section-title">
                {t('reports.topProductsByQuantity', 'Top Products by Quantity')}
              </h3>
              <table className="inventory-summary__table">
                <thead>
                  <tr>
                    <th>{t('reports.sku', 'SKU')}</th>
                    <th>{t('reports.product', 'Product')}</th>
                    <th className="text-right">{t('reports.quantity', 'Quantity')}</th>
                  </tr>
                </thead>
                <tbody>
                  {report.topProductsByQuantity.map((item) => (
                    <tr key={item.productId}>
                      <td><code>{item.sku}</code></td>
                      <td>{item.productName}</td>
                      <td className="text-right">{item.quantityOnHand.toLocaleString()}</td>
                    </tr>
                  ))}
                  {report.topProductsByQuantity.length === 0 && (
                    <tr>
                      <td colSpan={3} className="text-center text-muted">
                        {t('common.noData', 'No data')}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            {/* Low Stock Products */}
            <div className="inventory-summary__table-section">
              <h3 className="inventory-summary__section-title">
                {t('reports.lowStockProducts', 'Low Stock Products')}
              </h3>
              <table className="inventory-summary__table">
                <thead>
                  <tr>
                    <th>{t('reports.sku', 'SKU')}</th>
                    <th>{t('reports.product', 'Product')}</th>
                    <th className="text-right">{t('reports.onHand', 'On Hand')}</th>
                    <th className="text-right">{t('reports.minLevel', 'Min Level')}</th>
                  </tr>
                </thead>
                <tbody>
                  {report.lowStockProducts.map((item) => (
                    <tr key={item.productId}>
                      <td><code>{item.sku}</code></td>
                      <td>{item.productName}</td>
                      <td className="text-right text-danger">{item.quantityOnHand.toLocaleString()}</td>
                      <td className="text-right">{item.minStockLevel?.toLocaleString() || '-'}</td>
                    </tr>
                  ))}
                  {report.lowStockProducts.length === 0 && (
                    <tr>
                      <td colSpan={4} className="text-center text-muted">
                        {t('reports.noLowStock', 'No low stock products')}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Expiring Stock */}
          {report.expiringStock.length > 0 && (
            <div className="inventory-summary__section">
              <h3 className="inventory-summary__section-title">
                {t('reports.expiringStock', 'Expiring Stock (30 days)')}
              </h3>
              <table className="inventory-summary__table inventory-summary__table--full">
                <thead>
                  <tr>
                    <th>{t('reports.sku', 'SKU')}</th>
                    <th>{t('reports.product', 'Product')}</th>
                    <th>{t('reports.batch', 'Batch')}</th>
                    <th>{t('reports.expiryDate', 'Expiry Date')}</th>
                    <th className="text-right">{t('reports.quantity', 'Quantity')}</th>
                    <th className="text-right">{t('reports.daysLeft', 'Days Left')}</th>
                  </tr>
                </thead>
                <tbody>
                  {report.expiringStock.map((item, index) => (
                    <tr key={`${item.productId}-${index}`}>
                      <td><code>{item.sku}</code></td>
                      <td>{item.productName}</td>
                      <td>{item.batchNumber || '-'}</td>
                      <td>{item.expiryDate ? new Date(item.expiryDate).toLocaleDateString() : '-'}</td>
                      <td className="text-right">{item.quantity.toLocaleString()}</td>
                      <td className={`text-right ${item.daysUntilExpiry <= 7 ? 'text-danger' : item.daysUntilExpiry <= 14 ? 'text-warning' : ''}`}>
                        {item.daysUntilExpiry}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default InventorySummary;
