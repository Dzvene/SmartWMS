import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { useGetCycleCountsQuery } from '@/api/modules/cycleCount';
import type { CycleCountSessionListDto, CycleCountStatus } from '@/api/modules/cycleCount';

const statusLabels: Record<CycleCountStatus, string> = {
  Scheduled: 'Scheduled',
  InProgress: 'In Progress',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  PartiallyCompleted: 'Partial',
};

const statusClasses: Record<CycleCountStatus, string> = {
  Scheduled: 'scheduled',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
  PartiallyCompleted: 'review',
};

export function CycleCount() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });
  const [selectedTask, setSelectedTask] = useState<CycleCountSessionListDto | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetCycleCountsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const tasks = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<CycleCountSessionListDto, unknown>[]>(() => [
    { accessorKey: 'countNumber', header: 'Count #', size: 100 },
    { accessorKey: 'countType', header: 'Type', size: 80 },
    { accessorKey: 'warehouseName', header: 'Warehouse', size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'scheduledDate',
      header: 'Scheduled',
      size: 100,
      cell: ({ getValue }) => getValue() ? new Date(getValue() as string).toLocaleDateString() : '—',
    },
    {
      id: 'progress',
      header: 'Progress',
      size: 100,
      cell: ({ row }) => `${row.original.countedItems}/${row.original.totalItems}`,
    },
    { accessorKey: 'varianceItems', header: 'Variance', size: 80, meta: { align: 'right' } },
    { accessorKey: 'assignedUserName', header: 'Assigned To', size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'status',
      header: t('common.status'),
      size: 100,
      cell: ({ getValue }) => {
        const status = getValue() as CycleCountStatus;
        return <span className={`status-badge status-badge--${statusClasses[status]}`}>{statusLabels[status]}</span>;
      },
    },
  ], [t]);

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('nav.inventory.cycleCount')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary">Schedule Count</button>
        </div>
      </div>
      <div className="page__content">
        <DataTable
          data={tasks}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={setSelectedTask}
          selectedRowId={selectedTask?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>
    </div>
  );
}

export default CycleCount;
