import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { useGetZoneByIdQuery, useUpdateZoneMutation, useDeleteZoneMutation } from '@/api/modules/zones';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import { WAREHOUSE } from '@/constants/routes';
import { ZoneForm, type ZoneFormData } from './ZoneForm';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import './Zones.scss';

export function ZoneDetails() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // tenantId is automatically injected by baseApi
  const { data: zoneResponse, isLoading: isLoadingZone } = useGetZoneByIdQuery(
    id || '',
    { skip: !id }
  );

  const { data: warehousesResponse } = useGetWarehouseOptionsQuery();

  const [updateZone, { isLoading: isUpdating }] = useUpdateZoneMutation();
  const [deleteZone, { isLoading: isDeleting }] = useDeleteZoneMutation();

  const zone = zoneResponse?.data;
  const warehouses = warehousesResponse?.data || [];

  const handleBack = () => {
    navigate(WAREHOUSE.ZONES);
  };

  const handleSubmit = async (data: ZoneFormData) => {
    if (!id) return;

    try {
      await updateZone({
        id,
        data: {
          name: data.name,
          description: data.description || undefined,
          zoneType: data.zoneType,
          pickSequence: data.pickSequence || undefined,
          isActive: data.isActive,
        },
      }).unwrap();
      navigate(WAREHOUSE.ZONES);
    } catch (error) {
      console.error('Failed to update zone:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteZone(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(WAREHOUSE.ZONES);
    } catch (error) {
      console.error('Failed to delete zone:', error);
    }
  };

  if (isLoadingZone) {
    return (
      <div className="zone-details">
        <div className="zone-details__loading">Loading...</div>
      </div>
    );
  }

  if (!zone) {
    return (
      <div className="zone-details">
        <div className="zone-details__not-found">
          <h2>Zone not found</h2>
          <button className="btn btn-secondary" onClick={handleBack}>
            Back to Zones
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="zone-details">
      {/* Header with back button */}
      <header className="zone-details__header">
        <div className="zone-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="zone-details__title-section">
            <h1 className="zone-details__title">{zone.name}</h1>
            <span className={`zone-type-badge zone-type-badge--${zone.zoneType.toLowerCase()}`}>
              {zone.zoneType}
            </span>
            <span className={`status-badge status-badge--${zone.isActive ? 'active' : 'inactive'}`}>
              {zone.isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
            </span>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="zone-details__content">
        <div className="zone-details__form-container">
          <ZoneForm
            initialData={{
              code: zone.code,
              name: zone.name,
              description: zone.description || '',
              warehouseId: zone.warehouseId,
              zoneType: zone.zoneType,
              pickSequence: zone.pickSequence ?? null,
              isActive: zone.isActive,
            }}
            warehouses={warehouses}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode
          />
        </div>

        {/* Actions sidebar */}
        <aside className="zone-details__sidebar">
          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('zone.statistics', 'Statistics')}</h3>
            <div className="sidebar-section__content">
              <dl className="info-list">
                <div className="info-list__item">
                  <dt>{t('zone.warehouse', 'Warehouse')}</dt>
                  <dd>{zone.warehouseName}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('zone.locations', 'Locations')}</dt>
                  <dd>{zone.locationCount}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('zone.pickSequence', 'Pick Sequence')}</dt>
                  <dd>{zone.pickSequence ?? '—'}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('zone.createdAt', 'Created')}</dt>
                  <dd>{new Date(zone.createdAt).toLocaleDateString()}</dd>
                </div>
              </dl>
            </div>
          </section>

          <section className="sidebar-section sidebar-section--danger">
            <h3 className="sidebar-section__title">{t('common.dangerZone', 'Danger Zone')}</h3>
            <div className="sidebar-section__content">
              <p className="sidebar-section__text">
                {t('zone.deleteWarning', 'Deleting this zone will remove all associated locations.')}
              </p>
              <button
                className="btn btn-danger btn-block"
                onClick={() => setDeleteConfirmOpen(true)}
                disabled={isDeleting || zone.locationCount > 0}
              >
                {t('zone.deleteZone', 'Delete Zone')}
              </button>
              {zone.locationCount > 0 && (
                <p className="sidebar-section__text" style={{ marginTop: '8px', fontSize: '0.75rem' }}>
                  {t('zone.cannotDeleteWithLocations', 'Cannot delete zone with locations.')}
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
        title={t('zone.confirmDelete', 'Confirm Delete')}
        subtitle={zone.name}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title="">
          <p>{t('zone.deleteConfirmText', 'Are you sure you want to delete this zone? This action cannot be undone.')}</p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default ZoneDetails;
