import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { INVENTORY } from '@/constants/routes';
import { TransferForm, type TransferFormData } from './TransferForm';
import { useCreateTransferMutation } from '@/api/modules/transfers';

export function TransferCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createTransfer, { isLoading: isCreating }] = useCreateTransferMutation();

  const handleBack = () => {
    navigate(INVENTORY.TRANSFERS);
  };

  const handleSubmit = async (data: TransferFormData) => {
    try {
      const result = await createTransfer({
        sourceWarehouseId: data.sourceWarehouseId,
        destinationWarehouseId: data.destinationWarehouseId,
        transferType: data.transferType,
        priority: data.priority,
        scheduledDate: data.scheduledDate || undefined,
        notes: data.notes || undefined,
      }).unwrap();

      if (result.data?.id) {
        navigate(`${INVENTORY.TRANSFERS}/${result.data.id}`);
      } else {
        navigate(INVENTORY.TRANSFERS);
      }
    } catch (error) {
      console.error('Failed to create transfer:', error);
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
            <h1 className="page__title">{t('transfers.newTransfer', 'New Transfer')}</h1>
            <p className="page__subtitle">
              {t('transfers.createSubtitle', 'Create a new stock transfer')}
            </p>
          </div>
        </div>
      </header>

      <div className="page__content">
        <TransferForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}

export default TransferCreate;
