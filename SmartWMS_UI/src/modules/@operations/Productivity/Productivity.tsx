import { useState } from 'react';
import { useIntl } from 'react-intl';
import {
  useGetProductivitySummaryQuery,
  useGetProductivityHistoryQuery,
} from '@/api/modules/operationHub';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import './Productivity.scss';

/**
 * Productivity Page
 *
 * Operator productivity metrics and leaderboard.
 */
export function Productivity() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [warehouseId, setWarehouseId] = useState<string>('');
  const [dateFrom, setDateFrom] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() - 7);
    return d.toISOString().split('T')[0];
  });
  const [dateTo, setDateTo] = useState<string>(() => new Date().toISOString().split('T')[0]);

  const { data: warehousesResponse } = useGetWarehousesQuery();
  const warehouses = warehousesResponse?.data?.items || [];

  const { data: summaryResponse, isLoading: summaryLoading } = useGetProductivitySummaryQuery({
    warehouseId: warehouseId || undefined,
    fromDate: dateFrom,
    toDate: dateTo,
  });

  const { data: historyResponse } = useGetProductivityHistoryQuery({
    warehouseId: warehouseId || undefined,
    fromDate: dateFrom,
    toDate: dateTo,
  });

  const summary = summaryResponse?.data;
  const history = historyResponse?.data?.items || [];

  const getMedalEmoji = (index: number) => {
    switch (index) {
      case 0: return '🥇';
      case 1: return '🥈';
      case 2: return '🥉';
      default: return '';
    }
  };

  return (
    <div className="productivity-page">
      <header className="productivity-page__header">
        <div className="productivity-page__title-section">
          <h1 className="productivity-page__title">{t('operations.productivity.title', 'Productivity')}</h1>
          <p className="productivity-page__subtitle">{t('operations.productivity.subtitle', 'Operator performance metrics')}</p>
        </div>
      </header>

      <div className="productivity-page__filters">
        <div className="productivity-page__filter">
          <label>{t('reports.warehouse', 'Warehouse')}</label>
          <select
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            className="productivity-page__select"
          >
            <option value="">{t('common.all', 'All Warehouses')}</option>
            {warehouses.map((wh) => (
              <option key={wh.id} value={wh.id}>
                {wh.name}
              </option>
            ))}
          </select>
        </div>
        <div className="productivity-page__filter">
          <label>{t('reports.dateFrom', 'From')}</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="productivity-page__input"
          />
        </div>
        <div className="productivity-page__filter">
          <label>{t('reports.dateTo', 'To')}</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="productivity-page__input"
          />
        </div>
      </div>

      {summaryLoading ? (
        <div className="productivity-page__loading">
          <div className="loading-spinner" />
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      ) : summary ? (
        <>
          {/* Summary Metrics */}
          <div className="productivity-page__section">
            <h2 className="productivity-page__section-title">{t('operations.summary', 'Summary')}</h2>
            <div className="productivity-page__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{summary.totalOperators}</div>
                <div className="metric-card__label">{t('operations.totalOperators', 'Operators')}</div>
              </div>
              <div className="metric-card">
                <div className="metric-card__value">{summary.totalWorkDays}</div>
                <div className="metric-card__label">{t('operations.workDays', 'Work Days')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{summary.totalTasksCompleted.toLocaleString()}</div>
                <div className="metric-card__label">{t('operations.tasksCompleted', 'Tasks')}</div>
              </div>
              <div className="metric-card metric-card--primary">
                <div className="metric-card__value">{summary.totalUnitsProcessed.toLocaleString()}</div>
                <div className="metric-card__label">{t('operations.unitsProcessed', 'Units')}</div>
              </div>
            </div>
          </div>

          {/* Averages */}
          <div className="productivity-page__section">
            <h2 className="productivity-page__section-title">{t('operations.averages', 'Averages')}</h2>
            <div className="productivity-page__metrics">
              <div className="metric-card">
                <div className="metric-card__value">{summary.avgTasksPerOperatorPerDay.toFixed(1)}</div>
                <div className="metric-card__label">{t('operations.tasksPerDay', 'Tasks/Day/Operator')}</div>
              </div>
              <div className="metric-card">
                <div className="metric-card__value">{summary.avgUnitsPerOperatorPerDay.toFixed(1)}</div>
                <div className="metric-card__label">{t('operations.unitsPerDay', 'Units/Day/Operator')}</div>
              </div>
              <div className="metric-card metric-card--success">
                <div className="metric-card__value">{summary.avgAccuracyRate.toFixed(1)}%</div>
                <div className="metric-card__label">{t('operations.avgAccuracy', 'Avg Accuracy')}</div>
              </div>
              <div className="metric-card">
                <div className="metric-card__value">{summary.avgPicksPerHour.toFixed(1)}</div>
                <div className="metric-card__label">{t('operations.picksPerHour', 'Picks/Hour')}</div>
              </div>
            </div>
          </div>

          {/* Top Performers */}
          {summary.topPerformers.length > 0 && (
            <div className="productivity-page__section">
              <h2 className="productivity-page__section-title">{t('operations.topPerformers', 'Top Performers')}</h2>
              <div className="productivity-page__leaderboard">
                {summary.topPerformers.map((performer, index) => (
                  <div key={performer.userId} className={`performer-card ${index < 3 ? 'performer-card--top' : ''}`}>
                    <div className="performer-card__rank">
                      {getMedalEmoji(index) || `#${index + 1}`}
                    </div>
                    <div className="performer-card__info">
                      <span className="performer-card__name">{performer.userName}</span>
                      <div className="performer-card__stats">
                        <span>{performer.tasksCompleted} tasks</span>
                        <span>{performer.unitsProcessed.toLocaleString()} units</span>
                        <span>{performer.accuracyRate.toFixed(1)}% accuracy</span>
                      </div>
                    </div>
                    <div className="performer-card__rate">
                      <span className="performer-card__rate-value">{performer.avgTasksPerHour.toFixed(1)}</span>
                      <span className="performer-card__rate-label">tasks/hr</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* History Table */}
          {history.length > 0 && (
            <div className="productivity-page__section">
              <h2 className="productivity-page__section-title">{t('operations.history', 'Daily History')}</h2>
              <table className="productivity-page__table">
                <thead>
                  <tr>
                    <th>{t('operations.date', 'Date')}</th>
                    <th>{t('operations.operator', 'Operator')}</th>
                    <th className="text-right">{t('operations.tasks', 'Tasks')}</th>
                    <th className="text-right">{t('operations.units', 'Units')}</th>
                    <th className="text-right">{t('operations.workTime', 'Work Time')}</th>
                    <th className="text-right">{t('operations.accuracy', 'Accuracy')}</th>
                    <th className="text-right">{t('operations.tasksPerHour', 'Tasks/hr')}</th>
                  </tr>
                </thead>
                <tbody>
                  {history.slice(0, 20).map((item) => (
                    <tr key={item.id}>
                      <td>{new Date(item.date).toLocaleDateString()}</td>
                      <td>{item.userName}</td>
                      <td className="text-right">{item.totalTasksCompleted}</td>
                      <td className="text-right">{(item.totalUnitsPicked + item.totalUnitsPacked + item.totalUnitsPutaway).toLocaleString()}</td>
                      <td className="text-right">{Math.floor(item.totalWorkMinutes / 60)}h {item.totalWorkMinutes % 60}m</td>
                      <td className={`text-right ${item.accuracyRate >= 98 ? 'text-success' : item.accuracyRate >= 95 ? '' : 'text-warning'}`}>
                        {item.accuracyRate.toFixed(1)}%
                      </td>
                      <td className="text-right">{item.tasksPerHour.toFixed(1)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      ) : (
        <div className="productivity-page__empty">
          <p>{t('operations.noProductivityData', 'No productivity data for selected period')}</p>
        </div>
      )}
    </div>
  );
}

export default Productivity;
