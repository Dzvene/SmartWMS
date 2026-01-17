import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { WAREHOUSE } from '@/constants/routes';
import { EquipmentForm, type EquipmentFormData } from './EquipmentForm';
import { useCreateEquipmentMutation } from '@/api/modules/equipment';
import type { CreateEquipmentRequest } from '@/api/modules/equipment';

export function EquipmentCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createEquipment, { isLoading: isCreating }] = useCreateEquipmentMutation();

  const handleBack = () => {
    navigate(WAREHOUSE.EQUIPMENT);
  };

  const handleSubmit = async (data: EquipmentFormData) => {
    try {
      const createData: CreateEquipmentRequest = {
        code: data.code,
        name: data.name,
        description: data.description || undefined,
        type: data.type,
        status: data.status,
        warehouseId: data.warehouseId || undefined,
        serialNumber: data.serialNumber || undefined,
        manufacturer: data.manufacturer || undefined,
        model: data.model || undefined,
        purchaseDate: data.purchaseDate || undefined,
        warrantyExpiryDate: data.warrantyExpiryDate || undefined,
        nextMaintenanceDate: data.nextMaintenanceDate || undefined,
        isActive: data.isActive,
      };

      const result = await createEquipment(createData).unwrap();

      // Navigate to the created equipment or back to list
      if (result.data?.id) {
        navigate(`${WAREHOUSE.EQUIPMENT}/${result.data.id}`);
      } else {
        navigate(WAREHOUSE.EQUIPMENT);
      }
    } catch (error) {
      console.error('Failed to create equipment:', error);
    }
  };

  return (
    <div className="detail-page">
      <header className="detail-page__header">
        <div className="detail-page__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="detail-page__title-section">
            <h1 className="detail-page__title">
              {t('equipment.addEquipment', 'Add Equipment')}
            </h1>
            <span className="detail-page__subtitle">
              {t('equipment.createSubtitle', 'Fill in the details to add new equipment')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <EquipmentForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
