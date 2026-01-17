import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetEquipmentQuery } from '@/api/modules/equipment';
import type { EquipmentDto, EquipmentType, EquipmentStatus } from '@/api/modules/equipment';
import { WAREHOUSE } from '@/constants/routes';

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
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<EquipmentType | ''>('');
  const [statusFilter, setStatusFilter] = useState<EquipmentStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetEquipmentQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    type: typeFilter || undefined,
    status: statusFilter || undefined,
  });

  const equipment = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<EquipmentDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('code', {
        header: t('equipment.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="equipment__code">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('name', {
        header: t('common.name', 'Name'),
        size: 150,
        cell: ({ getValue }) => (
          <span className="equipment__name">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('type', {
        header: t('common.type', 'Type'),
        size: 120,
        cell: ({ getValue }) => TYPE_LABELS[getValue()] || getValue(),
      }),
      columnHelper.accessor('warehouseName', {
        header: t('equipment.warehouse', 'Warehouse'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('assignedToUserName', {
        header: t('equipment.assignedTo', 'Assigned To'),
        size: 130,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('nextMaintenanceDate', {
        header: t('equipment.nextMaintenance', 'Next Maintenance'),
        size: 140,
        cell: ({ getValue }) => {
          const date = getValue();
          if (!date) return '-';
          const d = new Date(date);
          const isOverdue = d < new Date();
          return (
            <span className={isOverdue ? 'text-error' : ''}>
              {d.toLocaleDateString()}
            </span>
          );
        },
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 110,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${STATUS_CLASSES[status]}`}>
              {STATUS_LABELS[status] || status}
            </span>
          );
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (item: EquipmentDto) => {
    setSelectedId(item.id);
    navigate(`${WAREHOUSE.EQUIPMENT}/${item.id}`);
  };

  const handleCreateEquipment = () => {
    navigate(WAREHOUSE.EQUIPMENT_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('equipment.title', 'Equipment')}</h1>
          <p className="page__subtitle">
            {t('equipment.subtitle', 'Manage warehouse equipment and devices')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateEquipment}>
            {t('equipment.addEquipment', 'Add Equipment')}
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
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value as EquipmentType | '')}
          >
            <option value="">{t('equipment.allTypes', 'All Types')}</option>
            {Object.entries(TYPE_LABELS).map(([key, label]) => (
              <option key={key} value={key}>
                {label}
              </option>
            ))}
          </select>
          <select
            className="page-filter__select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as EquipmentStatus | '')}
          >
            <option value="">{t('equipment.allStatuses', 'All Statuses')}</option>
            {Object.entries(STATUS_LABELS).map(([key, label]) => (
              <option key={key} value={key}>
                {label}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={equipment}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('equipment.noEquipment', 'No equipment found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Equipment;
