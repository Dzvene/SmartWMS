import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import type { ReasonCodeDto, ReasonCodeType } from '@/api/modules/configuration';

const categoryLabels: Record<ReasonCodeType, string> = {
  Adjustment: 'Adjustment',
  Return: 'Return',
  Damage: 'Damage',
  Expiry: 'Expiry',
  QualityHold: 'Quality Hold',
  Transfer: 'Transfer',
  Scrap: 'Scrap',
  Found: 'Found',
  Lost: 'Lost',
  Other: 'Other',
};

const REASON_TYPES: ReasonCodeType[] = [
  'Adjustment', 'Return', 'Damage', 'Expiry', 'QualityHold',
  'Transfer', 'Scrap', 'Found', 'Lost', 'Other'
];

export interface ReasonCodeFormData {
  code: string;
  name: string;
  description: string;
  reasonType: ReasonCodeType;
  requiresNotes: boolean;
  sortOrder: number;
  isActive: boolean;
}

interface ReasonCodeFormProps {
  initialData?: ReasonCodeDto;
  onSubmit: (data: ReasonCodeFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

export function ReasonCodeForm({ initialData, onSubmit, loading, isEditMode }: ReasonCodeFormProps) {
  const t = useTranslate();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<ReasonCodeFormData>({
    defaultValues: {
      code: '',
      name: '',
      description: '',
      reasonType: 'Adjustment',
      requiresNotes: false,
      sortOrder: 0,
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
        reasonType: initialData.reasonType,
        requiresNotes: initialData.requiresNotes,
        sortOrder: initialData.sortOrder,
        isActive: initialData.isActive,
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="reason-code-form" onSubmit={handleFormSubmit}>
      {/* Section: Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('reasonCodes.details', 'Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('reasonCodes.code', 'Code')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                placeholder="ADJ-01"
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
                placeholder="Inventory Adjustment"
                {...register('name', { required: t('validation.required', 'Required') })}
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('reasonCodes.category', 'Category')} <span className="required">*</span>
              </label>
              <select
                className="form-field__select"
                disabled={isEditMode}
                {...register('reasonType', { required: true })}
              >
                {REASON_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {categoryLabels[type]}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('reasonCodes.sortOrder', 'Sort Order')}</label>
              <input
                type="number"
                min="0"
                className="form-field__input"
                {...register('sortOrder', { valueAsNumber: true })}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('reasonCodes.description', 'Description')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('reasonCodes.descriptionPlaceholder', 'Optional description...')}
                {...register('description')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Settings */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('reasonCodes.settings', 'Settings')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-checkbox">
                <input type="checkbox" {...register('requiresNotes')} />
                <span>{t('reasonCodes.requiresNotes', 'Requires Notes')}</span>
              </label>
              <p className="form-field__hint">
                {t('reasonCodes.requiresNotesHint', 'When enabled, users must provide a note when using this reason code')}
              </p>
            </div>

            <div className="form-field">
              <label className="form-checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('common.active', 'Active')}</span>
              </label>
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
              : t('reasonCodes.createReasonCode', 'Create Reason Code')}
        </button>
      </div>
    </form>
  );
}
