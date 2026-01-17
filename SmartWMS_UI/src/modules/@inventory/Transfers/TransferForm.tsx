import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import type { TransferType, TransferPriority } from '@/api/modules/transfers';

export interface TransferFormData {
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  transferType: TransferType;
  priority: TransferPriority;
  scheduledDate: string;
  notes: string;
}

interface TransferFormProps {
  initialData?: Partial<TransferFormData>;
  onSubmit: (data: TransferFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const TRANSFER_TYPES: { value: TransferType; label: string }[] = [
  { value: 'InterWarehouse', label: 'Inter-Warehouse' },
  { value: 'IntraWarehouse', label: 'Intra-Warehouse' },
  { value: 'Replenishment', label: 'Replenishment' },
  { value: 'Consolidation', label: 'Consolidation' },
  { value: 'Relocation', label: 'Relocation' },
];

const PRIORITIES: { value: TransferPriority; label: string }[] = [
  { value: 'Low', label: 'Low' },
  { value: 'Normal', label: 'Normal' },
  { value: 'High', label: 'High' },
  { value: 'Urgent', label: 'Urgent' },
];

export function TransferForm({ initialData, onSubmit, loading, isEditMode }: TransferFormProps) {
  const t = useTranslate();
  const { data: warehousesData } = useGetWarehouseOptionsQuery();
  const warehouses = warehousesData?.data || [];

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<TransferFormData>({
    defaultValues: {
      sourceWarehouseId: '',
      destinationWarehouseId: '',
      transferType: 'InterWarehouse',
      priority: 'Normal',
      scheduledDate: '',
      notes: '',
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        sourceWarehouseId: initialData.sourceWarehouseId || '',
        destinationWarehouseId: initialData.destinationWarehouseId || '',
        transferType: initialData.transferType || 'InterWarehouse',
        priority: initialData.priority || 'Normal',
        scheduledDate: initialData.scheduledDate || '',
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
          <h3 className="form-section__title">{t('transfers.transferDetails', 'Transfer Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">
                {t('transfers.sourceWarehouse', 'Source Warehouse')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.sourceWarehouseId ? 'form-field__select--error' : ''}`}
                {...register('sourceWarehouseId', { required: t('validation.required', 'Required') })}
                disabled={isEditMode}
              >
                <option value="">{t('common.selectWarehouse', 'Select warehouse...')}</option>
                {warehouses.map((wh) => (
                  <option key={wh.id} value={wh.id}>
                    {wh.code} - {wh.name}
                  </option>
                ))}
              </select>
              {errors.sourceWarehouseId && <span className="form-field__error">{errors.sourceWarehouseId.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('transfers.destinationWarehouse', 'Destination Warehouse')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.destinationWarehouseId ? 'form-field__select--error' : ''}`}
                {...register('destinationWarehouseId', { required: t('validation.required', 'Required') })}
                disabled={isEditMode}
              >
                <option value="">{t('common.selectWarehouse', 'Select warehouse...')}</option>
                {warehouses.map((wh) => (
                  <option key={wh.id} value={wh.id}>
                    {wh.code} - {wh.name}
                  </option>
                ))}
              </select>
              {errors.destinationWarehouseId && <span className="form-field__error">{errors.destinationWarehouseId.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('transfers.transferType', 'Transfer Type')}</label>
              <select
                className="form-field__select"
                {...register('transferType')}
                disabled={isEditMode}
              >
                {TRANSFER_TYPES.map((type) => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('common.priority', 'Priority')}</label>
              <select className="form-field__select" {...register('priority')}>
                {PRIORITIES.map((p) => (
                  <option key={p.value} value={p.value}>
                    {p.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('transfers.scheduledDate', 'Scheduled Date')}</label>
              <input
                type="date"
                className="form-field__input"
                {...register('scheduledDate')}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('common.notes', 'Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('transfers.notesPlaceholder', 'Additional notes...')}
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
