import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetTaskQueueQuery, type UnifiedTaskDto } from '@/api/modules/operationHub';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import './Tasks.scss';

/**
 * Tasks Page
 *
 * Full task queue management with filtering and sorting.
 */
export function Tasks() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [warehouseId, setWarehouseId] = useState<string>('');
  const [taskType, setTaskType] = useState<string>('');
  const [status, setStatus] = useState<string>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([{ id: 'priority', desc: true }]);

  const { data: warehousesResponse } = useGetWarehousesQuery();
  const warehouses = warehousesResponse?.data?.items || [];

  const { data: tasksResponse, isLoading } = useGetTaskQueueQuery({
    warehouseId: warehouseId || undefined,
    taskType: taskType || undefined,
    status: status || undefined,
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
  });

  const tasks = tasksResponse?.data?.items || [];
  const totalRows = tasksResponse?.data?.totalCount || 0;

  const columnHelper = createColumns<UnifiedTaskDto>();

  const getTaskTypeIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'pick': return '📦';
      case 'pack': return '📋';
      case 'putaway': return '📥';
      case 'cyclecount': return '🔢';
      default: return '📝';
    }
  };

  const columns = useMemo(
    () => [
      columnHelper.accessor('taskType', {
        header: t('operations.type', 'Type'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="task-type">
            {getTaskTypeIcon(getValue())} {getValue()}
          </span>
        ),
      }),
      columnHelper.accessor('taskNumber', {
        header: t('operations.taskNumber', 'Task #'),
        size: 120,
        cell: ({ getValue }) => <code>{getValue()}</code>,
      }),
      columnHelper.accessor('status', {
        header: t('operations.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => {
          const status = getValue();
          const statusClass = status.toLowerCase().replace(/\s/g, '-');
          return <span className={`status-badge status-badge--${statusClass}`}>{status}</span>;
        },
      }),
      columnHelper.accessor('priority', {
        header: t('operations.priority', 'Priority'),
        size: 80,
        cell: ({ getValue }) => {
          const priority = getValue();
          const priorityClass = priority >= 8 ? 'high' : priority >= 5 ? 'medium' : 'low';
          return <span className={`priority priority--${priorityClass}`}>{priority}</span>;
        },
      }),
      columnHelper.accessor('sourceLocation', {
        header: t('operations.from', 'From'),
        size: 100,
        cell: ({ getValue }) => getValue() ? <code>{getValue()}</code> : '-',
      }),
      columnHelper.accessor('destinationLocation', {
        header: t('operations.to', 'To'),
        size: 100,
        cell: ({ getValue }) => getValue() ? <code>{getValue()}</code> : '-',
      }),
      columnHelper.accessor('sku', {
        header: t('operations.sku', 'SKU'),
        size: 120,
        cell: ({ getValue }) => getValue() ? <code>{getValue()}</code> : '-',
      }),
      columnHelper.accessor('productName', {
        header: t('operations.product', 'Product'),
        size: 180,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('quantity', {
        header: t('operations.qty', 'Qty'),
        size: 80,
        cell: ({ row, getValue }) => {
          const qty = getValue();
          const uom = row.original.uom;
          return qty ? `${qty} ${uom || ''}` : '-';
        },
      }),
      columnHelper.accessor('assignedToUserName', {
        header: t('operations.assignedTo', 'Assigned'),
        size: 120,
        cell: ({ getValue }) => getValue() || <span className="text-muted">Unassigned</span>,
      }),
      columnHelper.accessor('createdAt', {
        header: t('operations.created', 'Created'),
        size: 140,
        cell: ({ getValue }) => new Date(getValue()).toLocaleString(),
      }),
    ],
    [columnHelper, t]
  );

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('operations.tasks.title', 'Task Queue')}</h1>
          <p className="page__subtitle">{t('operations.tasks.subtitle', 'Manage all warehouse tasks')}</p>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-filters">
          <select
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            className="page-filter__select"
          >
            <option value="">{t('common.allWarehouses', 'All Warehouses')}</option>
            {warehouses.map((wh) => (
              <option key={wh.id} value={wh.id}>
                {wh.name}
              </option>
            ))}
          </select>
          <select
            value={taskType}
            onChange={(e) => setTaskType(e.target.value)}
            className="page-filter__select"
          >
            <option value="">{t('common.allTypes', 'All Types')}</option>
            <option value="Pick">Pick</option>
            <option value="Pack">Pack</option>
            <option value="Putaway">Putaway</option>
            <option value="CycleCount">Cycle Count</option>
          </select>
          <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="page-filter__select"
          >
            <option value="">{t('common.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="Assigned">Assigned</option>
            <option value="InProgress">In Progress</option>
            <option value="Complete">Complete</option>
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
          getRowId={(row) => row.id}
          emptyMessage={t('operations.noTasks', 'No tasks found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Tasks;
