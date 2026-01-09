import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useGetStockMovementReportQuery } from '@/api/modules/reports';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import './StockMovements.scss';

/**
 * Stock Movements Report
 *
 * Displays stock movement statistics by type and time period.
 */
export function StockMovements() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [warehouseId, setWarehouseId] = useState<string>('');
  const [dateFrom, setDateFrom] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().split('T')[0];
  });
  const [dateTo, setDateTo] = useState<string>(() => new Date().toISOString().split('T')[0]);

  const { data: warehousesResponse } = useGetWarehousesQuery();
  const warehouses = warehousesResponse?.data?.items || [];

  const { data: reportResponse, isLoading, error } = useGetStockMovementReportQuery({
    dateFrom,
    dateTo,
    warehouseId: warehouseId || undefined,
  });

  const report = reportResponse?.data;

  if (isLoading) {
    return (
      <div className="stock-movements stock-movements--loading">
        <div className="loading-spinner" />
        <p>{t('common.loading', 'Loading...')}</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="stock-movements stock-movements--error">
        <p>{t('common.error', 'Error loading report')}</p>
      </div>
    );
  }

  const getMovementTypeColor = (type: string) => {
    switch (type.toLowerCase()) {
      case 'receipt':
        return 'success';
      case 'issue':
        return 'danger';
      case 'transfer':
        return 'primary';
      case 'adjustment':
        return 'warning';
      default:
        return '';
    }
  };

  return (
    <div className="stock-movements">
      <div className="stock-movements__filters">
        <div className="stock-movements__filter">
          <label>{t('reports.warehouse', 'Warehouse')}</label>
          <select
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            className="stock-movements__select"
          >
            <option value="">{t('reports.allWarehouses', 'All Warehouses')}</option>
            {warehouses.map((wh) => (
              <option key={wh.id} value={wh.id}>
                {wh.name}
              </option>
            ))}
          </select>
        </div>
        <div className="stock-movements__filter">
          <label>{t('reports.dateFrom', 'From')}</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="stock-movements__input"
          />
        </div>
        <div className="stock-movements__filter">
          <label>{t('reports.dateTo', 'To')}</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="stock-movements__input"
          />
        </div>
        {report?.generatedAt && (
          <div className="stock-movements__generated">
            {t('reports.generatedAt', 'Generated')}: {new Date(report.generatedAt).toLocaleString()}
          </div>
        )}
      </div>

      {report && (
        <>
          {/* Movement Types Summary */}
          <div className="stock-movements__section">
            <h3 className="stock-movements__section-title">
              {t('reports.movementsByType', 'Movements by Type')}
            </h3>
            <div className="stock-movements__metrics">
              {report.movementsByType.map((item) => (
                <div key={item.movementType} className={`metric-card metric-card--${getMovementTypeColor(item.movementType)}`}>
                  <div className="metric-card__value">{item.count.toLocaleString()}</div>
                  <div className="metric-card__label">{item.movementType}</div>
                  <div className="metric-card__sub">{item.totalQuantity.toLocaleString()} units</div>
                </div>
              ))}
              {report.movementsByType.length === 0 && (
                <div className="stock-movements__empty">
                  {t('reports.noMovements', 'No movements in selected period')}
                </div>
              )}
            </div>
          </div>

          {/* Daily Movements Chart (simplified as table) */}
          <div className="stock-movements__section">
            <h3 className="stock-movements__section-title">
              {t('reports.dailyMovements', 'Daily Movements')}
            </h3>
            <div className="stock-movements__chart">
              <table className="stock-movements__table">
                <thead>
                  <tr>
                    <th>{t('reports.date', 'Date')}</th>
                    <th className="text-right">{t('reports.receipts', 'Receipts')}</th>
                    <th className="text-right">{t('reports.issues', 'Issues')}</th>
                    <th className="text-right">{t('reports.transfers', 'Transfers')}</th>
                    <th className="text-right">{t('reports.adjustments', 'Adjustments')}</th>
                    <th className="text-right">{t('reports.totalQty', 'Total Qty')}</th>
                  </tr>
                </thead>
                <tbody>
                  {report.dailyMovements.map((day) => (
                    <tr key={day.date}>
                      <td>{new Date(day.date).toLocaleDateString()}</td>
                      <td className="text-right text-success">{day.receiptCount}</td>
                      <td className="text-right text-danger">{day.issueCount}</td>
                      <td className="text-right text-primary">{day.transferCount}</td>
                      <td className="text-right text-warning">{day.adjustmentCount}</td>
                      <td className="text-right font-bold">{day.totalQuantityMoved.toLocaleString()}</td>
                    </tr>
                  ))}
                  {report.dailyMovements.length === 0 && (
                    <tr>
                      <td colSpan={6} className="text-center text-muted">
                        {t('common.noData', 'No data')}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Top Moved Products */}
          <div className="stock-movements__section">
            <h3 className="stock-movements__section-title">
              {t('reports.topMovedProducts', 'Top Moved Products')}
            </h3>
            <table className="stock-movements__table">
              <thead>
                <tr>
                  <th>{t('reports.sku', 'SKU')}</th>
                  <th>{t('reports.product', 'Product')}</th>
                  <th className="text-right">{t('reports.movements', 'Movements')}</th>
                  <th className="text-right">{t('reports.totalQuantity', 'Total Quantity')}</th>
                </tr>
              </thead>
              <tbody>
                {report.topMovedProducts.map((item) => (
                  <tr key={item.productId}>
                    <td><code>{item.sku}</code></td>
                    <td>{item.productName}</td>
                    <td className="text-right">{item.movementCount.toLocaleString()}</td>
                    <td className="text-right font-bold">{item.totalQuantityMoved.toLocaleString()}</td>
                  </tr>
                ))}
                {report.topMovedProducts.length === 0 && (
                  <tr>
                    <td colSpan={4} className="text-center text-muted">
                      {t('common.noData', 'No data')}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}

export default StockMovements;
