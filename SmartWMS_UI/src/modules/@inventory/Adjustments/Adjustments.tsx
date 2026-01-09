import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
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
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });
  const [selectedAdjustment, setSelectedAdjustment] = useState<StockAdjustmentSummaryDto | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetAdjustmentsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const adjustments = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<StockAdjustmentSummaryDto, unknown>[]>(() => [
    { accessorKey: 'adjustmentNumber', header: 'Adj #', size: 100 },
    { accessorKey: 'adjustmentType', header: 'Type', size: 100 },
    { accessorKey: 'warehouseName', header: 'Warehouse', size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'reasonCodeName', header: 'Reason', size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'totalLines', header: 'Lines', size: 80, meta: { align: 'right' } },
    {
      accessorKey: 'totalQuantityChange',
      header: 'Qty Change',
      size: 100,
      meta: { align: 'right' },
      cell: ({ getValue }) => {
        const v = getValue() as number;
        return <span className={v > 0 ? 'text-success' : v < 0 ? 'text-error' : ''}>{v > 0 ? `+${v}` : v}</span>;
      },
    },
    { accessorKey: 'createdByUserName', header: 'Created By', size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'createdAt',
      header: 'Date',
      size: 100,
      cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
    },
    {
      accessorKey: 'status',
      header: t('common.status'),
      size: 100,
      cell: ({ getValue }) => {
        const status = getValue() as AdjustmentStatus;
        return <span className={`status-badge status-badge--${statusClasses[status]}`}>{statusLabels[status]}</span>;
      },
    },
  ], [t]);

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('nav.inventory.adjustments')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary">New Adjustment</button>
        </div>
      </div>
      <div className="page__content">
        <DataTable
          data={adjustments}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={setSelectedAdjustment}
          selectedRowId={selectedAdjustment?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>
    </div>
  );
}

export default Adjustments;
