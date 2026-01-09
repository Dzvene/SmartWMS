import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useGetWarehouseUtilizationQuery } from '@/api/modules/reports';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import './WarehouseUtilization.scss';

/**
 * Warehouse Utilization Report
 *
 * Displays warehouse capacity utilization by zone.
 */
export function WarehouseUtilization() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [warehouseId, setWarehouseId] = useState<string>('');

  const { data: warehousesResponse } = useGetWarehousesQuery();
  const warehouses = warehousesResponse?.data?.items || [];

  const { data: reportResponse, isLoading, error } = useGetWarehouseUtilizationQuery(
    warehouseId,
    { skip: !warehouseId }
  );

  const report = reportResponse?.data;

  const getUtilizationColor = (percent: number) => {
    if (percent >= 90) return 'danger';
    if (percent >= 70) return 'warning';
    if (percent >= 30) return 'success';
    return 'primary';
  };

  return (
    <div className="warehouse-utilization">
      <div className="warehouse-utilization__filters">
        <div className="warehouse-utilization__filter">
          <label>{t('reports.selectWarehouse', 'Select Warehouse')}</label>
          <select
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            className="warehouse-utilization__select"
          >
            <option value="">{t('reports.selectWarehouse', 'Select Warehouse...')}</option>
            {warehouses.map((wh) => (
              <option key={wh.id} value={wh.id}>
                {wh.name}
              </option>
            ))}
          </select>
        </div>
        {report?.generatedAt && (
          <div className="warehouse-utilization__generated">
            {t('reports.generatedAt', 'Generated')}: {new Date(report.generatedAt).toLocaleString()}
          </div>
        )}
      </div>

      {!warehouseId && (
        <div className="warehouse-utilization__empty">
          <div className="warehouse-utilization__empty-icon">🏭</div>
          <p>{t('reports.selectWarehousePrompt', 'Please select a warehouse to view utilization data')}</p>
        </div>
      )}

      {warehouseId && isLoading && (
        <div className="warehouse-utilization warehouse-utilization--loading">
          <div className="loading-spinner" />
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      )}

      {warehouseId && error && (
        <div className="warehouse-utilization warehouse-utilization--error">
          <p>{t('common.error', 'Error loading report')}</p>
        </div>
      )}

      {report && (
        <>
          {/* Overall Utilization */}
          <div className="warehouse-utilization__highlight">
            <div className="highlight-card">
              <div className="highlight-card__header">
                <span className="highlight-card__name">{report.warehouseName}</span>
              </div>
              <div className={`highlight-card__value highlight-card__value--${getUtilizationColor(report.overallUtilizationPercent)}`}>
                {report.overallUtilizationPercent}%
              </div>
              <div className="highlight-card__label">{t('reports.overallUtilization', 'Overall Utilization')}</div>
              <div className="highlight-card__bar">
                <div
                  className={`highlight-card__progress highlight-card__progress--${getUtilizationColor(report.overallUtilizationPercent)}`}
                  style={{ width: `${report.overallUtilizationPercent}%` }}
                />
              </div>
            </div>
          </div>

          {/* Location Summary */}
          <div className="warehouse-utilization__section">
            <h3 className="warehouse-utilization__section-title">
              {t('reports.locationSummary', 'Location Summary')}
            </h3>
            <div className="warehouse-utilization__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{report.totalLocations.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.totalLocations', 'Total Locations')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{report.occupiedLocations.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.occupied', 'Occupied')}</div>
              </div>
              <div className="metric-card">
                <div className="metric-card__value">{report.emptyLocations.toLocaleString()}</div>
                <div className="metric-card__label">{t('reports.empty', 'Empty')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{report.estimatedCapacityUsedPercent}%</div>
                <div className="metric-card__label">{t('reports.capacityUsed', 'Capacity Used')}</div>
              </div>
            </div>
          </div>

          {/* Zone Utilization */}
          <div className="warehouse-utilization__section">
            <h3 className="warehouse-utilization__section-title">
              {t('reports.zoneUtilization', 'Zone Utilization')}
            </h3>
            <div className="warehouse-utilization__zones">
              {report.zoneUtilizations.map((zone) => (
                <div key={zone.zoneId} className="zone-card">
                  <div className="zone-card__header">
                    <span className="zone-card__name">{zone.zoneName}</span>
                    <span className="zone-card__type">{zone.zoneType}</span>
                  </div>
                  <div className="zone-card__stats">
                    <div className="zone-card__stat">
                      <span className="zone-card__stat-value">{zone.occupiedLocations}</span>
                      <span className="zone-card__stat-label">/ {zone.totalLocations}</span>
                    </div>
                    <div className={`zone-card__percent zone-card__percent--${getUtilizationColor(zone.utilizationPercent)}`}>
                      {zone.utilizationPercent}%
                    </div>
                  </div>
                  <div className="zone-card__bar">
                    <div
                      className={`zone-card__progress zone-card__progress--${getUtilizationColor(zone.utilizationPercent)}`}
                      style={{ width: `${zone.utilizationPercent}%` }}
                    />
                  </div>
                </div>
              ))}
              {report.zoneUtilizations.length === 0 && (
                <div className="warehouse-utilization__empty-zones">
                  {t('reports.noZones', 'No zones found')}
                </div>
              )}
            </div>
          </div>

          {/* Zone Table */}
          <div className="warehouse-utilization__section">
            <h3 className="warehouse-utilization__section-title">
              {t('reports.zoneDetails', 'Zone Details')}
            </h3>
            <table className="warehouse-utilization__table">
              <thead>
                <tr>
                  <th>{t('reports.zone', 'Zone')}</th>
                  <th>{t('reports.type', 'Type')}</th>
                  <th className="text-right">{t('reports.total', 'Total')}</th>
                  <th className="text-right">{t('reports.occupied', 'Occupied')}</th>
                  <th className="text-right">{t('reports.empty', 'Empty')}</th>
                  <th className="text-right">{t('reports.utilization', 'Utilization')}</th>
                </tr>
              </thead>
              <tbody>
                {report.zoneUtilizations.map((zone) => (
                  <tr key={zone.zoneId}>
                    <td><strong>{zone.zoneName}</strong></td>
                    <td><span className="zone-type-badge">{zone.zoneType}</span></td>
                    <td className="text-right">{zone.totalLocations}</td>
                    <td className="text-right text-success">{zone.occupiedLocations}</td>
                    <td className="text-right">{zone.totalLocations - zone.occupiedLocations}</td>
                    <td className={`text-right text-${getUtilizationColor(zone.utilizationPercent)}`}>
                      <strong>{zone.utilizationPercent}%</strong>
                    </td>
                  </tr>
                ))}
                {report.zoneUtilizations.length === 0 && (
                  <tr>
                    <td colSpan={6} className="text-center text-muted">
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

export default WarehouseUtilization;
