import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useGetReceivingReportQuery } from '@/api/modules/reports';
import './Receiving.scss';

/**
 * Receiving Report
 *
 * Displays purchase order receiving metrics and quality statistics.
 */
export function Receiving() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [dateFrom, setDateFrom] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().split('T')[0];
  });
  const [dateTo, setDateTo] = useState<string>(() => new Date().toISOString().split('T')[0]);

  const { data: reportResponse, isLoading, error } = useGetReceivingReportQuery({
    dateFrom,
    dateTo,
  });

  const report = reportResponse?.data;

  if (isLoading) {
    return (
      <div className="receiving-report receiving-report--loading">
        <div className="loading-spinner" />
        <p>{t('common.loading', 'Loading...')}</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="receiving-report receiving-report--error">
        <p>{t('common.error', 'Error loading report')}</p>
      </div>
    );
  }

  return (
    <div className="receiving-report">
      <div className="receiving-report__filters">
        <div className="receiving-report__filter">
          <label>{t('reports.dateFrom', 'From')}</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="receiving-report__input"
          />
        </div>
        <div className="receiving-report__filter">
          <label>{t('reports.dateTo', 'To')}</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="receiving-report__input"
          />
        </div>
        {report?.generatedAt && (
          <div className="receiving-report__generated">
            {t('reports.generatedAt', 'Generated')}: {new Date(report.generatedAt).toLocaleString()}
          </div>
        )}
      </div>

      {report && (
        <>
          {/* Quality Pass Rate */}
          <div className="receiving-report__highlight">
            <div className="highlight-card">
              <div className="highlight-card__value">{report.qualityPassRatePercent}%</div>
              <div className="highlight-card__label">{t('reports.qualityPassRate', 'Quality Pass Rate')}</div>
              <div className="highlight-card__bar">
                <div
                  className={`highlight-card__progress ${report.qualityPassRatePercent >= 95 ? 'highlight-card__progress--success' : report.qualityPassRatePercent >= 80 ? 'highlight-card__progress--warning' : 'highlight-card__progress--danger'}`}
                  style={{ width: `${report.qualityPassRatePercent}%` }}
                />
              </div>
            </div>
          </div>

          {/* Purchase Orders Metrics */}
          <div className="receiving-report__section">
            <h3 className="receiving-report__section-title">
              {t('reports.purchaseOrders', 'Purchase Orders')}
            </h3>
            <div className="receiving-report__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalPurchaseOrders.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalPOs', 'Total POs')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.pOsReceived.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.received', 'Received')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{report.pOsPartiallyReceived.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.partiallyReceived', 'Partially Received')}</div>
              </div>
              <div className="metric-card metric-card--warning">
                <div className="metric-card__value">{report.pOsPending.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.pending', 'Pending')}</div>
              </div>
            </div>
          </div>

          {/* Goods Receipts Metrics */}
          <div className="receiving-report__section">
            <h3 className="receiving-report__section-title">
              {t('reports.goodsReceipts', 'Goods Receipts')}
            </h3>
            <div className="receiving-report__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalGoodsReceipts.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalReceipts', 'Total Receipts')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.receiptsCompleted.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.completed', 'Completed')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{report.receiptsInProgress.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.inProgress', 'In Progress')}</div>
              </div>
              <div className="metric-card metric-card--large">
                <div className="metric-card__value">{report.totalQuantityReceived.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalQtyReceived', 'Total Qty Received')}</div>
              </div>
            </div>
          </div>

          {/* Quality Metrics */}
          <div className="receiving-report__section">
            <h3 className="receiving-report__section-title">
              {t('reports.qualityBreakdown', 'Quality Breakdown')}
            </h3>
            <div className="receiving-report__quality">
              <div className="quality-bar">
                <div className="quality-bar__segment quality-bar__segment--good" style={{ width: `${report.totalQuantityReceived > 0 ? (report.quantityGood / report.totalQuantityReceived * 100) : 0}%` }}>
                  <span className="quality-bar__label">{t('reports.good', 'Good')}</span>
                  <span className="quality-bar__value">{report.quantityGood.toLocaleString()}</span>
                </div>
                <div className="quality-bar__segment quality-bar__segment--damaged" style={{ width: `${report.totalQuantityReceived > 0 ? (report.quantityDamaged / report.totalQuantityReceived * 100) : 0}%` }}>
                  {report.quantityDamaged > 0 && (
                    <>
                      <span className="quality-bar__label">{t('reports.damaged', 'Damaged')}</span>
                      <span className="quality-bar__value">{report.quantityDamaged.toLocaleString()}</span>
                    </>
                  )}
                </div>
                <div className="quality-bar__segment quality-bar__segment--quarantine" style={{ width: `${report.totalQuantityReceived > 0 ? (report.quantityQuarantine / report.totalQuantityReceived * 100) : 0}%` }}>
                  {report.quantityQuarantine > 0 && (
                    <>
                      <span className="quality-bar__label">{t('reports.quarantine', 'Quarantine')}</span>
                      <span className="quality-bar__value">{report.quantityQuarantine.toLocaleString()}</span>
                    </>
                  )}
                </div>
              </div>
              <div className="receiving-report__quality-legend">
                <div className="legend-item">
                  <span className="legend-item__color legend-item__color--good" />
                  <span className="legend-item__label">{t('reports.good', 'Good')}: {report.quantityGood.toLocaleString()}</span>
                </div>
                <div className="legend-item">
                  <span className="legend-item__color legend-item__color--damaged" />
                  <span className="legend-item__label">{t('reports.damaged', 'Damaged')}: {report.quantityDamaged.toLocaleString()}</span>
                </div>
                <div className="legend-item">
                  <span className="legend-item__color legend-item__color--quarantine" />
                  <span className="legend-item__label">{t('reports.quarantine', 'Quarantine')}: {report.quantityQuarantine.toLocaleString()}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Daily Receiving */}
          <div className="receiving-report__section">
            <h3 className="receiving-report__section-title">
              {t('reports.dailyReceiving', 'Daily Receiving Activity')}
            </h3>
            <table className="receiving-report__table">
              <thead>
                <tr>
                  <th>{t('reports.date', 'Date')}</th>
                  <th className="text-right">{t('reports.receiptsCreated', 'Created')}</th>
                  <th className="text-right">{t('reports.receiptsCompleted', 'Completed')}</th>
                  <th className="text-right">{t('reports.quantityReceived', 'Qty Received')}</th>
                </tr>
              </thead>
              <tbody>
                {report.dailyReceiving.map((day) => (
                  <tr key={day.date}>
                    <td>{new Date(day.date).toLocaleDateString()}</td>
                    <td className="text-right">{day.receiptsCreated}</td>
                    <td className="text-right text-success">{day.receiptsCompleted}</td>
                    <td className="text-right font-bold">{day.quantityReceived.toLocaleString()}</td>
                  </tr>
                ))}
                {report.dailyReceiving.length === 0 && (
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

export default Receiving;
