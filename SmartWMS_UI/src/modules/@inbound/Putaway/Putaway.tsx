import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetPutawayTasksQuery } from '@/api/modules/putaway';
import type { PutawayTaskDto, PutawayTaskStatus } from '@/api/modules/putaway';
import { INBOUND } from '@/constants/routes';

const STATUS_COLORS: Record<PutawayTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Complete: 'success',
  Cancelled: 'error',
};

/**
 * Putaway Module
 * Manages putaway tasks for received inventory.
 */
export function Putaway() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<PutawayTaskStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // API Queries
  const { data: response, isLoading } = useGetPutawayTasksQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const tasks = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<PutawayTaskDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('taskNumber', {
        header: t('putaway.taskNumber', 'Task #'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('sku', {
        header: t('products.sku', 'SKU'),
        size: 100,
        cell: ({ getValue }) => <span className="sku">{getValue() || '-'}</span>,
      }),
      columnHelper.accessor('productName', {
        header: t('common.product', 'Product'),
        size: 150,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.display({
        id: 'quantity',
        header: t('common.quantity', 'Qty'),
        size: 100,
        cell: ({ row }) => {
          const { quantityPutaway, quantityToPutaway } = row.original;
          return (
            <span>
              {quantityPutaway}/{quantityToPutaway}
            </span>
          );
        },
      }),
      columnHelper.accessor('fromLocationCode', {
        header: t('putaway.fromLocation', 'From'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('suggestedLocationCode', {
        header: t('putaway.suggestedLocation', 'Suggested'),
        size: 110,
        cell: ({ getValue }) => getValue() || '-',
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
        header: t('putaway.assignedTo', 'Assigned To'),
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

  const handleRowClick = (task: PutawayTaskDto) => {
    setSelectedId(task.id);
    navigate(`${INBOUND.PUTAWAY}/${task.id}`);
  };

  const handleCreateTask = () => {
    navigate(INBOUND.PUTAWAY_CREATE);
  };

  const handleAutoAssign = () => {
    // TODO: This would trigger an API call to auto-assign pending tasks
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('putaway.title', 'Putaway')}</h1>
          <p className="page__subtitle">{t('putaway.subtitle', 'Manage putaway tasks')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--secondary" onClick={handleAutoAssign}>
            {t('putaway.autoAssign', 'Auto Assign')}
          </button>
          <button className="btn btn--primary" onClick={handleCreateTask}>
            {t('putaway.createTask', 'Create Task')}
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
            onChange={(e) => setStatusFilter(e.target.value as PutawayTaskStatus | '')}
          >
            <option value="">{t('putaway.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="Assigned">Assigned</option>
            <option value="InProgress">In Progress</option>
            <option value="Complete">Complete</option>
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
          emptyMessage={t('putaway.noTasks', 'No putaway tasks found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Putaway;
