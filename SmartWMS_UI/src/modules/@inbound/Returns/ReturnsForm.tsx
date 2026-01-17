import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetCustomersQuery } from '@/api/modules/orders';
import { useGetLocationsQuery } from '@/api/modules/locations';
import type { ReturnType } from '@/api/modules/returns';
import './ReturnsForm.scss';

export interface ReturnsFormData {
  customerId: string;
  returnType: ReturnType;
  receivingLocationId: string;
  requestedDate: string;
  rmaNumber: string;
  rmaExpiryDate: string;
  reasonDescription: string;
  notes: string;
}

interface ReturnsFormProps {
  onSubmit: (data: ReturnsFormData) => Promise<void>;
  loading?: boolean;
}

const RETURN_TYPE_LABELS: Record<ReturnType, string> = {
  CustomerReturn: 'Customer',
  SupplierReturn: 'Supplier',
  InternalTransfer: 'Internal',
  Damaged: 'Damaged',
  Recall: 'Recall',
};

const RETURN_TYPES: ReturnType[] = [
  'CustomerReturn',
  'SupplierReturn',
  'InternalTransfer',
  'Damaged',
  'Recall',
];

export function ReturnsForm({ onSubmit, loading }: ReturnsFormProps) {
  const t = useTranslate();

  const { data: customersData } = useGetCustomersQuery({ page: 1, pageSize: 100 });
  const { data: locationsData } = useGetLocationsQuery({ page: 1, pageSize: 100 });

  const customers = customersData?.data?.items || [];
  const locations = locationsData?.data?.items || [];

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReturnsFormData>({
    defaultValues: {
      customerId: '',
      returnType: 'CustomerReturn',
      receivingLocationId: '',
      requestedDate: '',
      rmaNumber: '',
      rmaExpiryDate: '',
      reasonDescription: '',
      notes: '',
    },
  });

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="returns-form" onSubmit={handleFormSubmit}>
      {/* Section: Return Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('returns.returnDetails', 'Return Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('common.customer', 'Customer')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.customerId ? 'form-field__select--error' : ''}`}
                {...register('customerId', { required: t('validation.required', 'Required') })}
              >
                <option value="">{t('common.selectCustomer', 'Select customer...')}</option>
                {customers.map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.code} - {customer.name}
                  </option>
                ))}
              </select>
              {errors.customerId && <span className="form-field__error">{errors.customerId.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('returns.type', 'Return Type')}</label>
              <select className="form-field__select" {...register('returnType')}>
                {RETURN_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {RETURN_TYPE_LABELS[type]}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('returns.rmaNumber', 'RMA Number')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="RMA-2024-001"
                {...register('rmaNumber')}
              />
              <p className="form-field__hint">{t('returns.rmaNumberHint', 'Leave empty to auto-generate')}</p>
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('returns.rmaExpiryDate', 'RMA Expiry Date')}</label>
              <input
                type="date"
                className="form-field__input"
                {...register('rmaExpiryDate')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Shipping & Receiving */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('returns.shippingDetails', 'Shipping & Receiving')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('returns.requestedDate', 'Requested Date')}</label>
              <input
                type="date"
                className="form-field__input"
                {...register('requestedDate')}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('returns.receivingLocation', 'Receiving Location')}</label>
              <select className="form-field__select" {...register('receivingLocationId')}>
                <option value="">{t('common.selectLocation', 'Select location...')}</option>
                {locations.map((loc) => (
                  <option key={loc.id} value={loc.id}>
                    {loc.code} - {loc.name || loc.zoneName}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('returns.reasonDescription', 'Reason')}</label>
              <textarea
                className="form-field__textarea"
                rows={2}
                placeholder={t('returns.reasonPlaceholder', 'Reason for return...')}
                {...register('reasonDescription')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Notes */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('common.notes', 'Notes')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('common.notes', 'Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('returns.notesPlaceholder', 'Internal notes...')}
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
          {loading ? t('common.saving', 'Saving...') : t('returns.createRMA', 'Create RMA')}
        </button>
      </div>
    </form>
  );
}
