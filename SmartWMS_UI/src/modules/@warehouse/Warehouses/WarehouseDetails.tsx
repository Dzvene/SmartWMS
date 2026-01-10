import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { WAREHOUSE } from '@/constants/routes';
import { WarehouseForm, type WarehouseFormData } from './WarehouseForm';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import {
  useGetWarehouseByIdQuery,
  useUpdateWarehouseMutation,
  useDeleteWarehouseMutation,
} from '@/api/modules/warehouses';

export function WarehouseDetails() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  const { data, isLoading: isLoadingWarehouse } = useGetWarehouseByIdQuery(id!, {
    skip: !id,
  });
  const [updateWarehouse, { isLoading: isUpdating }] = useUpdateWarehouseMutation();
  const [deleteWarehouse, { isLoading: isDeleting }] = useDeleteWarehouseMutation();

  const warehouse = data?.data;

  const handleBack = () => {
    navigate(WAREHOUSE.WAREHOUSES);
  };

  const handleSubmit = async (formData: WarehouseFormData) => {
    if (!id) return;
    try {
      await updateWarehouse({
        id,
        data: {
          name: formData.name,
          description: formData.description || undefined,
          addressLine1: formData.addressLine1 || undefined,
          addressLine2: formData.addressLine2 || undefined,
          city: formData.city || undefined,
          region: formData.region || undefined,
          postalCode: formData.postalCode || undefined,
          countryCode: formData.countryCode || undefined,
          timezone: formData.timezone || undefined,
          isPrimary: formData.isPrimary,
          isActive: formData.isActive,
        },
      }).unwrap();
      navigate(WAREHOUSE.WAREHOUSES);
    } catch (error) {
      console.error('Failed to update warehouse:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;
    try {
      await deleteWarehouse(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(WAREHOUSE.WAREHOUSES);
    } catch (error) {
      console.error('Failed to delete warehouse:', error);
    }
  };

  if (isLoadingWarehouse) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!warehouse) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          <h2>{t('warehouse.notFound', 'Warehouse not found')}</h2>
          <button className="btn btn-secondary" onClick={handleBack}>
            {t('warehouse.backToList', 'Back to Warehouses')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="detail-page">
      <header className="detail-page__header">
        <div className="detail-page__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="detail-page__title-section">
            <h1 className="detail-page__title">{warehouse.name}</h1>
            <div className="detail-page__meta">
              {warehouse.isPrimary && (
                <span className="status-badge status-badge--primary">{t('warehouse.primary', 'Primary')}</span>
              )}
              <span className={`status-badge status-badge--${warehouse.isActive ? 'success' : 'neutral'}`}>
                {warehouse.isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
              </span>
            </div>
          </div>
        </div>
      </header>

      <div className="detail-page__content detail-page__content--with-sidebar">
        <div className="detail-page__main">
          <WarehouseForm
            initialData={{
              code: warehouse.code,
              name: warehouse.name,
              description: warehouse.description || '',
              addressLine1: warehouse.addressLine1 || '',
              addressLine2: warehouse.addressLine2 || '',
              city: warehouse.city || '',
              region: warehouse.region || '',
              postalCode: warehouse.postalCode || '',
              countryCode: warehouse.countryCode || '',
              timezone: warehouse.timezone || '',
              isPrimary: warehouse.isPrimary,
              isActive: warehouse.isActive,
            }}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode
          />
        </div>

        <aside className="detail-page__sidebar">
          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('warehouse.statistics', 'Statistics')}</h3>
            <div className="sidebar-section__content">
              <dl className="info-list">
                <div className="info-list__item">
                  <dt>{t('warehouse.zones', 'Zones')}</dt>
                  <dd>{warehouse.zoneCount}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('warehouse.locations', 'Locations')}</dt>
                  <dd>{warehouse.locationCount}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('warehouse.createdAt', 'Created')}</dt>
                  <dd>{new Date(warehouse.createdAt).toLocaleDateString()}</dd>
                </div>
              </dl>
            </div>
          </section>

          <section className="sidebar-section sidebar-section--danger">
            <h3 className="sidebar-section__title">{t('common.dangerZone', 'Danger Zone')}</h3>
            <div className="sidebar-section__content">
              <p className="sidebar-section__text">
                {t('warehouse.deleteWarning', 'Deleting this warehouse will remove all associated zones and locations.')}
              </p>
              <button
                className="btn btn-danger btn-block"
                onClick={() => setDeleteConfirmOpen(true)}
                disabled={isDeleting || warehouse.isPrimary}
              >
                {t('warehouse.deleteWarehouse', 'Delete Warehouse')}
              </button>
              {warehouse.isPrimary && (
                <p className="sidebar-section__hint">
                  {t('warehouse.cannotDeletePrimary', 'Primary warehouse cannot be deleted.')}
                </p>
              )}
            </div>
          </section>
        </aside>
      </div>

      <FullscreenModal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('warehouse.confirmDelete', 'Confirm Delete')}
        subtitle={warehouse.name}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title="">
          <p>{t('warehouse.deleteConfirmText', 'Are you sure you want to delete this warehouse? This action cannot be undone.')}</p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default WarehouseDetails;
