import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import { useGetSuppliersQuery } from '@/api/modules/orders';
import type { GoodsReceiptDto } from '@/api/modules/receiving';
import './ReceivingForm.scss';

export interface ReceivingFormData {
  warehouseId: string;
  supplierId: string;
  carrierName: string;
  trackingNumber: string;
  deliveryNote: string;
  notes: string;
}

interface ReceivingFormProps {
  initialData?: GoodsReceiptDto;
  onSubmit: (data: ReceivingFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

export function ReceivingForm({ initialData, onSubmit, loading, isEditMode }: ReceivingFormProps) {
  const t = useTranslate();

  const { data: warehousesData } = useGetWarehouseOptionsQuery();
  const { data: suppliersData } = useGetSuppliersQuery({ page: 1, pageSize: 100 });

  const warehouses = warehousesData?.data || [];
  const suppliers = suppliersData?.data?.items || [];

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<ReceivingFormData>({
    defaultValues: {
      warehouseId: '',
      supplierId: '',
      carrierName: '',
      trackingNumber: '',
      deliveryNote: '',
      notes: '',
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        warehouseId: initialData.warehouseId,
        supplierId: initialData.supplierId || '',
        carrierName: initialData.carrierName || '',
        trackingNumber: initialData.trackingNumber || '',
        deliveryNote: initialData.deliveryNote || '',
        notes: initialData.notes || '',
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="receiving-form" onSubmit={handleFormSubmit}>
      {/* Section: Receipt Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('receiving.receiptDetails', 'Receipt Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('common.warehouse', 'Warehouse')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.warehouseId ? 'form-field__select--error' : ''}`}
                disabled={isEditMode}
                {...register('warehouseId', { required: t('validation.required', 'Required') })}
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
              <label className="form-field__label">{t('common.supplier', 'Supplier')}</label>
              <select
                className="form-field__select"
                disabled={isEditMode}
                {...register('supplierId')}
              >
                <option value="">{t('common.selectSupplier', 'Select supplier...')}</option>
                {suppliers.map((supplier) => (
                  <option key={supplier.id} value={supplier.id}>
                    {supplier.code} - {supplier.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Delivery Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('receiving.deliveryInfo', 'Delivery Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('receiving.carrierName', 'Carrier Name')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="UPS, FedEx, DHL..."
                {...register('carrierName')}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('receiving.trackingNumber', 'Tracking Number')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="1Z999AA10123456784"
                {...register('trackingNumber')}
              />
            </div>
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('receiving.deliveryNote', 'Delivery Note')}</label>
              <textarea
                className="form-field__textarea"
                rows={2}
                placeholder={t('receiving.deliveryNotePlaceholder', 'Delivery note or reference...')}
                {...register('deliveryNote')}
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
                placeholder={t('receiving.notesPlaceholder', 'Internal notes...')}
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
              : t('receiving.createReceipt', 'Create Receipt')}
        </button>
      </div>
    </form>
  );
}
