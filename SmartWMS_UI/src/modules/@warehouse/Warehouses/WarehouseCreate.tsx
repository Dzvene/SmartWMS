import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { WAREHOUSE } from '@/constants/routes';
import { WarehouseForm, type WarehouseFormData } from './WarehouseForm';
import './Warehouses.scss';

export function WarehouseCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  // TODO: Replace with useCreateWarehouseMutation
  const isCreating = false;

  const handleBack = () => {
    navigate(WAREHOUSE.WAREHOUSES);
  };

  const handleSubmit = async (data: WarehouseFormData) => {
    // TODO: Call createWarehouse mutation
    console.log('Create warehouse:', data);
    navigate(WAREHOUSE.WAREHOUSES);
  };

  return (
    <div className="warehouse-details">
      {/* Header with back button */}
      <header className="warehouse-details__header">
        <div className="warehouse-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="warehouse-details__title-section">
            <h1 className="warehouse-details__title">{t('warehouse.addWarehouse', 'Add Warehouse')}</h1>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="warehouse-details__content">
        <div className="warehouse-details__form-container warehouse-details__form-container--full">
          <WarehouseForm
            onSubmit={handleSubmit}
            loading={isCreating}
            isEditMode={false}
          />
        </div>
      </div>
    </div>
  );
}

export default WarehouseCreate;
