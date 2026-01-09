import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useCreateZoneMutation } from '@/api/modules/zones';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import { WAREHOUSE } from '@/constants/routes';
import { ZoneForm, type ZoneFormData } from './ZoneForm';
import './Zones.scss';

export function ZoneCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createZone, { isLoading: isCreating }] = useCreateZoneMutation();

  // tenantId is automatically injected by baseApi
  const { data: warehousesResponse } = useGetWarehouseOptionsQuery();

  const warehouses = warehousesResponse?.data || [];

  const handleBack = () => {
    navigate(WAREHOUSE.ZONES);
  };

  const handleSubmit = async (data: ZoneFormData) => {
    try {
      await createZone({
        code: data.code,
        name: data.name,
        description: data.description || undefined,
        warehouseId: data.warehouseId,
        zoneType: data.zoneType,
        pickSequence: data.pickSequence || undefined,
        isActive: data.isActive,
      }).unwrap();
      navigate(WAREHOUSE.ZONES);
    } catch (error) {
      console.error('Failed to create zone:', error);
    }
  };

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
            <h1 className="zone-details__title">{t('zone.addZone', 'Add Zone')}</h1>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="zone-details__content">
        <div className="zone-details__form-container zone-details__form-container--full">
          <ZoneForm
            warehouses={warehouses}
            onSubmit={handleSubmit}
            loading={isCreating}
            isEditMode={false}
          />
        </div>
      </div>
    </div>
  );
}

export default ZoneCreate;
