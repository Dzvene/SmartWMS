import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useGetOrderFulfillmentReportQuery } from '@/api/modules/reports';
import './OrderFulfillment.scss';

/**
 * Order Fulfillment Report
 *
 * Displays order fulfillment metrics, picking stats, and shipment tracking.
 */
export function OrderFulfillment() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [dateFrom, setDateFrom] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().split('T')[0];
  });
  const [dateTo, setDateTo] = useState<string>(() => new Date().toISOString().split('T')[0]);

  const { data: reportResponse, isLoading, error } = useGetOrderFulfillmentReportQuery({
    dateFrom,
    dateTo,
  });

  const report = reportResponse?.data;

  if (isLoading) {
    return (
      <div className="order-fulfillment order-fulfillment--loading">
        <div className="loading-spinner" />
        <p>{t('common.loading', 'Loading...')}</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="order-fulfillment order-fulfillment--error">
        <p>{t('common.error', 'Error loading report')}</p>
      </div>
    );
  }

  return (
    <div className="order-fulfillment">
      <div className="order-fulfillment__filters">
        <div className="order-fulfillment__filter">
          <label>{t('reports.dateFrom', 'From')}</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="order-fulfillment__input"
          />
        </div>
        <div className="order-fulfillment__filter">
          <label>{t('reports.dateTo', 'To')}</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="order-fulfillment__input"
          />
        </div>
        {report?.generatedAt && (
          <div className="order-fulfillment__generated">
            {t('reports.generatedAt', 'Generated')}: {new Date(report.generatedAt).toLocaleString()}
          </div>
        )}
      </div>

      {report && (
        <>
          {/* Fulfillment Rate */}
          <div className="order-fulfillment__highlight">
            <div className="highlight-card">
              <div className="highlight-card__value">{report.fulfillmentRatePercent}%</div>
              <div className="highlight-card__label">{t('reports.fulfillmentRate', 'Fulfillment Rate')}</div>
              <div className="highlight-card__bar">
                <div
                  className="highlight-card__progress"
                  style={{ width: `${report.fulfillmentRatePercent}%` }}
                />
              </div>
            </div>
          </div>

          {/* Sales Orders Metrics */}
          <div className="order-fulfillment__section">
            <h3 className="order-fulfillment__section-title">
              {t('reports.salesOrdersMetrics', 'Sales Orders')}
            </h3>
            <div className="order-fulfillment__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalSalesOrders.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalOrders', 'Total Orders')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.ordersDelivered.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.delivered', 'Delivered')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{report.ordersInProgress.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.inProgress', 'In Progress')}</div>
              </div>
              <div className="metric-card metric-card--warning">
                <div className="metric-card__value">{report.ordersPending.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.pending', 'Pending')}</div>
              </div>
              <div className="metric-card metric-card--danger">
                <div className="metric-card__value">{report.ordersCancelled.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.cancelled', 'Cancelled')}</div>
              </div>
            </div>
          </div>

          {/* Picking Metrics */}
          <div className="order-fulfillment__section">
            <h3 className="order-fulfillment__section-title">
              {t('reports.pickingMetrics', 'Picking Tasks')}
            </h3>
            <div className="order-fulfillment__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalPickTasks.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalPickTasks', 'Total Tasks')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.pickTasksCompleted.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.completed', 'Completed')}</div>
              </div>
              <div className="metric-card metric-card--warning">
                <div className="metric-card__value">{report.pickTasksPending.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.pending', 'Pending')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{report.pickCompletionRatePercent}%</div>
                <div className="metric-card__label">{t('reports.completionRate', 'Completion Rate')}</div>
              </div>
            </div>
          </div>

          {/* Shipment Metrics */}
          <div className="order-fulfillment__section">
            <h3 className="order-fulfillment__section-title">
              {t('reports.shipmentMetrics', 'Shipments')}
            </h3>
            <div className="order-fulfillment__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalShipments.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalShipments', 'Total Shipments')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.shipmentsDelivered.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.delivered', 'Delivered')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{report.shipmentsInTransit.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.inTransit', 'In Transit')}</div>
              </div>
            </div>
          </div>

          {/* Daily Orders */}
          <div className="order-fulfillment__section">
            <h3 className="order-fulfillment__section-title">
              {t('reports.dailyOrders', 'Daily Order Activity')}
            </h3>
            <table className="order-fulfillment__table">
              <thead>
                <tr>
                  <th>{t('reports.date', 'Date')}</th>
                  <th className="text-right">{t('reports.created', 'Created')}</th>
                  <th className="text-right">{t('reports.shipped', 'Shipped')}</th>
                  <th className="text-right">{t('reports.delivered', 'Delivered')}</th>
                </tr>
              </thead>
              <tbody>
                {report.dailyOrders.map((day) => (
                  <tr key={day.date}>
                    <td>{new Date(day.date).toLocaleDateString()}</td>
                    <td className="text-right">{day.ordersCreated}</td>
                    <td className="text-right text-primary">{day.ordersShipped}</td>
                    <td className="text-right text-success">{day.ordersDelivered}</td>
                  </tr>
                ))}
                {report.dailyOrders.length === 0 && (
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

export default OrderFulfillment;
