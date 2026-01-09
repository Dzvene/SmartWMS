import { useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm, useFieldArray } from 'react-hook-form';
import { useGetSuppliersQuery } from '@/api/modules/orders';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import { useGetProductsQuery } from '@/api/modules/products';
import type { PurchaseOrderDto } from '@/api/modules/orders/orders.types';
import './PurchaseOrderForm.scss';

export interface PurchaseOrderLineFormData {
  productId: string;
  quantityOrdered: number;
  expectedBatchNumber?: string;
  expectedExpiryDate?: string;
  notes?: string;
}

export interface PurchaseOrderFormData {
  supplierId: string;
  warehouseId: string;
  expectedDate?: string;
  externalReference?: string;
  receivingDockId?: string;
  notes?: string;
  internalNotes?: string;
  lines: PurchaseOrderLineFormData[];
}

interface PurchaseOrderFormProps {
  initialData?: PurchaseOrderDto;
  onSubmit: (data: PurchaseOrderFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

export function PurchaseOrderForm({ initialData, onSubmit, loading, isEditMode }: PurchaseOrderFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  // Fetch suppliers
  const { data: suppliersResponse } = useGetSuppliersQuery({ pageSize: 100, isActive: true });
  const suppliers = suppliersResponse?.data?.items || [];

  // Fetch warehouses
  const { data: warehousesResponse } = useGetWarehousesQuery({ pageSize: 100 });
  const warehouses = warehousesResponse?.data?.items || [];

  // Fetch products for line items
  const { data: productsResponse } = useGetProductsQuery({ pageSize: 500, isActive: true });
  const products = productsResponse?.data?.items || [];

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors, isDirty },
  } = useForm<PurchaseOrderFormData>({
    defaultValues: {
      supplierId: '',
      warehouseId: '',
      expectedDate: '',
      externalReference: '',
      receivingDockId: '',
      notes: '',
      internalNotes: '',
      lines: [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'lines',
  });

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        supplierId: initialData.supplierId || '',
        warehouseId: initialData.warehouseId || '',
        expectedDate: initialData.expectedDate?.split('T')[0] || '',
        externalReference: initialData.externalReference || '',
        receivingDockId: initialData.receivingDockId || '',
        notes: initialData.notes || '',
        internalNotes: initialData.internalNotes || '',
        lines: initialData.lines?.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          expectedBatchNumber: line.expectedBatchNumber || '',
          expectedExpiryDate: line.expectedExpiryDate?.split('T')[0] || '',
          notes: line.notes || '',
        })) || [],
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  const addLine = () => {
    append({
      productId: '',
      quantityOrdered: 1,
      expectedBatchNumber: '',
      expectedExpiryDate: '',
      notes: '',
    });
  };

  return (
    <form className="purchase-order-form" onSubmit={handleFormSubmit}>
      {/* Section: Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('orders.basicInfo', 'Basic Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            {/* Supplier */}
            <div className="form-field">
              <label className="form-field__label">
                {t('orders.supplier', 'Supplier')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.supplierId ? 'form-field__select--error' : ''}`}
                {...register('supplierId', { required: t('orders.supplierRequired', 'Supplier is required') })}
                disabled={isEditMode}
              >
                <option value="">{t('orders.selectSupplier', 'Select supplier...')}</option>
                {suppliers.map((supplier) => (
                  <option key={supplier.id} value={supplier.id}>
                    {supplier.code} - {supplier.name}
                  </option>
                ))}
              </select>
              {errors.supplierId && <span className="form-field__error">{errors.supplierId.message}</span>}
            </div>

            {/* Warehouse */}
            <div className="form-field">
              <label className="form-field__label">
                {t('orders.warehouse', 'Warehouse')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.warehouseId ? 'form-field__select--error' : ''}`}
                {...register('warehouseId', { required: t('orders.warehouseRequired', 'Warehouse is required') })}
              >
                <option value="">{t('orders.selectWarehouse', 'Select warehouse...')}</option>
                {warehouses.map((warehouse) => (
                  <option key={warehouse.id} value={warehouse.id}>
                    {warehouse.code} - {warehouse.name}
                  </option>
                ))}
              </select>
              {errors.warehouseId && <span className="form-field__error">{errors.warehouseId.message}</span>}
            </div>

            {/* Expected Date */}
            <div className="form-field">
              <label className="form-field__label">{t('orders.expectedDate', 'Expected Date')}</label>
              <input type="date" className="form-field__input" {...register('expectedDate')} />
            </div>

            {/* External Reference */}
            <div className="form-field">
              <label className="form-field__label">{t('orders.externalReference', 'External Reference')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder={t('orders.externalReferencePlaceholder', 'Supplier PO number, etc.')}
                {...register('externalReference')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Order Lines */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">
            {t('orders.orderLines', 'Order Lines')} ({fields.length})
          </h3>
          <button type="button" className="btn btn-secondary btn-sm" onClick={addLine}>
            + {t('orders.addLine', 'Add Line')}
          </button>
        </div>
        <div className="form-section__content">
          {fields.length === 0 ? (
            <div className="empty-lines">
              <p>{t('orders.noLines', 'No order lines yet. Click "Add Line" to add products.')}</p>
            </div>
          ) : (
            <div className="order-lines">
              <div className="order-lines__header">
                <div className="order-lines__col order-lines__col--product">{t('orders.product', 'Product')}</div>
                <div className="order-lines__col order-lines__col--qty">{t('orders.quantity', 'Qty')}</div>
                <div className="order-lines__col order-lines__col--batch">{t('orders.expectedBatch', 'Expected Batch')}</div>
                <div className="order-lines__col order-lines__col--actions"></div>
              </div>
              {fields.map((field, index) => (
                <div key={field.id} className="order-lines__row">
                  <div className="order-lines__col order-lines__col--product">
                    <select
                      className={`form-field__select ${errors.lines?.[index]?.productId ? 'form-field__select--error' : ''}`}
                      {...register(`lines.${index}.productId`, { required: 'Product required' })}
                    >
                      <option value="">{t('orders.selectProduct', 'Select product...')}</option>
                      {products.map((product) => (
                        <option key={product.id} value={product.id}>
                          {product.sku} - {product.name}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="order-lines__col order-lines__col--qty">
                    <input
                      type="number"
                      min="1"
                      className={`form-field__input ${errors.lines?.[index]?.quantityOrdered ? 'form-field__input--error' : ''}`}
                      {...register(`lines.${index}.quantityOrdered`, {
                        required: 'Qty required',
                        min: { value: 1, message: 'Min 1' },
                        valueAsNumber: true,
                      })}
                    />
                  </div>
                  <div className="order-lines__col order-lines__col--batch">
                    <input
                      type="text"
                      className="form-field__input"
                      placeholder={t('orders.batchOptional', 'Optional')}
                      {...register(`lines.${index}.expectedBatchNumber`)}
                    />
                  </div>
                  <div className="order-lines__col order-lines__col--actions">
                    <button
                      type="button"
                      className="btn btn-ghost btn-sm btn-danger"
                      onClick={() => remove(index)}
                    >
                      &times;
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </section>

      {/* Section: Notes */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('orders.notes', 'Notes')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('orders.supplierNotes', 'Supplier Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('orders.supplierNotesPlaceholder', 'Notes for supplier')}
                {...register('notes')}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('orders.internalNotes', 'Internal Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('orders.internalNotesPlaceholder', 'Internal notes')}
                {...register('internalNotes')}
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
              : t('orders.createOrder', 'Create Order')}
        </button>
      </div>
    </form>
  );
}
