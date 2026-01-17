import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import type { BarcodePrefixDto, BarcodePrefixType } from '@/api/modules/configuration';

export interface BarcodeFormData {
  prefix: string;
  prefixType: BarcodePrefixType;
  description: string;
  isActive: boolean;
}

interface BarcodeFormProps {
  initialData?: BarcodePrefixDto;
  onSubmit: (data: BarcodeFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const typeLabels: Record<BarcodePrefixType, string> = {
  Product: 'Product',
  Location: 'Location',
  Container: 'Container',
  Pallet: 'Pallet',
  Order: 'Order',
  Transfer: 'Transfer',
  Receipt: 'Receipt',
  Shipment: 'Shipment',
  User: 'User',
  Equipment: 'Equipment',
  Other: 'Other',
};

const PREFIX_TYPES: BarcodePrefixType[] = [
  'Product', 'Location', 'Container', 'Pallet', 'Order',
  'Transfer', 'Receipt', 'Shipment', 'User', 'Equipment', 'Other'
];

export function BarcodeForm({ initialData, onSubmit, loading, isEditMode }: BarcodeFormProps) {
  const t = useTranslate();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<BarcodeFormData>({
    defaultValues: {
      prefix: '',
      prefixType: 'Product',
      description: '',
      isActive: true,
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        prefix: initialData.prefix,
        prefixType: initialData.prefixType,
        description: initialData.description || '',
        isActive: initialData.isActive,
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="barcode-form" onSubmit={handleFormSubmit}>
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('barcodes.details', 'Prefix Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('barcodes.prefix', 'Prefix')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.prefix ? 'form-field__input--error' : ''}`}
                placeholder="PRD"
                {...register('prefix', { required: t('validation.required', 'Required') })}
              />
              {errors.prefix && <span className="form-field__error">{errors.prefix.message}</span>}
              <p className="form-field__hint">
                {t('barcodes.prefixHint', 'Short prefix used to identify barcode type (e.g., PRD for products)')}
              </p>
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('common.type', 'Type')} <span className="required">*</span>
              </label>
              <select
                className="form-field__select"
                disabled={isEditMode}
                {...register('prefixType', { required: true })}
              >
                {PREFIX_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {typeLabels[type]}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('barcodes.description', 'Description')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('barcodes.descriptionPlaceholder', 'Optional description...')}
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
              : t('barcodes.createBarcode', 'Create Prefix')}
        </button>
      </div>
    </form>
  );
}
