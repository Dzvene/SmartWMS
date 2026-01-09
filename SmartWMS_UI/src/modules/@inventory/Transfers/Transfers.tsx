import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { useGetTransfersQuery } from '@/api/modules/transfers';
import type { StockTransferSummaryDto, TransferStatus, TransferPriority } from '@/api/modules/transfers';

const statusLabels: Record<TransferStatus, string> = {
  Draft: 'Draft',
  Released: 'Released',
  InProgress: 'In Progress',
  PartiallyCompleted: 'Partial',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
};

const statusClasses: Record<TransferStatus, string> = {
  Draft: 'draft',
  Released: 'pending',
  InProgress: 'in_progress',
  PartiallyCompleted: 'review',
  Completed: 'completed',
  Cancelled: 'cancelled',
};

const priorityClasses: Record<TransferPriority, string> = {
  Low: 'priority-low',
  Normal: 'priority-normal',
  High: 'priority-high',
  Urgent: 'priority-urgent',
};

export function Transfers() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });
  const [selectedTransfer, setSelectedTransfer] = useState<StockTransferSummaryDto | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetTransfersQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const transfers = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<StockTransferSummaryDto, unknown>[]>(() => [
    { accessorKey: 'transferNumber', header: 'Transfer #', size: 100 },
    { accessorKey: 'transferType', header: 'Type', size: 100 },
    {
      accessorKey: 'priority',
      header: 'Priority',
      size: 80,
      cell: ({ getValue }) => {
        const priority = getValue() as TransferPriority;
        return <span className={priorityClasses[priority]}>{priority}</span>;
      },
    },
    { accessorKey: 'sourceWarehouseName', header: 'From', size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'destinationWarehouseName', header: 'To', size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      id: 'progress',
      header: 'Progress',
      size: 100,
      cell: ({ row }) => `${row.original.completedLines}/${row.original.totalLines}`,
    },
    {
      accessorKey: 'scheduledDate',
      header: 'Scheduled',
      size: 100,
      cell: ({ getValue }) => getValue() ? new Date(getValue() as string).toLocaleDateString() : '—',
    },
    { accessorKey: 'assignedUserName', header: 'Assigned To', size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'status',
      header: t('common.status'),
      size: 100,
      cell: ({ getValue }) => {
        const status = getValue() as TransferStatus;
        return <span className={`status-badge status-badge--${statusClasses[status]}`}>{statusLabels[status]}</span>;
      },
    },
  ], [t]);

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('nav.inventory.transfers')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary">New Transfer</button>
        </div>
      </div>
      <div className="page__content">
        <DataTable
          data={transfers}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={setSelectedTransfer}
          selectedRowId={selectedTransfer?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>
    </div>
  );
}

export default Transfers;
