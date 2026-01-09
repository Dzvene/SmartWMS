import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useCreateLocationMutation } from '@/api/modules/locations';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import { useGetZonesQuery } from '@/api/modules/zones';
import { WAREHOUSE } from '@/constants/routes';
import { LocationForm, type LocationFormData } from './LocationForm';
import './Locations.scss';

export function LocationCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createLocation, { isLoading: isCreating }] = useCreateLocationMutation();

  // tenantId is automatically injected by baseApi
  const { data: warehousesResponse } = useGetWarehouseOptionsQuery();

  const { data: zonesResponse } = useGetZonesQuery({ pageSize: 1000 });

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
    try {
      await createLocation({
        code: data.code,
        name: data.name || undefined,
        warehouseId: data.warehouseId,
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
      }).unwrap();
      navigate(WAREHOUSE.LOCATIONS);
    } catch (error) {
      console.error('Failed to create location:', error);
    }
  };

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
            <h1 className="location-details__title">{t('location.addLocation', 'Add Location')}</h1>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="location-details__content">
        <div className="location-details__form-container location-details__form-container--full">
          <LocationForm
            warehouses={warehouses}
            zones={zones}
            onSubmit={handleSubmit}
            loading={isCreating}
            isEditMode={false}
          />
        </div>
      </div>
    </div>
  );
}

export default LocationCreate;
