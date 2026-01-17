import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import type { ColumnDef } from '@tanstack/react-table';
import { useTranslate } from '@/hooks';
import { DataTable } from '@/components/DataTable';
import { INVENTORY } from '@/constants/routes';
import { useGetAdjustmentsQuery } from '@/api/modules/adjustments';
import type { StockAdjustmentSummaryDto, AdjustmentStatus } from '@/api/modules/adjustments';

const statusLabels: Record<AdjustmentStatus, string> = {
  Draft: 'Draft',
  PendingApproval: 'Pending',
  Approved: 'Approved',
  Posted: 'Posted',
  Cancelled: 'Cancelled',
};

const statusClasses: Record<AdjustmentStatus, string> = {
  Draft: 'draft',
  PendingApproval: 'pending',
  Approved: 'approved',
  Posted: 'completed',
  Cancelled: 'cancelled',
};

export function Adjustments() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetAdjustmentsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const adjustments = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<StockAdjustmentSummaryDto, unknown>[]>(() => [
    { accessorKey: 'adjustmentNumber', header: t('adjustments.adjustmentNumber', 'Adj #'), size: 100 },
    { accessorKey: 'adjustmentType', header: t('common.type', 'Type'), size: 100 },
    { accessorKey: 'warehouseName', header: t('common.warehouse', 'Warehouse'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'reasonCodeName', header: t('adjustments.reason', 'Reason'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'totalLines', header: t('adjustments.lines', 'Lines'), size: 80, meta: { align: 'right' } },
    {
      accessorKey: 'totalQuantityChange',
      header: t('adjustments.qtyChange', 'Qty Change'),
      size: 100,
      meta: { align: 'right' },
      cell: ({ getValue }) => {
        const v = getValue() as number;
        return <span className={v > 0 ? 'text-success' : v < 0 ? 'text-error' : ''}>{v > 0 ? `+${v}` : v}</span>;
      },
    },
    { accessorKey: 'createdByUserName', header: t('adjustments.createdBy', 'Created By'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'createdAt',
      header: t('common.date', 'Date'),
      size: 100,
      cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
    },
    {
      accessorKey: 'status',
      header: t('common.status', 'Status'),
      size: 100,
      cell: ({ getValue }) => {
        const status = getValue() as AdjustmentStatus;
        return <span className={`status-badge status-badge--${statusClasses[status]}`}>{statusLabels[status]}</span>;
      },
    },
  ], [t]);

  const handleRowClick = (adjustment: StockAdjustmentSummaryDto) => {
    setSelectedId(adjustment.id);
    navigate(`${INVENTORY.ADJUSTMENTS}/${adjustment.id}`);
  };

  const handleCreate = () => {
    navigate(INVENTORY.ADJUSTMENT_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('adjustments.title', 'Adjustments')}</h1>
          <p className="page__subtitle">{t('adjustments.subtitle', 'Manage stock adjustments')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreate}>
            {t('adjustments.newAdjustment', 'New Adjustment')}
          </button>
        </div>
      </header>

      <div className="page__content">
        <DataTable
          data={adjustments}
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

export default Adjustments;
