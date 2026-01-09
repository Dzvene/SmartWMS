import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { INVENTORY } from '@/constants/routes';
import { ProductForm, type ProductFormData } from './ProductForm';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import {
  useGetProductByIdQuery,
  useUpdateProductMutation,
  useDeleteProductMutation,
} from '@/api/modules/products';
import './ProductCatalog.scss';

/**
 * ProductDetails - Edit existing product
 */
export function ProductDetails() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  const { data: productResponse, isLoading: isLoadingProduct } = useGetProductByIdQuery(
    id!,
    { skip: !id }
  );

  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();
  const [deleteProduct, { isLoading: isDeleting }] = useDeleteProductMutation();

  const product = productResponse?.data;

  const handleBack = () => {
    navigate(INVENTORY.SKU_CATALOG);
  };

  const handleSubmit = async (data: ProductFormData) => {
    if (!id) return;

    try {
      await updateProduct({
        id,
        data: {
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
        },
      }).unwrap();
      navigate(INVENTORY.SKU_CATALOG);
    } catch (error) {
      console.error('Failed to update product:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteProduct(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(INVENTORY.SKU_CATALOG);
    } catch (error) {
      console.error('Failed to delete product:', error);
    }
  };

  if (isLoadingProduct) {
    return (
      <div className="product-details">
        <div className="product-details__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="product-details">
        <div className="product-details__not-found">
          <h2>{t('products.notFound', 'Product not found')}</h2>
          <button className="btn btn-secondary" onClick={handleBack}>
            {t('products.backToList', 'Back to Products')}
          </button>
        </div>
      </div>
    );
  }

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
            <h1 className="product-details__title">{product.name}</h1>
            <span className="product-details__sku">{product.sku}</span>
            <span className={`status-badge status-badge--${product.isActive ? 'active' : 'inactive'}`}>
              {product.isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
            </span>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="product-details__content">
        <div className="product-details__form-container">
          <ProductForm
            initialData={{
              sku: product.sku,
              name: product.name,
              description: product.description || '',
              categoryId: product.categoryId || '',
              barcode: product.barcode || '',
              unitOfMeasure: product.unitOfMeasure,
              unitsPerCase: product.unitsPerCase ?? null,
              casesPerPallet: product.casesPerPallet ?? null,
              widthMm: product.widthMm ?? null,
              heightMm: product.heightMm ?? null,
              depthMm: product.depthMm ?? null,
              grossWeightKg: product.grossWeightKg ?? null,
              netWeightKg: product.netWeightKg ?? null,
              isBatchTracked: product.isBatchTracked,
              isSerialTracked: product.isSerialTracked,
              hasExpiryDate: product.hasExpiryDate,
              minStockLevel: product.minStockLevel ?? null,
              maxStockLevel: product.maxStockLevel ?? null,
              reorderPoint: product.reorderPoint ?? null,
              isActive: product.isActive,
            }}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode
          />
        </div>

        {/* Actions sidebar */}
        <aside className="product-details__sidebar">
          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('products.statistics', 'Stock Information')}</h3>
            <div className="sidebar-section__content">
              <dl className="info-list">
                <div className="info-list__item">
                  <dt>{t('products.locations', 'Locations')}</dt>
                  <dd>{product.stockLevelCount}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('products.totalOnHand', 'Total On Hand')}</dt>
                  <dd>{product.totalOnHand?.toLocaleString() ?? '-'}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('products.category', 'Category')}</dt>
                  <dd>{product.categoryName || '-'}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('common.created', 'Created')}</dt>
                  <dd>{new Date(product.createdAt).toLocaleDateString()}</dd>
                </div>
              </dl>
            </div>
          </section>

          <section className="sidebar-section sidebar-section--danger">
            <h3 className="sidebar-section__title">{t('common.dangerZone', 'Danger Zone')}</h3>
            <div className="sidebar-section__content">
              <p className="sidebar-section__text">
                {t('products.deleteWarning', 'Deleting this product will remove it from the catalog. Stock must be removed first.')}
              </p>
              <button
                className="btn btn-danger btn-block"
                onClick={() => setDeleteConfirmOpen(true)}
                disabled={isDeleting || (product.stockLevelCount > 0)}
              >
                {t('products.deleteProduct', 'Delete Product')}
              </button>
              {product.stockLevelCount > 0 && (
                <p className="sidebar-section__text" style={{ marginTop: '8px', fontSize: '0.75rem' }}>
                  {t('products.cannotDeleteWithStock', 'Product with stock cannot be deleted.')}
                </p>
              )}
            </div>
          </section>
        </aside>
      </div>

      {/* Delete Confirmation Modal */}
      <FullscreenModal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('products.confirmDelete', 'Confirm Delete')}
        subtitle={product.name}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title="">
          <p>{t('products.deleteConfirmText', 'Are you sure you want to delete this product? This action cannot be undone.')}</p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default ProductDetails;
