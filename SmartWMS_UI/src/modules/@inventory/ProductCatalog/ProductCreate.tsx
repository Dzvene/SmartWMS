import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { INVENTORY } from '@/constants/routes';
import { ProductForm, type ProductFormData } from './ProductForm';
import { useCreateProductMutation } from '@/api/modules/products';
import './ProductCatalog.scss';

/**
 * ProductCreate - Create new product
 */
export function ProductCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createProduct, { isLoading }] = useCreateProductMutation();

  const handleBack = () => {
    navigate(INVENTORY.SKU_CATALOG);
  };

  const handleSubmit = async (data: ProductFormData) => {
    try {
      await createProduct({
        sku: data.sku,
        name: data.name,
        description: data.description || undefined,
        categoryId: data.categoryId || undefined,
        barcode: data.barcode || undefined,
        unitOfMeasure: data.unitOfMeasure,
        unitsPerCase: data.unitsPerCase ?? undefined,
        casesPerPallet: data.casesPerPallet ?? undefined,
        widthMm: data.widthMm ?? undefined,
        heightMm: data.heightMm ?? undefined,
        depthMm: data.depthMm ?? undefined,
        grossWeightKg: data.grossWeightKg ?? undefined,
        netWeightKg: data.netWeightKg ?? undefined,
        isBatchTracked: data.isBatchTracked,
        isSerialTracked: data.isSerialTracked,
        hasExpiryDate: data.hasExpiryDate,
        minStockLevel: data.minStockLevel ?? undefined,
        maxStockLevel: data.maxStockLevel ?? undefined,
        reorderPoint: data.reorderPoint ?? undefined,
        isActive: data.isActive,
      }).unwrap();
      navigate(INVENTORY.SKU_CATALOG);
    } catch (error) {
      console.error('Failed to create product:', error);
    }
  };

  return (
    <div className="product-details">
      {/* Header with back button */}
      <header className="product-details__header">
        <div className="product-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="product-details__title-section">
            <h1 className="product-details__title">{t('products.createProduct', 'Create Product')}</h1>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="product-details__content">
        <div className="product-details__form-container product-details__form-container--full">
          <ProductForm
            onSubmit={handleSubmit}
            loading={isLoading}
          />
        </div>
      </div>
    </div>
  );
}

export default ProductCreate;
