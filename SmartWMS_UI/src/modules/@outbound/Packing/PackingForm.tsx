import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetSalesOrdersQuery } from '@/api/modules/orders';
import { useGetPackingStationsQuery } from '@/api/modules/packing';
import './PackingForm.scss';

export interface PackingFormData {
  salesOrderId: string;
  packingStationId: string;
  priority: number;
  notes: string;
}

interface PackingFormProps {
  onSubmit: (data: PackingFormData) => Promise<void>;
  loading?: boolean;
}

export function PackingForm({ onSubmit, loading }: PackingFormProps) {
  const t = useTranslate();

  const { data: salesOrdersData } = useGetSalesOrdersQuery({ page: 1, pageSize: 100 });
  const { data: stationsData } = useGetPackingStationsQuery();

  const salesOrders = salesOrdersData?.data?.items || [];
  const stations = stationsData?.data?.items || [];

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PackingFormData>({
    defaultValues: {
      salesOrderId: '',
      packingStationId: '',
      priority: 5,
      notes: '',
    },
  });

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="packing-form" onSubmit={handleFormSubmit}>
      {/* Section: Task Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('packing.taskDetails', 'Task Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('packing.salesOrder', 'Sales Order')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.salesOrderId ? 'form-field__select--error' : ''}`}
                {...register('salesOrderId', { required: t('validation.required', 'Required') })}
              >
                <option value="">{t('common.selectOrder', 'Select order...')}</option>
                {salesOrders.map((order) => (
                  <option key={order.id} value={order.id}>
                    {order.orderNumber} - {order.customerName}
                  </option>
                ))}
              </select>
              {errors.salesOrderId && <span className="form-field__error">{errors.salesOrderId.message}</span>}
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('packing.station', 'Packing Station')}</label>
              <select className="form-field__select" {...register('packingStationId')}>
                <option value="">{t('packing.selectStation', 'Select station...')}</option>
                {stations.map((station) => (
                  <option key={station.id} value={station.id}>
                    {station.code} - {station.name}
                  </option>
                ))}
              </select>
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
              <p className="form-field__hint">{t('packing.priorityHint', '1-10, lower is higher priority')}</p>
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
                placeholder={t('packing.notesPlaceholder', 'Internal notes...')}
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
          {loading ? t('common.saving', 'Saving...') : t('packing.createTask', 'Create Task')}
        </button>
      </div>
    </form>
  );
}
