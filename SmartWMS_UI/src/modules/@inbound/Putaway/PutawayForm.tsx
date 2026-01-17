import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetProductsQuery } from '@/api/modules/products';
import { useGetLocationsQuery } from '@/api/modules/locations';
import './PutawayForm.scss';

export interface PutawayFormData {
  productId: string;
  fromLocationId: string;
  quantity: number;
  batchNumber: string;
  serialNumber: string;
  expiryDate: string;
  suggestedLocationId: string;
  priority: number;
  notes: string;
}

interface PutawayFormProps {
  onSubmit: (data: PutawayFormData) => Promise<void>;
  loading?: boolean;
}

export function PutawayForm({ onSubmit, loading }: PutawayFormProps) {
  const t = useTranslate();

  const { data: productsData } = useGetProductsQuery({ page: 1, pageSize: 100 });
  const { data: locationsData } = useGetLocationsQuery({ page: 1, pageSize: 100 });

  const products = productsData?.data?.items || [];
  const locations = locationsData?.data?.items || [];

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PutawayFormData>({
    defaultValues: {
      productId: '',
      fromLocationId: '',
      quantity: 1,
      batchNumber: '',
      serialNumber: '',
      expiryDate: '',
      suggestedLocationId: '',
      priority: 5,
      notes: '',
    },
  });

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="putaway-form" onSubmit={handleFormSubmit}>
      {/* Section: Task Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('putaway.taskDetails', 'Task Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('common.product', 'Product')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.productId ? 'form-field__select--error' : ''}`}
                {...register('productId', { required: t('validation.required', 'Required') })}
              >
                <option value="">{t('common.selectProduct', 'Select product...')}</option>
                {products.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.sku} - {product.name}
                  </option>
                ))}
              </select>
              {errors.productId && <span className="form-field__error">{errors.productId.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">
                {t('putaway.fromLocation', 'From Location')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.fromLocationId ? 'form-field__select--error' : ''}`}
                {...register('fromLocationId', { required: t('validation.required', 'Required') })}
              >
                <option value="">{t('common.selectLocation', 'Select location...')}</option>
                {locations.map((loc) => (
                  <option key={loc.id} value={loc.id}>
                    {loc.code} - {loc.name || loc.zoneName}
                  </option>
                ))}
              </select>
              {errors.fromLocationId && <span className="form-field__error">{errors.fromLocationId.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">
                {t('common.quantity', 'Quantity')} <span className="required">*</span>
              </label>
              <input
                type="number"
                min="1"
                className={`form-field__input ${errors.quantity ? 'form-field__input--error' : ''}`}
                {...register('quantity', { required: t('validation.required', 'Required'), min: 1, valueAsNumber: true })}
              />
              {errors.quantity && <span className="form-field__error">{errors.quantity.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('common.priority', 'Priority')}</label>
              <input
                type="number"
                min="1"
                max="10"
                className="form-field__input"
                {...register('priority', { valueAsNumber: true })}
              />
              <p className="form-field__hint">{t('putaway.priorityHint', '1-10, lower is higher priority')}</p>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Batch Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('putaway.batchInfo', 'Batch Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('putaway.batchNumber', 'Batch Number')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="BATCH-001"
                {...register('batchNumber')}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('putaway.serialNumber', 'Serial Number')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="SN-12345"
                {...register('serialNumber')}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('putaway.expiryDate', 'Expiry Date')}</label>
              <input
                type="date"
                className="form-field__input"
                {...register('expiryDate')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Putaway Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('putaway.putawayDetails', 'Putaway Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('putaway.suggestedLocation', 'Suggested Location')}</label>
              <select
                className="form-field__select"
                {...register('suggestedLocationId')}
              >
                <option value="">{t('putaway.noSuggestion', 'No suggestion')}</option>
                {locations.map((loc) => (
                  <option key={loc.id} value={loc.id}>
                    {loc.code} - {loc.name || loc.zoneName}
                  </option>
                ))}
              </select>
              <p className="form-field__hint">{t('putaway.suggestedLocationHint', 'Leave empty to auto-suggest')}</p>
            </div>
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('common.notes', 'Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('putaway.notesPlaceholder', 'Internal notes...')}
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
          disabled={loading}
        >
          {loading ? t('common.saving', 'Saving...') : t('putaway.createTask', 'Create Task')}
        </button>
      </div>
    </form>
  );
}
