import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import {
  useGetLocationByIdQuery,
  useUpdateLocationMutation,
  useDeleteLocationMutation,
} from '@/api/modules/locations';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import { useGetZonesQuery } from '@/api/modules/zones';
import { WAREHOUSE } from '@/constants/routes';
import { LocationForm, type LocationFormData } from './LocationForm';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import './Locations.scss';

export function LocationDetails() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // tenantId is automatically injected by baseApi
  const { data: locationResponse, isLoading: isLoadingLocation } = useGetLocationByIdQuery(
    id || '',
    { skip: !id }
  );

  const { data: warehousesResponse } = useGetWarehouseOptionsQuery();

  const { data: zonesResponse } = useGetZonesQuery({ pageSize: 1000 });

  const [updateLocation, { isLoading: isUpdating }] = useUpdateLocationMutation();
  const [deleteLocation, { isLoading: isDeleting }] = useDeleteLocationMutation();

  const location = locationResponse?.data;
  const warehouses = warehousesResponse?.data || [];
  const zones = (zonesResponse?.data?.items || []).map((z) => ({
    id: z.id,
    name: `${z.code} - ${z.name}`,
    warehouseId: z.warehouseId,
  }));

  const handleBack = () => {
    navigate(WAREHOUSE.LOCATIONS);
  };

  const handleSubmit = async (data: LocationFormData) => {
    if (!id) return;

    try {
      await updateLocation({
        id,
        data: {
          name: data.name || undefined,
          zoneId: data.zoneId || undefined,
          aisle: data.aisle || undefined,
          rack: data.rack || undefined,
          level: data.level || undefined,
          position: data.position || undefined,
          locationType: data.locationType,
          widthMm: data.widthMm || undefined,
          heightMm: data.heightMm || undefined,
          depthMm: data.depthMm || undefined,
          maxWeight: data.maxWeight || undefined,
          maxVolume: data.maxVolume || undefined,
          isActive: data.isActive,
          isPickLocation: data.isPickLocation,
          isPutawayLocation: data.isPutawayLocation,
          isReceivingDock: data.isReceivingDock,
          isShippingDock: data.isShippingDock,
          pickSequence: data.pickSequence || undefined,
          putawaySequence: data.putawaySequence || undefined,
        },
      }).unwrap();
      navigate(WAREHOUSE.LOCATIONS);
    } catch (error) {
      console.error('Failed to update location:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteLocation(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(WAREHOUSE.LOCATIONS);
    } catch (error) {
      console.error('Failed to delete location:', error);
    }
  };

  if (isLoadingLocation) {
    return (
      <div className="location-details">
        <div className="location-details__loading">Loading...</div>
      </div>
    );
  }

  if (!location) {
    return (
      <div className="location-details">
        <div className="location-details__not-found">
          <h2>Location not found</h2>
          <button className="btn btn-secondary" onClick={handleBack}>
            Back to Locations
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="location-details">
      {/* Header with back button */}
      <header className="location-details__header">
        <div className="location-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="location-details__title-section">
            <h1 className="location-details__title">{location.code}</h1>
            <span className={`location-type-badge location-type-badge--${location.locationType.toLowerCase()}`}>
              {location.locationType}
            </span>
            <span className={`status-badge status-badge--${location.isActive ? 'active' : 'inactive'}`}>
              {location.isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
            </span>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="location-details__content">
        <div className="location-details__form-container">
          <LocationForm
            initialData={{
              code: location.code,
              name: location.name || '',
              warehouseId: location.warehouseId,
              zoneId: location.zoneId || '',
              aisle: location.aisle || '',
              rack: location.rack || '',
              level: location.level || '',
              position: location.position || '',
              locationType: location.locationType,
              widthMm: location.widthMm ?? null,
              heightMm: location.heightMm ?? null,
              depthMm: location.depthMm ?? null,
              maxWeight: location.maxWeight ?? null,
              maxVolume: location.maxVolume ?? null,
              isActive: location.isActive,
              isPickLocation: location.isPickLocation,
              isPutawayLocation: location.isPutawayLocation,
              isReceivingDock: location.isReceivingDock,
              isShippingDock: location.isShippingDock,
              pickSequence: location.pickSequence ?? null,
              putawaySequence: location.putawaySequence ?? null,
            }}
            warehouses={warehouses}
            zones={zones}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode
          />
        </div>

        {/* Actions sidebar */}
        <aside className="location-details__sidebar">
          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('location.info', 'Information')}</h3>
            <div className="sidebar-section__content">
              <dl className="info-list">
                <div className="info-list__item">
                  <dt>{t('location.warehouse', 'Warehouse')}</dt>
                  <dd>{location.warehouseName}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('location.zone', 'Zone')}</dt>
                  <dd>{location.zoneName || '—'}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('location.stockLevels', 'Stock Levels')}</dt>
                  <dd>{location.stockLevelCount}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('location.createdAt', 'Created')}</dt>
                  <dd>{new Date(location.createdAt).toLocaleDateString()}</dd>
                </div>
              </dl>
            </div>
          </section>

          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('location.flags', 'Flags')}</h3>
            <div className="sidebar-section__content">
              <div className="location-flags">
                {location.isPickLocation && (
                  <span className="location-flag location-flag--pick">Pick</span>
                )}
                {location.isPutawayLocation && (
                  <span className="location-flag location-flag--putaway">Putaway</span>
                )}
                {location.isReceivingDock && (
                  <span className="location-flag location-flag--receiving">Receiving</span>
                )}
                {location.isShippingDock && (
                  <span className="location-flag location-flag--shipping">Shipping</span>
                )}
              </div>
            </div>
          </section>

          <section className="sidebar-section sidebar-section--danger">
            <h3 className="sidebar-section__title">{t('common.dangerZone', 'Danger Zone')}</h3>
            <div className="sidebar-section__content">
              <p className="sidebar-section__text">
                {t('location.deleteWarning', 'Deleting this location will remove all stock level references.')}
              </p>
              <button
                className="btn btn-danger btn-block"
                onClick={() => setDeleteConfirmOpen(true)}
                disabled={isDeleting || location.stockLevelCount > 0}
              >
                {t('location.deleteLocation', 'Delete Location')}
              </button>
              {location.stockLevelCount > 0 && (
                <p className="sidebar-section__text" style={{ marginTop: '8px', fontSize: '0.75rem' }}>
                  {t('location.cannotDeleteWithStock', 'Cannot delete location with stock.')}
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
        title={t('location.confirmDelete', 'Confirm Delete')}
        subtitle={location.code}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title="">
          <p>
            {t(
              'location.deleteConfirmText',
              'Are you sure you want to delete this location? This action cannot be undone.'
            )}
          </p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default LocationDetails;
