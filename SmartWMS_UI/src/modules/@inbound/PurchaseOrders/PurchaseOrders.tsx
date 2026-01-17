import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetPurchaseOrdersQuery } from '@/api/modules/orders';
import type { PurchaseOrderDto, PurchaseOrderStatus } from '@/api/modules/orders';
import { ORDERS } from '@/constants/routes';
import './PurchaseOrders.scss';

const STATUS_COLORS: Record<PurchaseOrderStatus, string> = {
  Draft: 'neutral',
  Pending: 'warning',
  Confirmed: 'info',
  PartiallyReceived: 'warning',
  Received: 'success',
  Completed: 'success',
  Cancelled: 'error',
  OnHold: 'warning',
};

/**
 * Purchase Orders Module (Inbound)
 *
 * Displays purchase orders focused on inbound receiving operations.
 * Creates/edits redirect to /orders module for full order management.
 */
export function PurchaseOrders() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<PurchaseOrderStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetPurchaseOrdersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const orders = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<PurchaseOrderDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('orderNumber', {
        header: t('purchaseOrders.orderNumber', 'PO #'),
        size: 120,
        cell: ({ getValue }) => (
          <span className="purchase-orders__order-number">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('supplierName', {
        header: t('purchaseOrders.supplier', 'Supplier'),
        size: 180,
        cell: ({ getValue, row }) => (
          <div className="purchase-orders__supplier">
            <span className="purchase-orders__supplier-name">{getValue() || '-'}</span>
            {row.original.supplierCode && (
              <span className="purchase-orders__supplier-code">{row.original.supplierCode}</span>
            )}
          </div>
        ),
      }),
      columnHelper.accessor('status', {
        header: t('purchaseOrders.status', 'Status'),
        size: 130,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${STATUS_COLORS[status]}`}>
              {status}
            </span>
          );
        },
      }),
      columnHelper.accessor('orderDate', {
        header: t('purchaseOrders.orderDate', 'Order Date'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
      columnHelper.accessor('expectedDate', {
        header: t('purchaseOrders.expectedDate', 'Expected'),
        size: 110,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('totalQuantity', {
        header: t('purchaseOrders.qty', 'Qty'),
        size: 80,
        cell: ({ getValue, row }) => (
          <div className="purchase-orders__quantities">
            <span>{getValue()}</span>
            {row.original.receivedQuantity > 0 && (
              <span className="purchase-orders__quantities-received">
                ({row.original.receivedQuantity} received)
              </span>
            )}
          </div>
        ),
      }),
      columnHelper.accessor('warehouseName', {
        header: t('purchaseOrders.warehouse', 'Warehouse'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('totalLines', {
        header: t('purchaseOrders.lines', 'Lines'),
        size: 70,
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (order: PurchaseOrderDto) => {
    setSelectedId(order.id);
    // Navigate to orders module for details/editing
    navigate(`${ORDERS.PURCHASE_ORDERS}/${order.id}`);
  };

  const handleCreateOrder = () => {
    // Navigate to orders module for creation
    navigate(ORDERS.PURCHASE_ORDER_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('purchaseOrders.title', 'Purchase Orders')}</h1>
          <p className="page__subtitle">
            {t('purchaseOrders.inboundSubtitle', 'Manage supplier orders and receiving operations')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateOrder}>
            {t('purchaseOrders.createOrder', 'Create PO')}
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
            onChange={(e) => setStatusFilter(e.target.value as PurchaseOrderStatus | '')}
          >
            <option value="">{t('purchaseOrders.allStatuses', 'All Statuses')}</option>
            <option value="Draft">Draft</option>
            <option value="Pending">Pending</option>
            <option value="Confirmed">Confirmed</option>
            <option value="PartiallyReceived">Partially Received</option>
            <option value="Received">Received</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={orders}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('purchaseOrders.noOrders', 'No purchase orders found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default PurchaseOrders;
