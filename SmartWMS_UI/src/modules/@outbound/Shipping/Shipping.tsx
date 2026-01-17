import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { DataTable, createColumns } from '@/components/DataTable';
import { useTranslate } from '@/hooks';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetShipmentsQuery } from '@/api/modules/fulfillment';
import type { ShipmentDto, ShipmentStatus } from '@/api/modules/fulfillment';
import { OUTBOUND } from '@/constants/routes';
import './Shipping.scss';

const STATUS_COLORS: Record<ShipmentStatus, string> = {
  Pending: 'warning',
  ReadyToShip: 'info',
  Shipped: 'info',
  Delivered: 'success',
  Cancelled: 'error',
};

/**
 * Shipping Module
 * Manages outbound shipments and carrier integration.
 */
export function Shipping() {
  const t = useTranslate();
  const navigate = useNavigate();

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

  const shipments = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<ShipmentDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('shipmentNumber', {
        header: t('shipment.shipmentNumber', 'Shipment #'),
        size: 140,
        cell: ({ getValue }) => <strong>{getValue()}</strong>,
      }),
      columnHelper.accessor('salesOrderNumber', {
        header: t('shipment.orderNumber', 'Order'),
        size: 120,
        cell: ({ getValue }) => <code className="code">{getValue() || '-'}</code>,
      }),
      columnHelper.accessor('customerName', {
        header: t('common.customer', 'Customer'),
        size: 150,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('carrierName', {
        header: t('shipment.carrier', 'Carrier'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('carrierServiceName', {
        header: t('shipment.service', 'Service'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('trackingNumber', {
        header: t('shipment.tracking', 'Tracking'),
        size: 160,
        cell: ({ getValue }) => {
          const value = getValue();
          if (!value) return <span className="text-muted">-</span>;
          return <code className="code">{value}</code>;
        },
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 110,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${STATUS_COLORS[status]}`}>
              {status}
            </span>
          );
        },
      }),
      columnHelper.accessor('packageCount', {
        header: t('shipment.packages', 'Packages'),
        size: 80,
      }),
      columnHelper.accessor('totalWeight', {
        header: t('shipment.weight', 'Weight'),
        size: 80,
        cell: ({ getValue, row }) => {
          const weight = getValue();
          const unit = row.original.weightUnit || 'kg';
          return weight ? `${weight} ${unit}` : '-';
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (shipment: ShipmentDto) => {
    setSelectedId(shipment.id);
    navigate(`${OUTBOUND.SHIPPING}/${shipment.id}`);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('shipment.title', 'Shipments')}</h1>
          <p className="page__subtitle">{t('shipment.subtitle', 'Manage outbound shipments')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--secondary">
            {t('common.export', 'Export')}
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
            onChange={(e) => setStatusFilter(e.target.value as ShipmentStatus | '')}
          >
            <option value="">{t('shipment.allStatuses', 'All Statuses')}</option>
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
          data={shipments}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('shipment.noShipments', 'No shipments found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Shipping;
