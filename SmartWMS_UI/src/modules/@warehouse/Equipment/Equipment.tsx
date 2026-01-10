import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetEquipmentQuery,
  useCreateEquipmentMutation,
  useUpdateEquipmentMutation,
  useDeleteEquipmentMutation,
} from '@/api/modules/equipment';
import type {
  EquipmentDto,
  EquipmentType,
  EquipmentStatus,
  CreateEquipmentRequest,
  UpdateEquipmentRequest,
} from '@/api/modules/equipment';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';

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

const EQUIPMENT_TYPES: EquipmentType[] = [
  'Forklift', 'ReachTruck', 'OrderPicker', 'PalletJack', 'HandScanner',
  'RFGun', 'Printer', 'Conveyor', 'Sorter', 'ASRS', 'AGV', 'Dock', 'Scale', 'Other',
];

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

const EQUIPMENT_STATUSES: EquipmentStatus[] = [
  'Available', 'InUse', 'Maintenance', 'OutOfService', 'Reserved',
];

interface EquipmentFormData {
  code: string;
  name: string;
  description: string;
  type: EquipmentType;
  status: EquipmentStatus;
  warehouseId: string;
  serialNumber: string;
  manufacturer: string;
  model: string;
  purchaseDate: string;
  warrantyExpiryDate: string;
  nextMaintenanceDate: string;
  isActive: boolean;
}

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
  const { data: warehousesData } = useGetWarehouseOptionsQuery();

  const equipment = response?.data?.items || [];
  const totalCount = response?.data?.totalCount || 0;
  const warehouses = warehousesData?.data || [];

  const [selectedItem, setSelectedItem] = useState<EquipmentDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // API Mutations
  const [createEquipment, { isLoading: isCreating }] = useCreateEquipmentMutation();
  const [updateEquipment, { isLoading: isUpdating }] = useUpdateEquipmentMutation();
  const [deleteEquipment, { isLoading: isDeleting }] = useDeleteEquipmentMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<EquipmentFormData>({
    defaultValues: {
      code: '',
      name: '',
      description: '',
      type: 'Forklift',
      status: 'Available',
      warehouseId: '',
      serialNumber: '',
      manufacturer: '',
      model: '',
      purchaseDate: '',
      warrantyExpiryDate: '',
      nextMaintenanceDate: '',
      isActive: true,
    },
  });

  // Reset form when editing changes
  useEffect(() => {
    if (selectedItem) {
      reset({
        code: selectedItem.code,
        name: selectedItem.name,
        description: selectedItem.description || '',
        type: selectedItem.type,
        status: selectedItem.status,
        warehouseId: selectedItem.warehouseId || '',
        serialNumber: selectedItem.serialNumber || '',
        manufacturer: selectedItem.manufacturer || '',
        model: selectedItem.model || '',
        purchaseDate: selectedItem.purchaseDate?.split('T')[0] || '',
        warrantyExpiryDate: selectedItem.warrantyExpiryDate?.split('T')[0] || '',
        nextMaintenanceDate: selectedItem.nextMaintenanceDate?.split('T')[0] || '',
        isActive: selectedItem.isActive,
      });
    } else {
      reset({
        code: '',
        name: '',
        description: '',
        type: 'Forklift',
        status: 'Available',
        warehouseId: '',
        serialNumber: '',
        manufacturer: '',
        model: '',
        purchaseDate: '',
        warrantyExpiryDate: '',
        nextMaintenanceDate: '',
        isActive: true,
      });
    }
  }, [selectedItem, reset]);

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

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedItem(null);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, search: value, page: 1 }));
  };

  const onSubmit = async (data: EquipmentFormData) => {
    try {
      if (selectedItem) {
        const updateData: UpdateEquipmentRequest = {
          name: data.name,
          description: data.description || undefined,
          type: data.type,
          status: data.status,
          warehouseId: data.warehouseId || undefined,
          serialNumber: data.serialNumber || undefined,
          manufacturer: data.manufacturer || undefined,
          model: data.model || undefined,
          purchaseDate: data.purchaseDate || undefined,
          warrantyExpiryDate: data.warrantyExpiryDate || undefined,
          nextMaintenanceDate: data.nextMaintenanceDate || undefined,
          isActive: data.isActive,
        };
        await updateEquipment({ id: selectedItem.id, body: updateData }).unwrap();
      } else {
        const createData: CreateEquipmentRequest = {
          code: data.code,
          name: data.name,
          description: data.description || undefined,
          type: data.type,
          status: data.status,
          warehouseId: data.warehouseId || undefined,
          serialNumber: data.serialNumber || undefined,
          manufacturer: data.manufacturer || undefined,
          model: data.model || undefined,
          purchaseDate: data.purchaseDate || undefined,
          warrantyExpiryDate: data.warrantyExpiryDate || undefined,
          nextMaintenanceDate: data.nextMaintenanceDate || undefined,
          isActive: data.isActive,
        };
        await createEquipment(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save equipment:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedItem) return;

    try {
      await deleteEquipment(selectedItem.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete equipment:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('equipment.title', 'Equipment')}</h1>
          <p className="page__subtitle">{t('equipment.subtitle', 'Manage warehouse equipment and devices')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
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
            value={filters.search}
            onChange={(e) => handleSearch(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <select
            className="page-filter__select"
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
            className="page-filter__select"
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

      {/* Equipment Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={selectedItem ? t('equipment.editEquipment', 'Edit Equipment') : t('equipment.addEquipment', 'Add Equipment')}
        subtitle={selectedItem ? `${selectedItem.code} - ${selectedItem.name}` : undefined}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('equipment.basicInfo', 'Basic Information')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('equipment.code', 'Code')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                  placeholder="EQ-001"
                  disabled={!!selectedItem}
                  {...register('code', { required: t('validation.required', 'Required') })}
                />
                {errors.code && <span className="form-field__error">{errors.code.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.name', 'Name')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                  placeholder="Forklift #1"
                  {...register('name', { required: t('validation.required', 'Required') })}
                />
                {errors.name && <span className="form-field__error">{errors.name.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('common.type', 'Type')}</label>
                <select className="form-field__select" {...register('type')}>
                  {EQUIPMENT_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {TYPE_LABELS[type]}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('common.status', 'Status')}</label>
                <select className="form-field__select" {...register('status')}>
                  {EQUIPMENT_STATUSES.map((status) => (
                    <option key={status} value={status}>
                      {STATUS_LABELS[status]}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('common.warehouse', 'Warehouse')}</label>
                <select className="form-field__select" {...register('warehouseId')}>
                  <option value="">{t('common.selectWarehouse', 'Select warehouse...')}</option>
                  {warehouses.map((wh) => (
                    <option key={wh.id} value={wh.id}>
                      {wh.code} - {wh.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-checkbox">
                  <input type="checkbox" {...register('isActive')} />
                  <span>{t('common.active', 'Active')}</span>
                </label>
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('common.description', 'Description')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={2}
                  placeholder={t('equipment.descriptionPlaceholder', 'Equipment description...')}
                  {...register('description')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('equipment.technicalDetails', 'Technical Details')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('equipment.serialNumber', 'Serial Number')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="SN-12345"
                  {...register('serialNumber')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('equipment.manufacturer', 'Manufacturer')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="Toyota"
                  {...register('manufacturer')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('equipment.model', 'Model')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="8FBE15"
                  {...register('model')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('equipment.purchaseDate', 'Purchase Date')}</label>
                <input
                  type="date"
                  className="form-field__input"
                  {...register('purchaseDate')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('equipment.warrantyExpiryDate', 'Warranty Expiry')}</label>
                <input
                  type="date"
                  className="form-field__input"
                  {...register('warrantyExpiryDate')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('equipment.nextMaintenance', 'Next Maintenance')}</label>
                <input
                  type="date"
                  className="form-field__input"
                  {...register('nextMaintenanceDate')}
                />
              </div>
            </div>
          </ModalSection>

          {selectedItem && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('equipment.deleteWarning', 'Deleting equipment will remove all associated records.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('equipment.deleteEquipment', 'Delete Equipment')}
                </button>
              </div>
            </ModalSection>
          )}
        </form>
      </FullscreenModal>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('equipment.deleteEquipment', 'Delete Equipment')}
      >
        <div className="modal__body">
          <p>
            {t(
              'equipment.deleteConfirmation',
              `Are you sure you want to delete "${selectedItem?.name}"? This action cannot be undone.`
            )}
          </p>
        </div>
        <div className="modal__actions">
          <button className="btn btn-ghost" onClick={() => setDeleteConfirmOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button className="btn btn--danger" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? t('common.deleting', 'Deleting...') : t('common.delete', 'Delete')}
          </button>
        </div>
      </Modal>
    </div>
  );
}

export default Equipment;
