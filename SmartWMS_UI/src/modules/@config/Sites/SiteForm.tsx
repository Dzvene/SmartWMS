import { useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm, Controller } from 'react-hook-form';

export interface SiteFormData {
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

interface SiteFormProps {
  initialData?: Partial<SiteFormData>;
  onSubmit: (data: SiteFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const defaultValues: SiteFormData = {
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

/**
 * SiteForm - Pure form component
 *
 * Receives initialData from parent container.
 * Knows nothing about data fetching or API calls.
 */
export function SiteForm({ initialData, onSubmit, loading, isEditMode }: SiteFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isDirty },
  } = useForm<SiteFormData>({
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
    <form className="site-form" onSubmit={handleFormSubmit}>
      {/* Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('site.basicInfo', 'Basic Information')}</h3>
          <p className="form-section__description">{t('site.basicInfoDesc', 'Site identification and description')}</p>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('common.code', 'Code')}</label>
              <input
                type="text"
                className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                {...register('code', { required: 'Code is required' })}
                placeholder="SITE-001"
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
                placeholder="Main Distribution Site"
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>
          </div>
          <div className="form-grid--single">
            <div className="form-field">
              <label className="form-field__label">{t('common.description', 'Description')}</label>
              <textarea
                className="form-field__input form-field__textarea"
                {...register('description')}
                placeholder="Enter site description..."
                rows={3}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Address */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('site.address', 'Address')}</h3>
          <p className="form-section__description">{t('site.addressDesc', 'Physical location of the site')}</p>
        </div>
        <div className="form-section__content">
          <div className="form-grid--single">
            <div className="form-field">
              <label className="form-field__label">{t('site.addressLine1', 'Address Line 1')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('addressLine1')}
                placeholder="123 Industrial Blvd"
              />
            </div>
          </div>
          <div className="form-grid--single">
            <div className="form-field">
              <label className="form-field__label">{t('site.addressLine2', 'Address Line 2')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('addressLine2')}
                placeholder="Building A, Suite 100"
              />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('site.city', 'City')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('city')}
                placeholder="Stockholm"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('site.region', 'Region/State')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('region')}
                placeholder="Stockholm County"
              />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('site.postalCode', 'Postal Code')}</label>
              <input
                type="text"
                className="form-field__input"
                {...register('postalCode')}
                placeholder="111 22"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('site.country', 'Country')}</label>
              <Controller
                name="countryCode"
                control={control}
                render={({ field }) => (
                  <select className="form-field__select" {...field}>
                    <option value="">Select country...</option>
                    <option value="SE">Sweden</option>
                    <option value="NO">Norway</option>
                    <option value="DK">Denmark</option>
                    <option value="FI">Finland</option>
                    <option value="DE">Germany</option>
                    <option value="PL">Poland</option>
                    <option value="US">United States</option>
                    <option value="GB">United Kingdom</option>
                  </select>
                )}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Settings */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('site.settings', 'Settings')}</h3>
          <p className="form-section__description">{t('site.settingsDesc', 'Timezone and status configuration')}</p>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('site.timezone', 'Timezone')}</label>
              <Controller
                name="timezone"
                control={control}
                render={({ field }) => (
                  <select className="form-field__select" {...field}>
                    <option value="">Select timezone...</option>
                    <option value="Europe/Stockholm">Europe/Stockholm (CET)</option>
                    <option value="Europe/Oslo">Europe/Oslo (CET)</option>
                    <option value="Europe/Copenhagen">Europe/Copenhagen (CET)</option>
                    <option value="Europe/Helsinki">Europe/Helsinki (EET)</option>
                    <option value="Europe/Berlin">Europe/Berlin (CET)</option>
                    <option value="Europe/Warsaw">Europe/Warsaw (CET)</option>
                    <option value="Europe/London">Europe/London (GMT)</option>
                    <option value="America/New_York">America/New_York (EST)</option>
                    <option value="America/Los_Angeles">America/Los_Angeles (PST)</option>
                  </select>
                )}
              />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__checkbox">
                <input type="checkbox" {...register('isPrimary')} />
                <span>{t('site.isPrimary', 'Primary Site')}</span>
              </label>
              <span className="form-field__hint">
                {t('site.isPrimaryHint', 'Primary site is used as default for new operations')}
              </span>
            </div>
            <div className="form-field">
              <label className="form-field__checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('site.isActive', 'Active')}</span>
              </label>
              <span className="form-field__hint">
                {t('site.isActiveHint', 'Inactive sites cannot be used for operations')}
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

export default SiteForm;
