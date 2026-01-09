import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetBatchesQuery } from '@/api/modules/fulfillment';
import type { FulfillmentBatchDto, BatchStatus } from '@/api/modules/fulfillment';
import './Fulfillment.scss';

const STATUS_COLORS: Record<BatchStatus, string> = {
  Open: 'neutral',
  InProgress: 'info',
  Completed: 'success',
  Cancelled: 'error',
};

/**
 * Fulfillment Module
 * Manages fulfillment batches - groups of orders processed together.
 */
export function Fulfillment() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<BatchStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetBatchesQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const batches = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<FulfillmentBatchDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('batchNumber', {
        header: t('fulfillment.batchNumber', 'Batch #'),
        size: 120,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('warehouseName', {
        header: t('common.warehouse', 'Warehouse'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 110,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${STATUS_COLORS[status]}`}>
              {status}
            </span>
          );
        },
      }),
      columnHelper.display({
        id: 'progress',
        header: t('fulfillment.progress', 'Progress'),
        size: 120,
        cell: ({ row }) => {
          const { completedTasks, totalTasks } = row.original;
          const percent = totalTasks > 0 ? Math.round((completedTasks / totalTasks) * 100) : 0;
          return (
            <div className="progress-cell">
              <span>{completedTasks}/{totalTasks} tasks</span>
              <span className="progress-cell__percent">({percent}%)</span>
            </div>
          );
        },
      }),
      columnHelper.accessor('assignedToUserName', {
        header: t('fulfillment.assignedTo', 'Assigned To'),
        size: 130,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('startedAt', {
        header: t('fulfillment.startedAt', 'Started'),
        size: 110,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('completedAt', {
        header: t('fulfillment.completedAt', 'Completed'),
        size: 110,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('createdAt', {
        header: t('common.createdAt', 'Created'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (batch: FulfillmentBatchDto) => {
    setSelectedId(batch.id);
  };

  return (
    <div className="fulfillment">
      <header className="fulfillment__header">
        <div className="fulfillment__title-section">
          <h1 className="fulfillment__title">{t('fulfillment.title', 'Fulfillment Batches')}</h1>
          <p className="fulfillment__subtitle">{t('fulfillment.subtitle', 'Manage order fulfillment batches')}</p>
        </div>
        <div className="fulfillment__actions">
          <button className="btn btn--primary">
            {t('fulfillment.createBatch', 'Create Batch')}
          </button>
        </div>
      </header>

      <div className="fulfillment__toolbar">
        <div className="fulfillment__search">
          <input
            type="search"
            className="fulfillment__search-input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="fulfillment__filters">
          <select
            className="select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as BatchStatus | '')}
          >
            <option value="">{t('fulfillment.allStatuses', 'All Statuses')}</option>
            <option value="Open">Open</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="fulfillment__content">
        <DataTable
          data={batches}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedId}
          getRowId={(row) => row.id}
          emptyMessage={t('fulfillment.noBatches', 'No fulfillment batches found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Fulfillment;
