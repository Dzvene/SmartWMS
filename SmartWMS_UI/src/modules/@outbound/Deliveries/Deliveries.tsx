import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetShipmentsQuery } from '@/api/modules/fulfillment';
import type { ShipmentDto, ShipmentStatus } from '@/api/modules/fulfillment';

const STATUS_COLORS: Record<ShipmentStatus, string> = {
  Pending: 'warning',
  ReadyToShip: 'info',
  Shipped: 'info',
  Delivered: 'success',
  Cancelled: 'error',
};

const STATUS_LABELS: Record<ShipmentStatus, string> = {
  Pending: 'Pending',
  ReadyToShip: 'Ready to Ship',
  Shipped: 'Shipped',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
};

/**
 * Deliveries Module
 * Tracks shipments and delivery status.
 */
export function Deliveries() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<ShipmentStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetShipmentsQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });

  const deliveries = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<ShipmentDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('shipmentNumber', {
        header: t('deliveries.deliveryNumber', 'Delivery #'),
        size: 120,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('salesOrderNumber', {
        header: t('deliveries.orderNumber', 'Order #'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('customerName', {
        header: t('common.customer', 'Customer'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('carrierName', {
        header: t('deliveries.carrier', 'Carrier'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('trackingNumber', {
        header: t('deliveries.trackingNumber', 'Tracking #'),
        size: 140,
        cell: ({ row, getValue }) => {
          const tracking = getValue();
          const url = row.original.trackingUrl;
          if (!tracking) return '-';
          return url ? (
            <a href={url} target="_blank" rel="noopener noreferrer" className="link">
              {tracking}
            </a>
          ) : (
            <span className="code">{tracking}</span>
          );
        },
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 110,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${STATUS_COLORS[status]}`}>
              {STATUS_LABELS[status]}
            </span>
          );
        },
      }),
      columnHelper.accessor('packageCount', {
        header: t('deliveries.packages', 'Packages'),
        size: 80,
      }),
      columnHelper.accessor('shippedAt', {
        header: t('deliveries.shippedDate', 'Shipped'),
        size: 100,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('estimatedDeliveryDate', {
        header: t('deliveries.estimatedDelivery', 'Est. Delivery'),
        size: 110,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('deliveredAt', {
        header: t('deliveries.deliveredDate', 'Delivered'),
        size: 100,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (delivery: ShipmentDto) => {
    setSelectedId(delivery.id);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('deliveries.title', 'Deliveries')}</h1>
          <p className="page__subtitle">{t('deliveries.subtitle', 'Track shipments and delivery status')}</p>
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
            onChange={(e) => setStatusFilter(e.target.value as ShipmentStatus | '')}
          >
            <option value="">{t('deliveries.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="ReadyToShip">Ready to Ship</option>
            <option value="Shipped">Shipped</option>
            <option value="Delivered">Delivered</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={deliveries}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedId}
          getRowId={(row) => row.id}
          emptyMessage={t('deliveries.noDeliveries', 'No deliveries found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Deliveries;
