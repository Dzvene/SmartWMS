import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal } from '@/components/FullscreenModal';
import { useGetEquipmentQuery } from '@/api/modules/equipment';
import type { EquipmentDto, EquipmentType, EquipmentStatus } from '@/api/modules/equipment';

const TYPE_LABELS: Record<EquipmentType, string> = {
  Forklift: 'Forklift',
  ReachTruck: 'Reach Truck',
  OrderPicker: 'Order Picker',
  PalletJack: 'Pallet Jack',
  HandScanner: 'Hand Scanner',
  RFGun: 'RF Gun',
  Printer: 'Printer',
  Conveyor: 'Conveyor',
  Sorter: 'Sorter',
  ASRS: 'AS/RS',
  AGV: 'AGV',
  Dock: 'Dock',
  Scale: 'Scale',
  Other: 'Other',
};

const STATUS_LABELS: Record<EquipmentStatus, string> = {
  Available: 'Available',
  InUse: 'In Use',
  Maintenance: 'Maintenance',
  OutOfService: 'Out of Service',
  Reserved: 'Reserved',
};

const STATUS_CLASSES: Record<EquipmentStatus, string> = {
  Available: 'success',
  InUse: 'info',
  Maintenance: 'warning',
  OutOfService: 'error',
  Reserved: 'neutral',
};

