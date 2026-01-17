import { useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { CarrierDto, CarrierIntegrationType } from '@/api/modules/carriers';

const INTEGRATION_LABELS: Record<CarrierIntegrationType, string> = {
  Manual: 'Manual',
  API: 'API',
  EDI: 'EDI',
  File: 'File Import',
};

const INTEGRATION_TYPES: CarrierIntegrationType[] = ['Manual', 'API', 'EDI', 'File'];

export interface CarrierFormData {
  code: string;
  name: string;
  description: string;
  contactName: string;
  phone: string;
  email: string;
  website: string;
  accountNumber: string;
  integrationType: CarrierIntegrationType;
  defaultServiceCode: string;
  notes: string;
  isActive: boolean;
}

interface CarrierFormProps {
  initialData?: CarrierDto;
  onSubmit: (data: CarrierFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

export function CarrierForm({ initialData, onSubmit, loading, isEditMode }: CarrierFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<CarrierFormData>({
    defaultValues: {
      code: '',
      name: '',
      description: '',
      contactName: '',
      phone: '',
      email: '',
      website: '',
      accountNumber: '',
      integrationType: 'Manual',
      defaultServiceCode: '',
      notes: '',
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
        contactName: initialData.contactName || '',
        phone: initialData.phone || '',
        email: initialData.email || '',
        website: initialData.website || '',
        accountNumber: initialData.accountNumber || '',
        integrationType: initialData.integrationType,
        defaultServiceCode: initialData.defaultServiceCode || '',
        notes: initialData.notes || '',
        isActive: initialData.isActive,
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="carrier-form" onSubmit={handleFormSubmit}>
      {/* Section: Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('carriers.basicInfo', 'Basic Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('carriers.carrierCode', 'Code')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                placeholder="UPS"
                disabled={isEditMode}
                {...register('code', { required: t('validation.required', 'Required') })}
              />
              {errors.code && <span className="form-field__error">{errors.code.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('carriers.carrierName', 'Name')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                placeholder="United Parcel Service"
                {...register('name', { required: t('validation.required', 'Required') })}
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('carriers.integrationType', 'Integration Type')}</label>
              <select className="form-field__select" {...register('integrationType')}>
                {INTEGRATION_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {INTEGRATION_LABELS[type]}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('carriers.accountNumber', 'Account Number')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="123456789"
                {...register('accountNumber')}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('carriers.description', 'Description')}</label>
              <textarea
                className="form-field__textarea"
                rows={2}
                placeholder={t('carriers.descriptionPlaceholder', 'Optional description...')}
                {...register('description')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('common.status', 'Status')}</label>
              <label className="form-checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('common.active', 'Active')}</span>
              </label>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Contact Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('carriers.contactInfo', 'Contact Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('carriers.contactName', 'Contact Name')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="John Doe"
                {...register('contactName')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('carriers.phone', 'Phone')}</label>
              <input
                type="tel"
                className="form-field__input"
                placeholder="+1 800 742 5877"
                {...register('phone')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('carriers.email', 'Email')}</label>
              <input
                type="email"
                className="form-field__input"
                placeholder="support@carrier.com"
                {...register('email')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('carriers.website', 'Website')}</label>
              <input
                type="url"
                className="form-field__input"
                placeholder="https://www.ups.com"
                {...register('website')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Settings */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('carriers.settings', 'Settings')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('carriers.defaultService', 'Default Service Code')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="GROUND"
                {...register('defaultServiceCode')}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('carriers.notes', 'Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('carriers.notesPlaceholder', 'Internal notes...')}
                {...register('notes')}
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
              : t('carriers.createCarrier', 'Create Carrier')}
        </button>
      </div>
    </form>
  );
}
