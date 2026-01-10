import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetPackingTasksQuery } from '@/api/modules/packing';
import type { PackingTaskListDto, PackingTaskStatus } from '@/api/modules/packing';
import { OUTBOUND } from '@/constants/routes';

const STATUS_COLORS: Record<PackingTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Completed: 'success',
  Cancelled: 'error',
};

/**
 * Packing Module
 * Manages packing tasks for order fulfillment.
 */
export function Packing() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<PackingTaskStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetPackingTasksQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const tasks = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<PackingTaskListDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('taskNumber', {
        header: t('packing.taskNumber', 'Task #'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('salesOrderNumber', {
        header: t('packing.orderNumber', 'Order #'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('customerName', {
        header: t('common.customer', 'Customer'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('packingStationCode', {
        header: t('packing.station', 'Station'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.display({
        id: 'progress',
        header: t('packing.progress', 'Progress'),
        size: 100,
        cell: ({ row }) => {
          const { packedItems, totalItems } = row.original;
          return `${packedItems}/${totalItems}`;
        },
      }),
      columnHelper.accessor('boxCount', {
        header: t('packing.boxes', 'Boxes'),
        size: 70,
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 100,
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
        header: t('packing.assignedTo', 'Assigned To'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('priority', {
        header: t('common.priority', 'Priority'),
        size: 80,
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (task: PackingTaskListDto) => {
    setSelectedId(task.id);
    navigate(`${OUTBOUND.PACKING}/${task.id}`);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('packing.title', 'Packing')}</h1>
          <p className="page__subtitle">{t('packing.subtitle', 'Manage packing tasks')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary">
            {t('packing.createTask', 'Create Task')}
          </button>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as PackingTaskStatus | '')}
          >
            <option value="">{t('packing.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="Assigned">Assigned</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
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
          emptyMessage={t('packing.noTasks', 'No packing tasks found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Packing;
