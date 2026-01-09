import { useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm, Controller } from 'react-hook-form';
import type { ZoneType } from '@/api/modules/zones';

export interface ZoneFormData {
  code: string;
  name: string;
  description: string;
  warehouseId: string;
  zoneType: ZoneType;
  pickSequence: number | null;
  isActive: boolean;
}

interface WarehouseOption {
  id: string;
  name: string;
}

interface ZoneFormProps {
  initialData?: Partial<ZoneFormData>;
  warehouses: WarehouseOption[];
  onSubmit: (data: ZoneFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const defaultValues: ZoneFormData = {
  code: '',
  name: '',
  description: '',
  warehouseId: '',
  zoneType: 'Storage',
  pickSequence: null,
  isActive: true,
};

const zoneTypes: ZoneType[] = ['Storage', 'Picking', 'Packing', 'Staging', 'Shipping', 'Receiving', 'Returns'];

/**
 * ZoneForm - Pure form component
 *
 * Receives initialData and warehouses from parent container.
 * Knows nothing about data fetching or API calls.
 */
export function ZoneForm({ initialData, warehouses, onSubmit, loading, isEditMode }: ZoneFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isDirty },
  } = useForm<ZoneFormData>({
    defaultValues,
  });

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({ ...defaultValues, ...initialData });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="zone-form" onSubmit={handleFormSubmit}>
      {/* Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('zone.basicInfo', 'Basic Information')}</h3>
          <p className="form-section__description">{t('zone.basicInfoDesc', 'Zone identification and assignment')}</p>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('common.code', 'Code')}</label>
              <input
                type="text"
                className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                {...register('code', { required: 'Code is required' })}
                placeholder="ZONE-A1"
                disabled={isEditMode}
              />
              {errors.code && <span className="form-field__error">{errors.code.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('common.name', 'Name')}</label>
              <input
                type="text"
                className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                {...register('name', { required: 'Name is required' })}
                placeholder="Storage Zone A"
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('zone.warehouse', 'Warehouse')}</label>
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
                    <option value="">{t('zone.selectWarehouse', 'Select warehouse...')}</option>
                    {warehouses.map((wh) => (
                      <option key={wh.id} value={wh.id}>
                        {wh.name}
                      </option>
                    ))}
                  </select>
                )}
              />
              {errors.warehouseId && <span className="form-field__error">{errors.warehouseId.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('zone.zoneType', 'Zone Type')}</label>
              <Controller
                name="zoneType"
                control={control}
                render={({ field }) => (
                  <select className="form-field__select" {...field}>
                    {zoneTypes.map((type) => (
                      <option key={type} value={type}>
                        {type}
                      </option>
                    ))}
                  </select>
                )}
              />
            </div>
          </div>
          <div className="form-grid--single">
            <div className="form-field">
              <label className="form-field__label">{t('common.description', 'Description')}</label>
              <textarea
                className="form-field__textarea"
                {...register('description')}
                placeholder={t('zone.descriptionPlaceholder', 'Enter zone description...')}
                rows={3}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Settings */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('zone.settings', 'Settings')}</h3>
          <p className="form-section__description">{t('zone.settingsDesc', 'Pick sequence and status')}</p>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('zone.pickSequence', 'Pick Sequence')}</label>
              <input
                type="number"
                className="form-field__input"
                {...register('pickSequence', { valueAsNumber: true })}
                placeholder="1"
                min={1}
              />
              <span className="form-field__hint">
                {t('zone.pickSequenceHint', 'Order in which zones are visited during picking')}
              </span>
            </div>
            <div className="form-field">
              <label className="form-field__checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('zone.isActive', 'Active')}</span>
              </label>
              <span className="form-field__hint">
                {t('zone.isActiveHint', 'Inactive zones cannot be used for operations')}
              </span>
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
          {loading ? t('common.saving', 'Saving...') : isEditMode ? t('common.save', 'Save') : t('common.create', 'Create')}
        </button>
      </div>
    </form>
  );
}

export default ZoneForm;
