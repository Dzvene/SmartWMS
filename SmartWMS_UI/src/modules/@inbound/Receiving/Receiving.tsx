import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetGoodsReceiptsQuery } from '@/api/modules/receiving';
import type { GoodsReceiptDto, GoodsReceiptStatus } from '@/api/modules/receiving';
import { INBOUND } from '@/constants/routes';

const STATUS_COLORS: Record<GoodsReceiptStatus, string> = {
  Draft: 'neutral',
  Pending: 'warning',
  InProgress: 'info',
  Completed: 'success',
  PartiallyReceived: 'warning',
  Cancelled: 'error',
};

/**
 * Receiving Module
 * Manages inbound receipt processing from purchase orders.
 */
export function Receiving() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<GoodsReceiptStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // API Queries
  const { data: response, isLoading } = useGetGoodsReceiptsQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const receipts = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<GoodsReceiptDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('receiptNumber', {
        header: t('receiving.receiptNumber', 'Receipt #'),
        size: 130,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('purchaseOrderNumber', {
        header: t('receiving.poNumber', 'PO #'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('supplierName', {
        header: t('common.supplier', 'Supplier'),
        size: 150,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('warehouseName', {
        header: t('common.warehouse', 'Warehouse'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('receiptDate', {
        header: t('receiving.receiptDate', 'Receipt Date'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 120,
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
        header: t('receiving.progress', 'Progress'),
        size: 120,
        cell: ({ row }) => {
          const { totalQuantityReceived, totalQuantityExpected, progressPercent } = row.original;
          return (
            <div className="progress-cell">
              <span>{totalQuantityReceived}/{totalQuantityExpected}</span>
              <span className="progress-cell__percent">({progressPercent}%)</span>
            </div>
          );
        },
      }),
      columnHelper.accessor('totalLines', {
        header: t('receiving.lines', 'Lines'),
        size: 70,
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (receipt: GoodsReceiptDto) => {
    setSelectedId(receipt.id);
    navigate(`${INBOUND.RECEIVING}/${receipt.id}`);
  };

  const handleCreateReceipt = () => {
    navigate(INBOUND.RECEIVING_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('receiving.title', 'Receiving')}</h1>
          <p className="page__subtitle">{t('receiving.subtitle', 'Process inbound receipts')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateReceipt}>
            {t('receiving.startReceiving', 'Start Receiving')}
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
            onChange={(e) => setStatusFilter(e.target.value as GoodsReceiptStatus | '')}
          >
            <option value="">{t('receiving.allStatuses', 'All Statuses')}</option>
            <option value="Draft">Draft</option>
            <option value="Pending">Pending</option>
            <option value="InProgress">In Progress</option>
            <option value="PartiallyReceived">Partially Received</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={receipts}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedId}
          getRowId={(row) => row.id}
          emptyMessage={t('receiving.noReceipts', 'No receipts found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Receiving;
