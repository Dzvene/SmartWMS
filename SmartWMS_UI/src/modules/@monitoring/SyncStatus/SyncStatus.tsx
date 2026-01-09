import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { DataTable, createColumns } from '@/components/DataTable';
import { useGetSyncJobsQuery } from '@/api/modules/integrations';
import type { SyncJobDto, SyncStatus as SyncStatusType, SyncDirection } from '@/api/modules/integrations';
import './SyncStatus.scss';

const statusMap: Record<SyncStatusType, string> = {
  Pending: 'inactive',
  InProgress: 'active',
  Completed: 'completed',
  Failed: 'error',
  PartiallyCompleted: 'warning',
};

const directionLabels: Record<SyncDirection, string> = {
  Inbound: 'inbound',
  Outbound: 'outbound',
  Bidirectional: 'bidirectional',
};

/**
 * Sync Status Module
 *
 * Monitors integration sync jobs and their status.
 */
export function SyncStatus() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetSyncJobsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const jobs = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columnHelper = createColumns<SyncJobDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('integrationName', {
        header: t('common.name'),
        size: 180,
        cell: ({ getValue }) => <strong>{getValue() || '—'}</strong>,
      }),
      columnHelper.accessor('entityType', {
        header: 'Entity',
        size: 100,
      }),
      columnHelper.accessor('direction', {
        header: t('common.type'),
        size: 120,
        cell: ({ getValue }) => (
          <span className={`sync-type sync-type--${directionLabels[getValue()]}`}>
            {getValue()}
          </span>
        ),
      }),
      columnHelper.accessor('status', {
        header: t('common.status'),
        size: 120,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${statusMap[status]}`}>
              {status === 'InProgress' && <span className="status-badge__dot" />}
              {status}
            </span>
          );
        },
      }),
      columnHelper.accessor('startedAt', {
        header: t('sync.lastRun'),
        size: 160,
        cell: ({ getValue }) => {
          const value = getValue();
          if (!value) return <span className="text-muted">—</span>;
          return new Date(value).toLocaleString();
        },
      }),
      columnHelper.accessor('completedAt', {
        header: 'Completed',
        size: 160,
        cell: ({ getValue }) => {
          const value = getValue();
          if (!value) return <span className="text-muted">—</span>;
          return new Date(value).toLocaleString();
        },
      }),
      columnHelper.accessor('processedRecords', {
        header: 'Processed',
        size: 100,
        cell: ({ row }) => `${row.original.processedRecords}/${row.original.totalRecords}`,
      }),
      columnHelper.accessor('failedRecords', {
        header: 'Failed',
        size: 80,
        cell: ({ getValue }) => {
          const count = getValue();
          if (count === 0) return <span className="text-muted">0</span>;
          return <span className="text-error">{count}</span>;
        },
      }),
    ],
    [columnHelper, t]
  );

  const runningCount = jobs.filter(j => j.status === 'InProgress').length;
  const errorCount = jobs.filter(j => j.status === 'Failed').length;

  return (
    <div className="sync-status">
      <header className="sync-status__header">
        <div className="sync-status__title-section">
          <h1 className="sync-status__title">{t('sync.title')}</h1>
        </div>
        <div className="sync-status__actions">
          <button className="btn btn-secondary">
            {t('common.refresh')}
          </button>
        </div>
      </header>

      <div className="sync-status__summary">
        <div className="sync-status__stat">
          <span className="sync-status__stat-value">{totalRows}</span>
          <span className="sync-status__stat-label">Total Jobs</span>
        </div>
        <div className="sync-status__stat sync-status__stat--running">
          <span className="sync-status__stat-value">{runningCount}</span>
          <span className="sync-status__stat-label">{t('sync.running')}</span>
        </div>
        <div className="sync-status__stat sync-status__stat--error">
          <span className="sync-status__stat-value">{errorCount}</span>
          <span className="sync-status__stat-label">{t('sync.error')}</span>
        </div>
      </div>

      <div className="sync-status__content">
        <DataTable
          data={jobs}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>
    </div>
  );
}

export default SyncStatus;
