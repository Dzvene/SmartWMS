import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetPickTasksQuery } from '@/api/modules/fulfillment';
import type { PickTaskDto, PickTaskStatus } from '@/api/modules/fulfillment';
import { OUTBOUND } from '@/constants/routes';
import './Picking.scss';

const STATUS_COLORS: Record<PickTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Completed: 'success',
  ShortPicked: 'warning',
  Cancelled: 'error',
};

/**
 * Picking Module
 * Manages pick tasks generated from fulfillment batches.
 */
export function Picking() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<PickTaskStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetPickTasksQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const tasks = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<PickTaskDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('taskNumber', {
        header: t('picking.taskNumber', 'Task #'),
        size: 110,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('batchNumber', {
        header: t('picking.batch', 'Batch'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('sku', {
        header: t('products.sku', 'SKU'),
        size: 100,
        cell: ({ getValue }) => <span className="sku">{getValue()}</span>,
      }),
      columnHelper.accessor('productName', {
        header: t('common.product', 'Product'),
        size: 150,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('fromLocationCode', {
        header: t('picking.fromLocation', 'From'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('toLocationCode', {
        header: t('picking.toLocation', 'To'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.display({
        id: 'quantity',
        header: t('picking.quantity', 'Qty'),
        size: 100,
        cell: ({ row }) => {
          const { quantityPicked, quantityRequired } = row.original;
          return (
            <span>
              {quantityPicked}/{quantityRequired}
            </span>
          );
        },
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
      columnHelper.accessor('assignedToUserName', {
        header: t('picking.assignedTo', 'Assigned To'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (task: PickTaskDto) => {
    setSelectedId(task.id);
    navigate(`${OUTBOUND.PICKING}/${task.id}`);
  };

  return (
    <div className="picking">
      <header className="picking__header">
        <div className="picking__title-section">
          <h1 className="picking__title">{t('picking.title', 'Picking')}</h1>
          <p className="picking__subtitle">{t('picking.subtitle', 'Manage pick tasks')}</p>
        </div>
        <div className="picking__actions">
          <button className="btn btn--secondary">
            {t('common.refresh', 'Refresh')}
          </button>
        </div>
      </header>

      <div className="picking__toolbar">
        <div className="picking__search">
          <input
            type="search"
            className="picking__search-input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="picking__filters">
          <select
            className="select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as PickTaskStatus | '')}
          >
            <option value="">{t('picking.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="Assigned">Assigned</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
            <option value="ShortPicked">Short Picked</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="picking__content">
        <DataTable
          data={tasks}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedId}
          getRowId={(row) => row.id}
          emptyMessage={t('picking.noTasks', 'No pick tasks found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Picking;
