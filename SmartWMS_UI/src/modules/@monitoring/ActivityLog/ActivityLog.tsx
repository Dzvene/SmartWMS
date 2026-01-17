import { useState, useMemo } from 'react';
import { DataTable, createColumns } from '@/components/DataTable';
import { useTranslate } from '@/hooks';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetActivityLogsQuery } from '@/api/modules/audit';
import type { ActivityLogDto } from '@/api/modules/audit';
import './ActivityLog.scss';

const MODULE_COLORS: Record<string, string> = {
  auth: 'info',
  inventory: 'success',
  order: 'warning',
  orders: 'warning',
  config: 'neutral',
  system: 'error',
  fulfillment: 'info',
  receiving: 'success',
  shipping: 'warning',
};

/**
 * Activity Log Module
 * System-wide activity and audit log.
 */
export function ActivityLog() {
  const t = useTranslate();

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 50 });
  const [sorting, setSorting] = useState<SortingState>([{ id: 'activityTime', desc: true }]);
  const [moduleFilter, setModuleFilter] = useState<string>('');

  const { data: response, isLoading } = useGetActivityLogsQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    module: moduleFilter || undefined,
    searchTerm: searchQuery || undefined,
  });

  const activities = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<ActivityLogDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('activityTime', {
        header: t('activity.timestamp', 'Timestamp'),
        size: 170,
        cell: ({ getValue }) => new Date(getValue()).toLocaleString(),
      }),
      columnHelper.accessor('activityType', {
        header: t('activity.eventType', 'Event Type'),
        size: 140,
        cell: ({ getValue }) => <strong>{getValue()}</strong>,
      }),
      columnHelper.accessor('module', {
        header: t('activity.module', 'Module'),
        size: 100,
        cell: ({ getValue }) => {
          const module = getValue()?.toLowerCase() || 'other';
          const colorClass = MODULE_COLORS[module] || 'neutral';
          return (
            <span className={`activity-category activity-category--${colorClass}`}>
              {getValue() || '-'}
            </span>
          );
        },
      }),
      columnHelper.accessor('userName', {
        header: t('activity.user', 'User'),
        size: 120,
        cell: ({ getValue }) => <code className="code">{getValue() || '-'}</code>,
      }),
      columnHelper.accessor('description', {
        header: t('activity.details', 'Details'),
        size: 300,
      }),
      columnHelper.accessor('relatedEntityType', {
        header: t('activity.entityType', 'Entity'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('relatedEntityNumber', {
        header: t('activity.entityRef', 'Reference'),
        size: 120,
        cell: ({ getValue }) => getValue() ? <span className="code">{getValue()}</span> : '-',
      }),
      columnHelper.accessor('deviceType', {
        header: t('activity.device', 'Device'),
        size: 80,
        cell: ({ getValue }) => (
          <span className="text-muted">{getValue() || '-'}</span>
        ),
      }),
    ],
    [columnHelper, t]
  );

  return (
    <div className="activity-log">
      <header className="activity-log__header">
        <div className="activity-log__title-section">
          <h1 className="activity-log__title">{t('activity.title', 'Activity Log')}</h1>
          <p className="activity-log__subtitle">{t('activity.subtitle', 'System-wide activity and audit log')}</p>
        </div>
        <div className="activity-log__actions">
          <button className="btn btn-secondary">
            {t('common.export', 'Export')}
          </button>
        </div>
      </header>

      <div className="activity-log__toolbar">
        <div className="activity-log__search">
          <input
            type="search"
            className="activity-log__search-input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="activity-log__filters">
          <select
            className="activity-log__filter-select"
            value={moduleFilter}
            onChange={(e) => setModuleFilter(e.target.value)}
          >
            <option value="">{t('activity.allModules', 'All Modules')}</option>
            <option value="Auth">Auth</option>
            <option value="Inventory">Inventory</option>
            <option value="Orders">Orders</option>
            <option value="Fulfillment">Fulfillment</option>
            <option value="Receiving">Receiving</option>
            <option value="Shipping">Shipping</option>
            <option value="Config">Config</option>
            <option value="System">System</option>
          </select>
        </div>
      </div>

      <div className="activity-log__content">
        <DataTable
          data={activities}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          getRowId={(row) => row.id}
          emptyMessage={t('activity.noActivities', 'No activity logs found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default ActivityLog;
