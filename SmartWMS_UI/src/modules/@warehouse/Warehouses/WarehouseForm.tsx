import { useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';

export interface WarehouseFormData {
  code: string;
  name: string;
  description: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  region: string;
  postalCode: string;
  countryCode: string;
  timezone: string;
  isPrimary: boolean;
  isActive: boolean;
}

interface WarehouseFormProps {
  initialData?: Partial<WarehouseFormData>;
  onSubmit: (data: WarehouseFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const defaultValues: WarehouseFormData = {
  code: '',
  name: '',
  description: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  region: '',
  postalCode: '',
  countryCode: '',
  timezone: '',
  isPrimary: false,
  isActive: true,
};

export function WarehouseForm({ initialData, onSubmit, loading, isEditMode }: WarehouseFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<WarehouseFormData>({
    defaultValues: { ...defaultValues, ...initialData },
  });

  useEffect(() => {
    if (initialData) {
      reset({ ...defaultValues, ...initialData });
    }
  }, [initialData, reset]);

  const onFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="warehouse-form" onSubmit={onFormSubmit}>
      {/* Basic Info */}
      <section className="warehouse-form__section">
        <h3 className="warehouse-form__section-title">{t('warehouse.basicInfo', 'Basic Information')}</h3>

        <div className="warehouse-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('warehouse.code', 'Code')} *</label>
            <input
              type="text"
              className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
              placeholder="WH-001"
              {...register('code', { required: t('validation.required', 'Required') })}
              disabled={isEditMode}
            />
            {errors.code && <span className="form-field__error">{errors.code.message}</span>}
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('warehouse.name', 'Name')} *</label>
            <input
              type="text"
              className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
              placeholder={t('warehouse.namePlaceholder', 'Main Warehouse')}
              {...register('name', { required: t('validation.required', 'Required') })}
            />
            {errors.name && <span className="form-field__error">{errors.name.message}</span>}
          </div>
        </div>

        <div className="form-field">
          <label className="form-field__label">{t('common.description', 'Description')}</label>
          <textarea
            className="form-field__textarea"
            rows={3}
            placeholder={t('warehouse.descriptionPlaceholder', 'Warehouse description...')}
            {...register('description')}
          />
        </div>

        <div className="warehouse-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('warehouse.timezone', 'Timezone')}</label>
            <select className="form-field__select" {...register('timezone')}>
              <option value="">Select timezone</option>
              <option value="Europe/Stockholm">Europe/Stockholm</option>
              <option value="Europe/London">Europe/London</option>
              <option value="America/New_York">America/New_York</option>
              <option value="America/Los_Angeles">America/Los_Angeles</option>
              <option value="Asia/Tokyo">Asia/Tokyo</option>
            </select>
          </div>

          <div className="form-field">
            <label className="form-field__label">&nbsp;</label>
            <div className="form-field__checkbox-group">
              <label className="form-field__checkbox">
                <input type="checkbox" {...register('isPrimary')} />
                <span>{t('warehouse.isPrimary', 'Primary Warehouse')}</span>
              </label>
              <label className="form-field__checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('common.active', 'Active')}</span>
              </label>
            </div>
          </div>
        </div>
      </section>

      {/* Address */}
      <section className="warehouse-form__section">
        <h3 className="warehouse-form__section-title">{t('warehouse.address', 'Address')}</h3>

        <div className="form-field">
          <label className="form-field__label">{t('warehouse.addressLine1', 'Address Line 1')}</label>
          <input
            type="text"
            className="form-field__input"
            placeholder="123 Industrial Blvd"
            {...register('addressLine1')}
          />
        </div>

        <div className="form-field">
          <label className="form-field__label">{t('warehouse.addressLine2', 'Address Line 2')}</label>
          <input
            type="text"
            className="form-field__input"
            placeholder="Building A"
            {...register('addressLine2')}
          />
        </div>

        <div className="warehouse-form__row--three warehouse-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('warehouse.city', 'City')}</label>
            <input
              type="text"
              className="form-field__input"
              placeholder="Stockholm"
              {...register('city')}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('warehouse.region', 'Region/State')}</label>
            <input
              type="text"
              className="form-field__input"
              placeholder="Stockholm County"
              {...register('region')}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('warehouse.postalCode', 'Postal Code')}</label>
            <input
              type="text"
              className="form-field__input"
              placeholder="111 22"
              {...register('postalCode')}
            />
          </div>
        </div>

        <div className="form-field">
          <label className="form-field__label">{t('warehouse.country', 'Country')}</label>
          <select className="form-field__select" {...register('countryCode')}>
            <option value="">Select country</option>
            <option value="SE">Sweden</option>
            <option value="NO">Norway</option>
            <option value="DK">Denmark</option>
            <option value="FI">Finland</option>
            <option value="DE">Germany</option>
            <option value="GB">United Kingdom</option>
            <option value="US">United States</option>
          </select>
        </div>
      </section>

      {/* Actions */}
      <div className="warehouse-form__actions">
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

export default WarehouseForm;
