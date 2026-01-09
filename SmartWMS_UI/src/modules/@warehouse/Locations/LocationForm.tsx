import { useEffect, useRef } from 'react';
import { useIntl } from 'react-intl';
import { useForm, Controller } from 'react-hook-form';
import type { LocationType } from '@/api/modules/locations';

export interface LocationFormData {
  code: string;
  name: string;
  warehouseId: string;
  zoneId: string;
  aisle: string;
  rack: string;
  level: string;
  position: string;
  locationType: LocationType;
  widthMm: number | null;
  heightMm: number | null;
  depthMm: number | null;
  maxWeight: number | null;
  maxVolume: number | null;
  isActive: boolean;
  isPickLocation: boolean;
  isPutawayLocation: boolean;
  isReceivingDock: boolean;
  isShippingDock: boolean;
  pickSequence: number | null;
  putawaySequence: number | null;
}

interface WarehouseOption {
  id: string;
  name: string;
}

interface ZoneOption {
  id: string;
  name: string;
  warehouseId: string;
}

interface LocationFormProps {
  initialData?: Partial<LocationFormData>;
  warehouses: WarehouseOption[];
  zones: ZoneOption[];
  onSubmit: (data: LocationFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const defaultValues: LocationFormData = {
  code: '',
  name: '',
  warehouseId: '',
  zoneId: '',
  aisle: '',
  rack: '',
  level: '',
  position: '',
  locationType: 'Bulk',
  widthMm: null,
  heightMm: null,
  depthMm: null,
  maxWeight: null,
  maxVolume: null,
  isActive: true,
  isPickLocation: false,
  isPutawayLocation: true,
  isReceivingDock: false,
  isShippingDock: false,
  pickSequence: null,
  putawaySequence: null,
};

const locationTypes: LocationType[] = [
  'Bulk',
  'Pick',
  'Staging',
  'Receiving',
  'Shipping',
  'Returns',
  'Quarantine',
  'Reserve',
];

/**
 * LocationForm - Pure form component
 *
 * Receives initialData, warehouses, and zones from parent container.
 * Knows nothing about data fetching or API calls.
 */
export function LocationForm({
  initialData,
  warehouses,
  zones,
  onSubmit,
  loading,
  isEditMode,
}: LocationFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const {
    register,
    handleSubmit,
    control,
    reset,
    watch,
    formState: { errors, isDirty },
  } = useForm<LocationFormData>({
    defaultValues,
  });

  const selectedWarehouseId = watch('warehouseId');

  // Filter zones by selected warehouse - show empty list if no warehouse selected
  const filteredZones = selectedWarehouseId
    ? zones.filter((z) => z.warehouseId === selectedWarehouseId)
    : [];

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({ ...defaultValues, ...initialData });
    }
  }, [initialData, reset]);

  // Reset zone when warehouse changes (only in create mode)
  const prevWarehouseId = useRef(selectedWarehouseId);
  useEffect(() => {
    if (!isEditMode && prevWarehouseId.current && prevWarehouseId.current !== selectedWarehouseId) {
      reset((values) => ({ ...values, zoneId: '' }));
    }
    prevWarehouseId.current = selectedWarehouseId;
  }, [selectedWarehouseId, isEditMode, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="location-form" onSubmit={handleFormSubmit}>
      {/* Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('location.basicInfo', 'Basic Information')}</h3>
          <p className="form-section__description">
            {t('location.basicInfoDesc', 'Location identification and assignment')}
          </p>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('common.code', 'Code')}</label>
              <input
                type="text"
                className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                {...register('code', { required: 'Code is required' })}
                placeholder="A-01-01-01"
                disabled={isEditMode}
              />
              {errors.code && <span className="form-field__error">{errors.code.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('common.name', 'Name')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('name')}
                placeholder={t('location.namePlaceholder', 'Optional location name')}
              />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('location.warehouse', 'Warehouse')}</label>
              <Controller
                name="warehouseId"
                control={control}
                rules={{ required: 'Warehouse is required' }}
                render={({ field }) => (
                  <select
                    className={`form-field__select ${errors.warehouseId ? 'form-field__input--error' : ''}`}
                    {...field}
                    disabled={isEditMode}
                  >
                    <option value="">{t('location.selectWarehouse', 'Select warehouse...')}</option>
                    {warehouses.map((wh) => (
                      <option key={wh.id} value={wh.id}>
                        {wh.name}
                      </option>
                    ))}
                  </select>
                )}
              />
              {errors.warehouseId && (
                <span className="form-field__error">{errors.warehouseId.message}</span>
              )}
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.zone', 'Zone')}</label>
              <Controller
                name="zoneId"
                control={control}
                render={({ field }) => (
                  <select className="form-field__select" {...field} disabled={!selectedWarehouseId}>
                    <option value="">{t('location.selectZone', 'Select zone (optional)...')}</option>
                    {filteredZones.map((zone) => (
                      <option key={zone.id} value={zone.id}>
                        {zone.name}
                      </option>
                    ))}
                  </select>
                )}
              />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('location.locationType', 'Location Type')}</label>
              <Controller
                name="locationType"
                control={control}
                render={({ field }) => (
                  <select className="form-field__select" {...field}>
                    {locationTypes.map((type) => (
                      <option key={type} value={type}>
                        {type}
                      </option>
                    ))}
                  </select>
                )}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Position */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('location.positionInfo', 'Position')}</h3>
          <p className="form-section__description">
            {t('location.positionDesc', 'Physical location coordinates')}
          </p>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--4">
            <div className="form-field">
              <label className="form-field__label">{t('location.aisle', 'Aisle')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('aisle')}
                placeholder="A"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.rack', 'Rack')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('rack')}
                placeholder="01"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.level', 'Level')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('level')}
                placeholder="01"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.position', 'Position')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('position')}
                placeholder="01"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Dimensions & Capacity */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('location.dimensions', 'Dimensions & Capacity')}</h3>
          <p className="form-section__description">
            {t('location.dimensionsDesc', 'Physical dimensions and capacity limits')}
          </p>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--3">
            <div className="form-field">
              <label className="form-field__label">{t('location.width', 'Width (mm)')}</label>
              <input
                type="number"
                className="form-field__input"
                {...register('widthMm', { valueAsNumber: true })}
                placeholder="1000"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.height', 'Height (mm)')}</label>
              <input
                type="number"
                className="form-field__input"
                {...register('heightMm', { valueAsNumber: true })}
                placeholder="2000"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.depth', 'Depth (mm)')}</label>
              <input
                type="number"
                className="form-field__input"
                {...register('depthMm', { valueAsNumber: true })}
                placeholder="800"
              />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('location.maxWeight', 'Max Weight (kg)')}</label>
              <input
                type="number"
                className="form-field__input"
                {...register('maxWeight', { valueAsNumber: true })}
                placeholder="500"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.maxVolume', 'Max Volume (m³)')}</label>
              <input
                type="number"
                step="0.01"
                className="form-field__input"
                {...register('maxVolume', { valueAsNumber: true })}
                placeholder="1.6"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Settings */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('location.settings', 'Settings')}</h3>
          <p className="form-section__description">
            {t('location.settingsDesc', 'Location flags and sequences')}
          </p>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('location.pickSequence', 'Pick Sequence')}</label>
              <input
                type="number"
                className="form-field__input"
                {...register('pickSequence', { valueAsNumber: true })}
                placeholder="1"
                min={1}
              />
              <span className="form-field__hint">
                {t('location.pickSequenceHint', 'Order in picking route')}
              </span>
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('location.putawaySequence', 'Putaway Sequence')}</label>
              <input
                type="number"
                className="form-field__input"
                {...register('putawaySequence', { valueAsNumber: true })}
                placeholder="1"
                min={1}
              />
              <span className="form-field__hint">
                {t('location.putawaySequenceHint', 'Order in putaway route')}
              </span>
            </div>
          </div>
          <div className="form-field__checkbox-group">
            <label className="form-field__checkbox">
              <input type="checkbox" {...register('isActive')} />
              <span>{t('location.isActive', 'Active')}</span>
            </label>
            <label className="form-field__checkbox">
              <input type="checkbox" {...register('isPickLocation')} />
              <span>{t('location.isPickLocation', 'Pick Location')}</span>
            </label>
            <label className="form-field__checkbox">
              <input type="checkbox" {...register('isPutawayLocation')} />
              <span>{t('location.isPutawayLocation', 'Putaway Location')}</span>
            </label>
            <label className="form-field__checkbox">
              <input type="checkbox" {...register('isReceivingDock')} />
              <span>{t('location.isReceivingDock', 'Receiving Dock')}</span>
            </label>
            <label className="form-field__checkbox">
              <input type="checkbox" {...register('isShippingDock')} />
              <span>{t('location.isShippingDock', 'Shipping Dock')}</span>
            </label>
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
              ? t('common.save', 'Save')
              : t('common.create', 'Create')}
        </button>
      </div>
    </form>
  );
}

export default LocationForm;