export function Equipment() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 20,
    search: '',
    type: undefined as EquipmentType | undefined,
    status: undefined as EquipmentStatus | undefined,
  });

  const { data: response, isLoading } = useGetEquipmentQuery(filters);
  const equipment = response?.data?.items || [];
  const totalCount = response?.data?.totalCount || 0;

  const [selectedItem, setSelectedItem] = useState<EquipmentDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const columns = useMemo<ColumnDef<EquipmentDto, unknown>[]>(
    () => [
      {
        accessorKey: 'code',
        header: t('equipment.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue() as string}</span>,
      },
      {
        accessorKey: 'name',
        header: t('common.name', 'Name'),
        size: 150,
      },
      {
        accessorKey: 'type',
        header: t('common.type', 'Type'),
        size: 120,
        cell: ({ getValue }) => TYPE_LABELS[getValue() as EquipmentType] || getValue(),
      },
      {
        accessorKey: 'warehouseName',
        header: t('equipment.warehouse', 'Warehouse'),
        size: 140,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'assignedToUserName',
        header: t('equipment.assignedTo', 'Assigned To'),
        size: 130,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'nextMaintenanceDate',
        header: t('equipment.nextMaintenance', 'Next Maintenance'),
        size: 140,
        cell: ({ getValue }) => {
          const date = getValue() as string | undefined;
          if (!date) return '—';
          const d = new Date(date);
          const isOverdue = d < new Date();
          return (
            <span className={isOverdue ? 'text-error' : ''}>
              {d.toLocaleDateString()}
            </span>
          );
        },
      },
      {
        accessorKey: 'status',
        header: t('common.status', 'Status'),
        size: 110,
        cell: ({ getValue }) => {
          const status = getValue() as EquipmentStatus;
          return (
            <span className={`status-badge status-badge--${STATUS_CLASSES[status]}`}>
              {STATUS_LABELS[status] || status}
            </span>
          );
        },
      },
    ],
    [t]
  );

  const handleRowClick = (item: EquipmentDto) => {
    setSelectedItem(item);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedItem(null);
    setIsModalOpen(true);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, search: value, page: 1 }));
  };

  return (
    <div className="page">
      <div className="page__header">
        <div>
          <h1 className="page__title">{t('nav.warehouse.equipment', 'Equipment')}</h1>
          <p className="page__subtitle">{t('equipment.subtitle', 'Manage warehouse equipment and devices')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('equipment.addEquipment', 'Add Equipment')}
          </button>
        </div>
      </div>

      <div className="page__filters">
        <input
          type="text"
          className="input"
          placeholder={t('common.search', 'Search...')}
          value={filters.search}
          onChange={(e) => handleSearch(e.target.value)}
        />
        <select
          className="select"
          value={filters.type || ''}
          onChange={(e) =>
            setFilters((prev) => ({
              ...prev,
              type: (e.target.value || undefined) as EquipmentType | undefined,
              page: 1,
            }))
          }
        >
          <option value="">{t('equipment.allTypes', 'All Types')}</option>
          {Object.entries(TYPE_LABELS).map(([key, label]) => (
            <option key={key} value={key}>
              {label}
            </option>
          ))}
        </select>
        <select
          className="select"
          value={filters.status || ''}
          onChange={(e) =>
            setFilters((prev) => ({
              ...prev,
              status: (e.target.value || undefined) as EquipmentStatus | undefined,
              page: 1,
            }))
          }
        >
          <option value="">{t('equipment.allStatuses', 'All Statuses')}</option>
          {Object.entries(STATUS_LABELS).map(([key, label]) => (
            <option key={key} value={key}>
              {label}
            </option>
          ))}
        </select>
      </div>

      <div className="page__content">
        <DataTable
          data={equipment}
          columns={columns}
          pagination={{
            pageIndex: filters.page - 1,
            pageSize: filters.pageSize,
          }}
          onPaginationChange={({ pageIndex }) => handlePageChange(pageIndex + 1)}
          totalRows={totalCount}
          onRowClick={handleRowClick}
          selectedRowId={selectedItem?.id}
          getRowId={(row) => row.id}
          emptyMessage={t('equipment.noEquipment', 'No equipment found')}
          loading={isLoading}
        />
      </div>

      <FullscreenModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedItem ? `Edit ${selectedItem.name}` : t('equipment.addEquipment', 'Add Equipment')}
      >
        <div className="form">
          {selectedItem ? (
            <div className="equipment-details">
              <div className="form-group">
                <label>{t('equipment.code', 'Code')}</label>
                <p>{selectedItem.code}</p>
              </div>
              <div className="form-group">
                <label>{t('common.name', 'Name')}</label>
                <p>{selectedItem.name}</p>
              </div>
              <div className="form-group">
                <label>{t('common.type', 'Type')}</label>
                <p>{TYPE_LABELS[selectedItem.type]}</p>
              </div>
              <div className="form-group">
                <label>{t('common.status', 'Status')}</label>
                <p>{STATUS_LABELS[selectedItem.status]}</p>
              </div>
              {selectedItem.warehouseName && (
                <div className="form-group">
                  <label>{t('equipment.warehouse', 'Warehouse')}</label>
                  <p>{selectedItem.warehouseName}</p>
                </div>
              )}
              {selectedItem.assignedToUserName && (
                <div className="form-group">
                  <label>{t('equipment.assignedTo', 'Assigned To')}</label>
                  <p>{selectedItem.assignedToUserName}</p>
                </div>
              )}
              {selectedItem.serialNumber && (
                <div className="form-group">
                  <label>{t('equipment.serialNumber', 'Serial Number')}</label>
                  <p>{selectedItem.serialNumber}</p>
                </div>
              )}
              {selectedItem.manufacturer && (
                <div className="form-group">
                  <label>{t('equipment.manufacturer', 'Manufacturer')}</label>
                  <p>{selectedItem.manufacturer}</p>
                </div>
              )}
              {selectedItem.model && (
                <div className="form-group">
                  <label>{t('equipment.model', 'Model')}</label>
                  <p>{selectedItem.model}</p>
                </div>
              )}
              {selectedItem.description && (
                <div className="form-group">
                  <label>{t('common.description', 'Description')}</label>
                  <p>{selectedItem.description}</p>
                </div>
              )}
            </div>
          ) : (
            <p>{t('equipment.formPlaceholder', 'Equipment creation form will be implemented here.')}</p>
          )}
        </div>
      </FullscreenModal>
    </div>
  );
}

export default Equipment;
