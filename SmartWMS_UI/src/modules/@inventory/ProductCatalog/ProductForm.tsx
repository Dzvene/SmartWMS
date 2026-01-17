import { useEffect, useMemo } from 'react';
import { useForm, useWatch } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetProductCategoriesQuery, ProductCategoryResponse } from '@/api/modules/products';

export interface ProductFormData {
  sku: string;
  name: string;
  description: string;
  categoryId: string;
  barcode: string;
  unitOfMeasure: string;
  unitsPerCase: number | null;
  casesPerPallet: number | null;
  widthMm: number | null;
  heightMm: number | null;
  depthMm: number | null;
  grossWeightKg: number | null;
  netWeightKg: number | null;
  isBatchTracked: boolean;
  isSerialTracked: boolean;
  hasExpiryDate: boolean;
  minStockLevel: number | null;
  maxStockLevel: number | null;
  reorderPoint: number | null;
  isActive: boolean;
}

interface ProductFormProps {
  initialData?: Partial<ProductFormData>;
  onSubmit: (data: ProductFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

const defaultValues: ProductFormData = {
  sku: '',
  name: '',
  description: '',
  categoryId: '',
  barcode: '',
  unitOfMeasure: 'EA',
  unitsPerCase: null,
  casesPerPallet: null,
  widthMm: null,
  heightMm: null,
  depthMm: null,
  grossWeightKg: null,
  netWeightKg: null,
  isBatchTracked: false,
  isSerialTracked: false,
  hasExpiryDate: false,
  minStockLevel: null,
  maxStockLevel: null,
  reorderPoint: null,
  isActive: true,
};

export function ProductForm({ initialData, onSubmit, loading, isEditMode }: ProductFormProps) {
  const t = useTranslate();

  const { data: categoriesResponse } = useGetProductCategoriesQuery({
    pageSize: 100,
    isActive: true,
  });

  const categories = useMemo(() => categoriesResponse?.data?.items || [], [categoriesResponse?.data?.items]);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    control,
    formState: { errors, isDirty },
  } = useForm<ProductFormData>({
    defaultValues: { ...defaultValues, ...initialData },
  });

  // Watch category changes to apply defaults
  const selectedCategoryId = useWatch({ control, name: 'categoryId' });

  // Build a map of categories for quick lookup
  const categoriesMap = useMemo(() => {
    const map = new Map<string, ProductCategoryResponse>();
    categories.forEach((cat) => {
      map.set(cat.id, cat);
    });
    return map;
  }, [categories]);

  // Apply category defaults when category changes (only in create mode)
  useEffect(() => {
    if (isEditMode || !selectedCategoryId) {
      return;
    }

    const category = categoriesMap.get(selectedCategoryId);
    if (!category) {
      return;
    }

    // Apply default unit of measure if set
    if (category.defaultUnitOfMeasure) {
      setValue('unitOfMeasure', category.defaultUnitOfMeasure, { shouldDirty: true });
    }

    // Apply tracking requirements - these are enforced by category
    if (category.requiresBatchTracking) {
      setValue('isBatchTracked', true, { shouldDirty: true });
    }

    if (category.requiresSerialTracking) {
      setValue('isSerialTracked', true, { shouldDirty: true });
    }

    if (category.requiresExpiryDate) {
      setValue('hasExpiryDate', true, { shouldDirty: true });
    }
  }, [selectedCategoryId, categoriesMap, isEditMode, setValue]);

  // Get selected category for displaying requirements info
  const selectedCategory = selectedCategoryId ? categoriesMap.get(selectedCategoryId) : null;

  useEffect(() => {
    if (initialData) {
      reset({ ...defaultValues, ...initialData });
    }
  }, [initialData, reset]);

  const onFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="product-form" onSubmit={onFormSubmit}>
      {/* Basic Info */}
      <section className="product-form__section">
        <h3 className="product-form__section-title">{t('products.basicInfo', 'Basic Information')}</h3>

        <div className="product-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('products.sku', 'SKU')} *</label>
            <input
              type="text"
              className={`form-field__input ${errors.sku ? 'form-field__input--error' : ''}`}
              placeholder="SKU-001"
              {...register('sku', { required: t('validation.required', 'Required') })}
              disabled={isEditMode}
            />
            {errors.sku && <span className="form-field__error">{errors.sku.message}</span>}
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.name', 'Name')} *</label>
            <input
              type="text"
              className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
              placeholder={t('products.namePlaceholder', 'Product name')}
              {...register('name', { required: t('validation.required', 'Required') })}
            />
            {errors.name && <span className="form-field__error">{errors.name.message}</span>}
          </div>
        </div>

        <div className="form-field">
          <label className="form-field__label">{t('common.description', 'Description')}</label>
          <textarea
            className="form-field__textarea"
            rows={3}
            placeholder={t('products.descriptionPlaceholder', 'Product description...')}
            {...register('description')}
          />
        </div>

        <div className="product-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('products.category', 'Category')}</label>
            <select className="form-field__select" {...register('categoryId')}>
              <option value="">{t('products.selectCategory', 'Select category')}</option>
              {categories.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.path ? `${cat.path} / ${cat.name}` : cat.name}
                </option>
              ))}
            </select>
            {/* Show category handling info */}
            {selectedCategory && (selectedCategory.isHazardous || selectedCategory.isFragile || selectedCategory.handlingInstructions) && (
              <div className="product-form__handling-info">
                {selectedCategory.isHazardous && (
                  <span className="product-form__handling-badge product-form__handling-badge--hazardous">
                    ⚠️ {t('products.hazardous', 'Hazardous')}
                  </span>
                )}
                {selectedCategory.isFragile && (
                  <span className="product-form__handling-badge product-form__handling-badge--fragile">
                    📦 {t('products.fragile', 'Fragile')}
                  </span>
                )}
                {selectedCategory.handlingInstructions && (
                  <span className="product-form__handling-instructions">
                    {selectedCategory.handlingInstructions}
                  </span>
                )}
              </div>
            )}
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.barcode', 'Barcode')}</label>
            <input
              type="text"
              className="form-field__input"
              placeholder="1234567890123"
              {...register('barcode')}
            />
          </div>
        </div>

        <div className="product-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('products.uom', 'Unit of Measure')} *</label>
            <select
              className={`form-field__select ${errors.unitOfMeasure ? 'form-field__select--error' : ''}`}
              {...register('unitOfMeasure', { required: t('validation.required', 'Required') })}
            >
              <option value="EA">EA (Each)</option>
              <option value="BOX">BOX (Box)</option>
              <option value="CASE">CASE (Case)</option>
              <option value="PALLET">PALLET (Pallet)</option>
              <option value="KG">KG (Kilogram)</option>
              <option value="LTR">LTR (Liter)</option>
              <option value="MTR">MTR (Meter)</option>
              <option value="SET">SET (Set)</option>
            </select>
          </div>

          <div className="form-field">
            <label className="form-field__label">&nbsp;</label>
            <div className="form-field__checkbox-group">
              <label className="form-field__checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('common.active', 'Active')}</span>
              </label>
            </div>
          </div>
        </div>
      </section>

      {/* Pack Configuration */}
      <section className="product-form__section">
        <h3 className="product-form__section-title">{t('products.packConfig', 'Pack Configuration')}</h3>

        <div className="product-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('products.unitsPerCase', 'Units per Case')}</label>
            <input
              type="number"
              className="form-field__input"
              placeholder="12"
              {...register('unitsPerCase', { valueAsNumber: true })}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.casesPerPallet', 'Cases per Pallet')}</label>
            <input
              type="number"
              className="form-field__input"
              placeholder="48"
              {...register('casesPerPallet', { valueAsNumber: true })}
            />
          </div>
        </div>
      </section>

      {/* Dimensions & Weight */}
      <section className="product-form__section">
        <h3 className="product-form__section-title">{t('products.dimensions', 'Dimensions & Weight')}</h3>

        <div className="product-form__row--three product-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('products.width', 'Width (mm)')}</label>
            <input
              type="number"
              className="form-field__input"
              placeholder="100"
              {...register('widthMm', { valueAsNumber: true })}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.height', 'Height (mm)')}</label>
            <input
              type="number"
              className="form-field__input"
              placeholder="50"
              {...register('heightMm', { valueAsNumber: true })}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.depth', 'Depth (mm)')}</label>
            <input
              type="number"
              className="form-field__input"
              placeholder="200"
              {...register('depthMm', { valueAsNumber: true })}
            />
          </div>
        </div>

        <div className="product-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('products.grossWeight', 'Gross Weight (kg)')}</label>
            <input
              type="number"
              step="0.001"
              className="form-field__input"
              placeholder="1.5"
              {...register('grossWeightKg', { valueAsNumber: true })}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.netWeight', 'Net Weight (kg)')}</label>
            <input
              type="number"
              step="0.001"
              className="form-field__input"
              placeholder="1.2"
              {...register('netWeightKg', { valueAsNumber: true })}
            />
          </div>
        </div>
      </section>

      {/* Tracking Requirements */}
      <section className="product-form__section">
        <h3 className="product-form__section-title">{t('products.tracking', 'Tracking Requirements')}</h3>

        {/* Show category requirements info */}
        {selectedCategory && (selectedCategory.requiresBatchTracking || selectedCategory.requiresSerialTracking || selectedCategory.requiresExpiryDate) && (
          <div className="product-form__category-info">
            <span className="product-form__category-info-icon">ℹ️</span>
            <span>
              {t('products.categoryRequires', 'Category requires:')}{' '}
              {[
                selectedCategory.requiresBatchTracking && t('products.batchTracking', 'Batch Tracking'),
                selectedCategory.requiresSerialTracking && t('products.serialTracking', 'Serial Tracking'),
                selectedCategory.requiresExpiryDate && t('products.expiryTracking', 'Expiry Date'),
              ].filter(Boolean).join(', ')}
            </span>
          </div>
        )}

        <div className="form-field__checkbox-group">
          <label className={`form-field__checkbox ${selectedCategory?.requiresBatchTracking ? 'form-field__checkbox--required' : ''}`}>
            <input
              type="checkbox"
              {...register('isBatchTracked')}
              disabled={selectedCategory?.requiresBatchTracking}
            />
            <span>{t('products.batchTracked', 'Batch/Lot Tracking')}</span>
            {selectedCategory?.requiresBatchTracking && (
              <span className="form-field__required-badge">{t('common.required', 'Required')}</span>
            )}
          </label>
          <label className={`form-field__checkbox ${selectedCategory?.requiresSerialTracking ? 'form-field__checkbox--required' : ''}`}>
            <input
              type="checkbox"
              {...register('isSerialTracked')}
              disabled={selectedCategory?.requiresSerialTracking}
            />
            <span>{t('products.serialTracked', 'Serial Number Tracking')}</span>
            {selectedCategory?.requiresSerialTracking && (
              <span className="form-field__required-badge">{t('common.required', 'Required')}</span>
            )}
          </label>
          <label className={`form-field__checkbox ${selectedCategory?.requiresExpiryDate ? 'form-field__checkbox--required' : ''}`}>
            <input
              type="checkbox"
              {...register('hasExpiryDate')}
              disabled={selectedCategory?.requiresExpiryDate}
            />
            <span>{t('products.expiryDate', 'Expiry Date Tracking')}</span>
            {selectedCategory?.requiresExpiryDate && (
              <span className="form-field__required-badge">{t('common.required', 'Required')}</span>
            )}
          </label>
        </div>
      </section>

      {/* Inventory Levels */}
      <section className="product-form__section">
        <h3 className="product-form__section-title">{t('products.inventoryLevels', 'Inventory Levels')}</h3>

        <div className="product-form__row--three product-form__row">
          <div className="form-field">
            <label className="form-field__label">{t('products.minStock', 'Min Stock')}</label>
            <input
              type="number"
              step="0.01"
              className="form-field__input"
              placeholder="10"
              {...register('minStockLevel', { valueAsNumber: true })}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.maxStock', 'Max Stock')}</label>
            <input
              type="number"
              step="0.01"
              className="form-field__input"
              placeholder="1000"
              {...register('maxStockLevel', { valueAsNumber: true })}
            />
          </div>

          <div className="form-field">
            <label className="form-field__label">{t('products.reorderPoint', 'Reorder Point')}</label>
            <input
              type="number"
              step="0.01"
              className="form-field__input"
              placeholder="50"
              {...register('reorderPoint', { valueAsNumber: true })}
            />
          </div>
        </div>
      </section>

      {/* Actions */}
      <div className="product-form__actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || (!isDirty && isEditMode)}
        >
          {loading ? t('common.saving', 'Saving...') : isEditMode ? t('common.save', 'Save') : t('common.create', 'Create')}
        </button>
      </div>
    </form>
  );
}

export default ProductForm;
