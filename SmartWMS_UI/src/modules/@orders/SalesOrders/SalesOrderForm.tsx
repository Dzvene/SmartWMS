import { useEffect, useMemo, useState } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetCustomersQuery } from '@/api/modules/orders';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import { useGetProductsQuery } from '@/api/modules/products';
import type { OrderPriority, SalesOrderDto } from '@/api/modules/orders/orders.types';
import './SalesOrderForm.scss';

export interface SalesOrderLineFormData {
  productId: string;
  quantityOrdered: number;
  requiredBatchNumber?: string;
  requiredExpiryDate?: string;
  notes?: string;
}

export interface SalesOrderFormData {
  customerId: string;
  warehouseId: string;
  priority: OrderPriority;
  requiredDate?: string;
  externalReference?: string;
  shipToName?: string;
  shipToAddressLine1?: string;
  shipToAddressLine2?: string;
  shipToCity?: string;
  shipToRegion?: string;
  shipToPostalCode?: string;
  shipToCountryCode?: string;
  carrierCode?: string;
  serviceLevel?: string;
  shippingInstructions?: string;
  notes?: string;
  internalNotes?: string;
  lines: SalesOrderLineFormData[];
}

interface SalesOrderFormProps {
  initialData?: SalesOrderDto;
  onSubmit: (data: SalesOrderFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const PRIORITY_OPTIONS: OrderPriority[] = ['Low', 'Normal', 'High', 'Urgent'];

export function SalesOrderForm({ initialData, onSubmit, loading, isEditMode }: SalesOrderFormProps) {
  const t = useTranslate();

  const [useCustomAddress, setUseCustomAddress] = useState(false);

  // Fetch customers
  const { data: customersResponse } = useGetCustomersQuery({ pageSize: 100, isActive: true });
  const customers = useMemo(() => customersResponse?.data?.items || [], [customersResponse?.data?.items]);

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
    watch,
    setValue,
    control,
    formState: { errors, isDirty },
  } = useForm<SalesOrderFormData>({
    defaultValues: {
      customerId: '',
      warehouseId: '',
      priority: 'Normal',
      requiredDate: '',
      externalReference: '',
      shipToName: '',
      shipToAddressLine1: '',
      shipToAddressLine2: '',
      shipToCity: '',
      shipToRegion: '',
      shipToPostalCode: '',
      shipToCountryCode: '',
      carrierCode: '',
      serviceLevel: '',
      shippingInstructions: '',
      notes: '',
      internalNotes: '',
      lines: [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'lines',
  });

  const selectedCustomerId = watch('customerId');

  // Auto-fill shipping address from customer
  const selectedCustomer = useMemo(() => {
    return customers.find((c) => c.id === selectedCustomerId);
  }, [customers, selectedCustomerId]);

  useEffect(() => {
    if (selectedCustomer && !useCustomAddress && !isEditMode) {
      setValue('shipToName', selectedCustomer.name);
      setValue('shipToAddressLine1', selectedCustomer.addressLine1 || '');
      setValue('shipToAddressLine2', selectedCustomer.addressLine2 || '');
      setValue('shipToCity', selectedCustomer.city || '');
      setValue('shipToRegion', selectedCustomer.region || '');
      setValue('shipToPostalCode', selectedCustomer.postalCode || '');
      setValue('shipToCountryCode', selectedCustomer.countryCode || '');
    }
  }, [selectedCustomer, useCustomAddress, isEditMode, setValue]);

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        customerId: initialData.customerId || '',
        warehouseId: initialData.warehouseId || '',
        priority: initialData.priority || 'Normal',
        requiredDate: initialData.requiredDate?.split('T')[0] || '',
        externalReference: initialData.externalReference || '',
        shipToName: initialData.shipToName || '',
        shipToAddressLine1: initialData.shipToAddressLine1 || '',
        shipToAddressLine2: initialData.shipToAddressLine2 || '',
        shipToCity: initialData.shipToCity || '',
        shipToRegion: initialData.shipToRegion || '',
        shipToPostalCode: initialData.shipToPostalCode || '',
        shipToCountryCode: initialData.shipToCountryCode || '',
        carrierCode: initialData.carrierCode || '',
        serviceLevel: initialData.serviceLevel || '',
        shippingInstructions: initialData.shippingInstructions || '',
        notes: initialData.notes || '',
        internalNotes: initialData.internalNotes || '',
        lines: initialData.lines?.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          requiredBatchNumber: line.requiredBatchNumber || '',
          requiredExpiryDate: line.requiredExpiryDate?.split('T')[0] || '',
          notes: line.notes || '',
        })) || [],
      });
      setUseCustomAddress(true);
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  const addLine = () => {
    append({
      productId: '',
      quantityOrdered: 1,
      requiredBatchNumber: '',
      requiredExpiryDate: '',
      notes: '',
    });
  };

  return (
    <form className="sales-order-form" onSubmit={handleFormSubmit}>
      {/* Section: Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('orders.basicInfo', 'Basic Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            {/* Customer */}
            <div className="form-field">
              <label className="form-field__label">
                {t('orders.customer', 'Customer')} <span className="required">*</span>
              </label>
              <select
                className={`form-field__select ${errors.customerId ? 'form-field__select--error' : ''}`}
                {...register('customerId', { required: t('orders.customerRequired', 'Customer is required') })}
                disabled={isEditMode}
              >
                <option value="">{t('orders.selectCustomer', 'Select customer...')}</option>
                {customers.map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.code} - {customer.name}
                  </option>
                ))}
              </select>
              {errors.customerId && <span className="form-field__error">{errors.customerId.message}</span>}
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

            {/* Priority */}
            <div className="form-field">
              <label className="form-field__label">{t('orders.priority', 'Priority')}</label>
              <select className="form-field__select" {...register('priority')}>
                {PRIORITY_OPTIONS.map((priority) => (
                  <option key={priority} value={priority}>
                    {t(`orders.priority.${priority.toLowerCase()}`, priority)}
                  </option>
                ))}
              </select>
            </div>

            {/* Required Date */}
            <div className="form-field">
              <label className="form-field__label">{t('orders.requiredDate', 'Required Date')}</label>
              <input type="date" className="form-field__input" {...register('requiredDate')} />
            </div>

            {/* External Reference */}
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('orders.externalReference', 'External Reference')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder={t('orders.externalReferencePlaceholder', 'PO number, customer order number, etc.')}
                {...register('externalReference')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Shipping Address */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('orders.shippingAddress', 'Shipping Address')}</h3>
          {!isEditMode && selectedCustomer && (
            <label className="form-checkbox">
              <input
                type="checkbox"
                checked={useCustomAddress}
                onChange={(e) => setUseCustomAddress(e.target.checked)}
              />
              <span>{t('orders.useCustomAddress', 'Use custom address')}</span>
            </label>
          )}
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('orders.shipToName', 'Ship To Name')}</label>
              <input type="text" className="form-field__input" {...register('shipToName')} />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('orders.addressLine1', 'Address Line 1')}</label>
              <input type="text" className="form-field__input" {...register('shipToAddressLine1')} />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('orders.addressLine2', 'Address Line 2')}</label>
              <input type="text" className="form-field__input" {...register('shipToAddressLine2')} />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('orders.city', 'City')}</label>
              <input type="text" className="form-field__input" {...register('shipToCity')} />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('orders.region', 'Region/State')}</label>
              <input type="text" className="form-field__input" {...register('shipToRegion')} />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('orders.postalCode', 'Postal Code')}</label>
              <input type="text" className="form-field__input" {...register('shipToPostalCode')} />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('orders.country', 'Country')}</label>
              <input type="text" className="form-field__input" {...register('shipToCountryCode')} />
            </div>
          </div>
        </div>
      </section>

      {/* Section: Shipping Options */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('orders.shippingOptions', 'Shipping Options')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('orders.carrier', 'Carrier')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder={t('orders.carrierPlaceholder', 'e.g., FedEx, UPS, DHL')}
                {...register('carrierCode')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('orders.serviceLevel', 'Service Level')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder={t('orders.serviceLevelPlaceholder', 'e.g., Ground, Express, Overnight')}
                {...register('serviceLevel')}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('orders.shippingInstructions', 'Shipping Instructions')}</label>
              <textarea
                className="form-field__textarea"
                rows={2}
                {...register('shippingInstructions')}
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
                <div className="order-lines__col order-lines__col--batch">{t('orders.batch', 'Batch')}</div>
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
                      {...register(`lines.${index}.requiredBatchNumber`)}
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
              <label className="form-field__label">{t('orders.customerNotes', 'Customer Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('orders.customerNotesPlaceholder', 'Notes visible to customer')}
                {...register('notes')}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('orders.internalNotes', 'Internal Notes')}</label>
              <textarea
                className="form-field__textarea"
                rows={3}
                placeholder={t('orders.internalNotesPlaceholder', 'Internal notes (not visible to customer)')}
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
