import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { INVENTORY } from '@/constants/routes';
import { AdjustmentForm, type AdjustmentFormData } from './AdjustmentForm';
import { useCreateAdjustmentMutation } from '@/api/modules/adjustments';

export function AdjustmentCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createAdjustment, { isLoading: isCreating }] = useCreateAdjustmentMutation();

  const handleBack = () => {
    navigate(INVENTORY.ADJUSTMENTS);
  };

  const handleSubmit = async (data: AdjustmentFormData) => {
    try {
      const result = await createAdjustment({
        warehouseId: data.warehouseId,
        adjustmentType: data.adjustmentType,
        reasonNotes: data.reasonNotes || undefined,
        notes: data.notes || undefined,
      }).unwrap();

      if (result.data?.id) {
        navigate(`${INVENTORY.ADJUSTMENTS}/${result.data.id}`);
      } else {
        navigate(INVENTORY.ADJUSTMENTS);
      }
    } catch (error) {
      console.error('Failed to create adjustment:', error);
    }
  };

  return (
    <div className="page page--form">
      <header className="page__header">
        <div className="page__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="page__title-section">
            <h1 className="page__title">{t('adjustments.newAdjustment', 'New Adjustment')}</h1>
            <p className="page__subtitle">
              {t('adjustments.createSubtitle', 'Create a new stock adjustment')}
            </p>
          </div>
        </div>
      </header>

      <div className="page__content">
        <AdjustmentForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}

export default AdjustmentCreate;
