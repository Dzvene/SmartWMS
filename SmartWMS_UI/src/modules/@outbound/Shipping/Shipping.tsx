import { useState, useMemo } from 'react';
import { DataTable, createColumns } from '@/components/DataTable';
import { useTranslate } from '@/hooks';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetShipmentsQuery } from '@/api/modules/fulfillment';
import type { ShipmentDto, ShipmentStatus } from '@/api/modules/fulfillment';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
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

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<ShipmentStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedShipment, setSelectedShipment] = useState<ShipmentDto | null>(null);

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
    setSelectedShipment(shipment);
    setModalOpen(true);
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

      <FullscreenModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={t('shipment.details', 'Shipment Details')}
        subtitle={selectedShipment?.shipmentNumber}
        maxWidth="md"
      >
        {selectedShipment && (
          <>
            <ModalSection title={t('shipment.information', 'Shipment Information')}>
              <div className="shipment-details">
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('shipment.shipmentNumber', 'Shipment #')}</span>
                  <span className="shipment-details__value">{selectedShipment.shipmentNumber}</span>
                </div>
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('shipment.orderNumber', 'Order #')}</span>
                  <span className="shipment-details__value">{selectedShipment.salesOrderNumber || '-'}</span>
                </div>
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('common.status', 'Status')}</span>
                  <span className="shipment-details__value">{selectedShipment.status}</span>
                </div>
              </div>
            </ModalSection>

            <ModalSection title={t('shipment.carrierDetails', 'Carrier Details')}>
              <div className="shipment-details">
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('shipment.carrier', 'Carrier')}</span>
                  <span className="shipment-details__value">{selectedShipment.carrierName || '-'}</span>
                </div>
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('shipment.service', 'Service')}</span>
                  <span className="shipment-details__value">{selectedShipment.carrierServiceName || '-'}</span>
                </div>
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('shipment.tracking', 'Tracking #')}</span>
                  <span className="shipment-details__value">
                    {selectedShipment.trackingNumber || '-'}
                  </span>
                </div>
              </div>
            </ModalSection>

            <ModalSection title={t('shipment.packageDetails', 'Package Details')}>
              <div className="shipment-details">
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('shipment.packages', 'Packages')}</span>
                  <span className="shipment-details__value">{selectedShipment.packageCount}</span>
                </div>
                <div className="shipment-details__row">
                  <span className="shipment-details__label">{t('shipment.totalWeight', 'Total Weight')}</span>
                  <span className="shipment-details__value">
                    {selectedShipment.totalWeight} {selectedShipment.weightUnit || 'kg'}
                  </span>
                </div>
              </div>
            </ModalSection>
          </>
        )}
      </FullscreenModal>
    </div>
  );
}

export default Shipping;
