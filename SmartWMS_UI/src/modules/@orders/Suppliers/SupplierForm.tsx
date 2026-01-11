import { useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { SupplierDto } from '@/api/modules/orders';

export interface SupplierFormData {
  code: string;
  name: string;
  contactName: string;
  email: string;
  phone: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  region: string;
  postalCode: string;
  countryCode: string;
  taxId: string;
  paymentTerms: string;
  leadTimeDays: number | '';
  isActive: boolean;
}

interface SupplierFormProps {
  initialData?: SupplierDto;
  onSubmit: (data: SupplierFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

export function SupplierForm({ initialData, onSubmit, loading, isEditMode }: SupplierFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<SupplierFormData>({
    defaultValues: {
      code: '',
      name: '',
      contactName: '',
      email: '',
      phone: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      region: '',
      postalCode: '',
      countryCode: '',
      taxId: '',
      paymentTerms: '',
      leadTimeDays: '',
      isActive: true,
    },
  });

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        code: initialData.code,
        name: initialData.name,
        contactName: initialData.contactName || '',
        email: initialData.email || '',
        phone: initialData.phone || '',
        addressLine1: initialData.addressLine1 || '',
        addressLine2: initialData.addressLine2 || '',
        city: initialData.city || '',
        region: initialData.region || '',
        postalCode: initialData.postalCode || '',
        countryCode: initialData.countryCode || '',
        taxId: initialData.taxId || '',
        paymentTerms: initialData.paymentTerms || '',
        leadTimeDays: initialData.leadTimeDays || '',
        isActive: initialData.isActive,
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="supplier-form" onSubmit={handleFormSubmit}>
      {/* Section: Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('suppliers.basicInfo', 'Basic Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('suppliers.code', 'Code')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                placeholder="SUPP001"
                disabled={isEditMode}
                {...register('code', { required: t('validation.required', 'Required') })}
              />
              {errors.code && <span className="form-field__error">{errors.code.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('suppliers.name', 'Name')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                placeholder="Supplier Name"
                {...register('name', { required: t('validation.required', 'Required') })}
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.contact', 'Contact Person')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="John Doe"
                {...register('contactName')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.email', 'Email')}</label>
              <input
                type="email"
                className="form-field__input"
                placeholder="email@example.com"
                {...register('email')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.phone', 'Phone')}</label>
              <input
                type="tel"
                className="form-field__input"
                placeholder="+1 234 567 8900"
                {...register('phone')}
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

      {/* Section: Address */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('suppliers.address', 'Address')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('suppliers.addressLine1', 'Address Line 1')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="123 Main Street"
                {...register('addressLine1')}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('suppliers.addressLine2', 'Address Line 2')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="Suite 100"
                {...register('addressLine2')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.city', 'City')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="New York"
                {...register('city')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.region', 'Region/State')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="NY"
                {...register('region')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.postalCode', 'Postal Code')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="10001"
                {...register('postalCode')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.country', 'Country')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="US"
                {...register('countryCode')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Business Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('suppliers.businessInfo', 'Business Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('suppliers.taxId', 'Tax ID')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="XX-XXXXXXX"
                {...register('taxId')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.paymentTerms', 'Payment Terms')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="Net 30"
                {...register('paymentTerms')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('suppliers.leadTimeDays', 'Lead Time (days)')}</label>
              <input
                type="number"
                min="0"
                className="form-field__input"
                placeholder="7"
                {...register('leadTimeDays')}
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
              : t('suppliers.createSupplier', 'Create Supplier')}
        </button>
      </div>
    </form>
  );
}
