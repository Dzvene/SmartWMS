import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetReturnOrdersQuery } from '@/api/modules/returns';
import type { ReturnOrderListDto, ReturnOrderStatus, ReturnType } from '@/api/modules/returns';
import { INBOUND } from '@/constants/routes';

const STATUS_COLORS: Record<ReturnOrderStatus, string> = {
  Pending: 'warning',
  InTransit: 'info',
  Received: 'info',
  InProgress: 'info',
  Complete: 'success',
  Cancelled: 'error',
};

const RETURN_TYPE_LABELS: Record<ReturnType, string> = {
  CustomerReturn: 'Customer',
  SupplierReturn: 'Supplier',
  InternalTransfer: 'Internal',
  Damaged: 'Damaged',
  Recall: 'Recall',
};

/**
 * Returns Module
 * Manages customer return orders and RMA processing.
 */
export function Returns() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<ReturnOrderStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // API Queries
  const { data: response, isLoading } = useGetReturnOrdersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    status: statusFilter || undefined,
    rmaNumber: searchQuery || undefined,
  });

  const returns = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<ReturnOrderListDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('returnNumber', {
        header: t('returns.returnNumber', 'Return #'),
        size: 120,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('rmaNumber', {
        header: t('returns.rmaNumber', 'RMA #'),
        size: 110,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('originalSalesOrderNumber', {
        header: t('returns.originalOrder', 'Original Order'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('customerName', {
        header: t('common.customer', 'Customer'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('returnType', {
        header: t('returns.type', 'Type'),
        size: 90,
        cell: ({ getValue }) => RETURN_TYPE_LABELS[getValue()] || getValue(),
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
      columnHelper.display({
        id: 'progress',
        header: t('returns.progress', 'Progress'),
        size: 100,
        cell: ({ row }) => {
          const { totalQuantityExpected, totalQuantityReceived } = row.original;
          const percent = totalQuantityExpected > 0
            ? Math.round((totalQuantityReceived / totalQuantityExpected) * 100)
            : 0;
          return (
            <span>
              {totalQuantityReceived}/{totalQuantityExpected} ({percent}%)
            </span>
          );
        },
      }),
      columnHelper.accessor('totalLines', {
        header: t('returns.lines', 'Lines'),
        size: 70,
      }),
      columnHelper.accessor('requestedDate', {
        header: t('returns.requestedDate', 'Requested'),
        size: 100,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('receivedDate', {
        header: t('returns.receivedDate', 'Received'),
        size: 100,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('createdAt', {
        header: t('common.createdAt', 'Created'),
        size: 100,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (returnOrder: ReturnOrderListDto) => {
    setSelectedId(returnOrder.id);
    navigate(`${INBOUND.RETURNS}/${returnOrder.id}`);
  };

  const handleCreateRMA = () => {
    navigate(INBOUND.RETURNS_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('returns.title', 'Returns')}</h1>
          <p className="page__subtitle">{t('returns.subtitle', 'Manage return orders and RMA processing')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateRMA}>
            {t('returns.createRMA', 'Create RMA')}
          </button>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('returns.searchRMA', 'Search by RMA #...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as ReturnOrderStatus | '')}
          >
            <option value="">{t('returns.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="InTransit">In Transit</option>
            <option value="Received">Received</option>
            <option value="InProgress">In Progress</option>
            <option value="Complete">Complete</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={returns}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedId}
          getRowId={(row) => row.id}
          emptyMessage={t('returns.noReturns', 'No return orders found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Returns;
