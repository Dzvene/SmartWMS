import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import type { AdjustmentType } from '@/api/modules/adjustments';

export interface AdjustmentFormData {
  warehouseId: string;
  adjustmentType: AdjustmentType;
  reasonNotes: string;
  notes: string;
}

interface AdjustmentFormProps {
  initialData?: Partial<AdjustmentFormData>;
  onSubmit: (data: AdjustmentFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const ADJUSTMENT_TYPES: { value: AdjustmentType; label: string }[] = [
  { value: 'Correction', label: 'Correction' },
  { value: 'CycleCount', label: 'Cycle Count' },
  { value: 'Damage', label: 'Damage' },
  { value: 'Scrap', label: 'Scrap' },
  { value: 'Found', label: 'Found' },
  { value: 'Lost', label: 'Lost' },
  { value: 'Expiry', label: 'Expiry' },
  { value: 'QualityHold', label: 'Quality Hold' },
  { value: 'Revaluation', label: 'Revaluation' },
  { value: 'Opening', label: 'Opening Balance' },
  { value: 'Other', label: 'Other' },
];

export function AdjustmentForm({ initialData, onSubmit, loading, isEditMode }: AdjustmentFormProps) {
  const t = useTranslate();
  const { data: warehousesData } = useGetWarehouseOptionsQuery();
  const warehouses = warehousesData?.data || [];

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<AdjustmentFormData>({
    defaultValues: {
      warehouseId: '',
      adjustmentType: 'Correction',
      reasonNotes: '',
      notes: '',
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        warehouseId: initialData.warehouseId || '',
        adjustmentType: initialData.adjustmentType || 'Correction',
        reasonNotes: initialData.reasonNotes || '',
        notes: initialData.notes || '',
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="entity-form" onSubmit={handleFormSubmit}>
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('adjustments.adjustmentDetails', 'Adjustment Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">
                {t('common.warehouse', 'Warehouse')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.warehouseId ? 'form-field__select--error' : ''}`}
                {...register('warehouseId', { required: t('validation.required', 'Required') })}
                disabled={isEditMode}
              >
                <option value="">{t('common.selectWarehouse', 'Select warehouse...')}</option>
                {warehouses.map((wh) => (
                  <option key={wh.id} value={wh.id}>
                    {wh.code} - {wh.name}
                  </option>
                ))}
              </select>
              {errors.warehouseId && <span className="form-field__error">{errors.warehouseId.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('adjustments.adjustmentType', 'Adjustment Type')}</label>
              <select
                className="form-field__select"
                {...register('adjustmentType')}
                disabled={isEditMode}
              >
                {ADJUSTMENT_TYPES.map((type) => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('adjustments.reason', 'Reason')}</label>
              <textarea
                className="form-field__textarea"
                rows={2}
                placeholder={t('adjustments.reasonPlaceholder', 'Reason for adjustment...')}
                {...register('reasonNotes')}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('common.notes', 'Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('adjustments.notesPlaceholder', 'Additional notes...')}
                {...register('notes')}
              />
            </div>
          </div>
        </div>
      </section>

      <div className="form-actions">
        <button type="submit" className="btn btn--primary" disabled={loading || (!isDirty && isEditMode)}>
          {loading
            ? t('common.saving', 'Saving...')
            : isEditMode
            ? t('common.saveChanges', 'Save Changes')
            : t('common.create', 'Create')}
        </button>
      </div>
    </form>
  );
}
