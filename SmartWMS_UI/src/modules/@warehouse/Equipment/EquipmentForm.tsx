import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import type { EquipmentDto, EquipmentType, EquipmentStatus } from '@/api/modules/equipment';
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

const EQUIPMENT_STATUSES: EquipmentStatus[] = [
  'Available', 'InUse', 'Maintenance', 'OutOfService', 'Reserved',
];

export interface EquipmentFormData {
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

interface EquipmentFormProps {
  initialData?: EquipmentDto;
  onSubmit: (data: EquipmentFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

export function EquipmentForm({ initialData, onSubmit, loading, isEditMode }: EquipmentFormProps) {
  const t = useTranslate();

  const { data: warehousesData } = useGetWarehouseOptionsQuery();
  const warehouses = warehousesData?.data || [];

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
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

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        code: initialData.code,
        name: initialData.name,
        description: initialData.description || '',
        type: initialData.type,
        status: initialData.status,
        warehouseId: initialData.warehouseId || '',
        serialNumber: initialData.serialNumber || '',
        manufacturer: initialData.manufacturer || '',
        model: initialData.model || '',
        purchaseDate: initialData.purchaseDate?.split('T')[0] || '',
        warrantyExpiryDate: initialData.warrantyExpiryDate?.split('T')[0] || '',
        nextMaintenanceDate: initialData.nextMaintenanceDate?.split('T')[0] || '',
        isActive: initialData.isActive,
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="equipment-form" onSubmit={handleFormSubmit}>
      {/* Section: Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('equipment.basicInfo', 'Basic Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('equipment.code', 'Code')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                placeholder="EQ-001"
                disabled={isEditMode}
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
              <label className="form-field__label">{t('common.status', 'Status')}</label>
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
        </div>
      </section>

      {/* Section: Technical Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('equipment.technicalDetails', 'Technical Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
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
        </div>
      </section>

      {/* Form Actions */}
      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || (!isDirty && isEditMode)}
        >
          {loading
            ? t('common.saving', 'Saving...')
            : isEditMode
              ? t('common.save', 'Save Changes')
              : t('equipment.addEquipment', 'Add Equipment')}
        </button>
      </div>
    </form>
  );
}
