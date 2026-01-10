import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetSalesOrdersQuery } from '@/api/modules/orders';
import type { SalesOrderDto, SalesOrderStatus, OrderPriority } from '@/api/modules/orders';
import { ORDERS } from '@/constants/routes';
import './SalesOrders.scss';

const STATUS_COLORS: Record<SalesOrderStatus, string> = {
  Draft: 'neutral',
  Pending: 'warning',
  Confirmed: 'info',
  Allocated: 'info',
  PartiallyAllocated: 'warning',
  Picking: 'info',
  PartiallyPicked: 'warning',
  Picked: 'success',
  Packing: 'info',
  Packed: 'success',
  Shipped: 'success',
  PartiallyShipped: 'warning',
  Delivered: 'success',
  Cancelled: 'error',
  OnHold: 'warning',
};

const PRIORITY_COLORS: Record<OrderPriority, string> = {
  Low: 'neutral',
  Normal: 'info',
  High: 'warning',
  Urgent: 'error',
};

export function SalesOrders() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<SalesOrderStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetSalesOrdersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const orders = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<SalesOrderDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('orderNumber', {
        header: t('salesOrders.orderNumber', 'Order #'),
        size: 120,
        cell: ({ getValue }) => (
          <span className="sales-orders__order-number">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('customerName', {
        header: t('salesOrders.customer', 'Customer'),
        size: 180,
        cell: ({ getValue, row }) => (
          <div className="sales-orders__customer">
            <span className="sales-orders__customer-name">{getValue() || '-'}</span>
            {row.original.customerCode && (
              <span className="sales-orders__customer-code">{row.original.customerCode}</span>
            )}
          </div>
        ),
      }),
      columnHelper.accessor('status', {
        header: t('salesOrders.status', 'Status'),
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
      columnHelper.accessor('priority', {
        header: t('salesOrders.priority', 'Priority'),
        size: 100,
        cell: ({ getValue }) => {
          const priority = getValue();
          return (
            <span className={`priority-badge priority-badge--${PRIORITY_COLORS[priority]}`}>
              {priority}
            </span>
          );
        },
      }),
      columnHelper.accessor('orderDate', {
        header: t('salesOrders.orderDate', 'Order Date'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
      columnHelper.accessor('requiredDate', {
        header: t('salesOrders.requiredDate', 'Required'),
        size: 110,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('totalQuantity', {
        header: t('salesOrders.qty', 'Qty'),
        size: 80,
        cell: ({ getValue, row }) => (
          <div className="sales-orders__quantities">
            <span>{getValue()}</span>
            {row.original.shippedQuantity > 0 && (
              <span className="sales-orders__quantities-shipped">
                ({row.original.shippedQuantity} shipped)
              </span>
            )}
          </div>
        ),
      }),
      columnHelper.accessor('warehouseName', {
        header: t('salesOrders.warehouse', 'Warehouse'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (order: SalesOrderDto) => {
    setSelectedId(order.id);
    navigate(`${ORDERS.SALES_ORDERS}/${order.id}`);
  };

  const handleCreateOrder = () => {
    navigate(ORDERS.SALES_ORDER_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('salesOrders.title', 'Sales Orders')}</h1>
          <p className="page__subtitle">
            {t('salesOrders.subtitle', 'Manage customer orders and fulfillment')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateOrder}>
            {t('salesOrders.createOrder', 'Create Order')}
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
            onChange={(e) => setStatusFilter(e.target.value as SalesOrderStatus | '')}
          >
            <option value="">{t('salesOrders.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="Confirmed">Confirmed</option>
            <option value="Allocated">Allocated</option>
            <option value="Picking">Picking</option>
            <option value="Picked">Picked</option>
            <option value="Packing">Packing</option>
            <option value="Shipped">Shipped</option>
            <option value="Delivered">Delivered</option>
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
          emptyMessage={t('salesOrders.noOrders', 'No sales orders found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default SalesOrders;
