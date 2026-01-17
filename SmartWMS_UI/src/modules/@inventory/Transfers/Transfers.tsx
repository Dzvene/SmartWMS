import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import type { ColumnDef } from '@tanstack/react-table';
import { useTranslate } from '@/hooks';
import { DataTable } from '@/components/DataTable';
import { INVENTORY } from '@/constants/routes';
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
  const t = useTranslate();
  const navigate = useNavigate();

  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetTransfersQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const transfers = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<StockTransferSummaryDto, unknown>[]>(() => [
    { accessorKey: 'transferNumber', header: t('transfers.transferNumber', 'Transfer #'), size: 100 },
    { accessorKey: 'transferType', header: t('common.type', 'Type'), size: 100 },
    {
      accessorKey: 'priority',
      header: t('common.priority', 'Priority'),
      size: 80,
      cell: ({ getValue }) => {
        const priority = getValue() as TransferPriority;
        return <span className={priorityClasses[priority]}>{priority}</span>;
      },
    },
    { accessorKey: 'sourceWarehouseName', header: t('transfers.from', 'From'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'destinationWarehouseName', header: t('transfers.to', 'To'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      id: 'progress',
      header: t('transfers.progress', 'Progress'),
      size: 100,
      cell: ({ row }) => `${row.original.completedLines}/${row.original.totalLines}`,
    },
    {
      accessorKey: 'scheduledDate',
      header: t('transfers.scheduled', 'Scheduled'),
      size: 100,
      cell: ({ getValue }) => getValue() ? new Date(getValue() as string).toLocaleDateString() : '—',
    },
    { accessorKey: 'assignedUserName', header: t('transfers.assignedTo', 'Assigned To'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'status',
      header: t('common.status', 'Status'),
      size: 100,
      cell: ({ getValue }) => {
        const status = getValue() as TransferStatus;
        return <span className={`status-badge status-badge--${statusClasses[status]}`}>{statusLabels[status]}</span>;
      },
    },
  ], [t]);

  const handleRowClick = (transfer: StockTransferSummaryDto) => {
    setSelectedId(transfer.id);
    navigate(`${INVENTORY.TRANSFERS}/${transfer.id}`);
  };

  const handleCreate = () => {
    navigate(INVENTORY.TRANSFER_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('transfers.title', 'Transfers')}</h1>
          <p className="page__subtitle">{t('transfers.subtitle', 'Manage stock transfers')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreate}>
            {t('transfers.newTransfer', 'New Transfer')}
          </button>
        </div>
      </header>

      <div className="page__content">
        <DataTable
          data={transfers}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedId}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData', 'No data found')}
        />
      </div>
    </div>
  );
}

export default Transfers;
