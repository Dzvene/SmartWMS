import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { INBOUND } from '@/constants/routes';
import { PutawayForm, type PutawayFormData } from './PutawayForm';
import { useCreatePutawayTaskMutation } from '@/api/modules/putaway';
import type { CreatePutawayTaskRequest } from '@/api/modules/putaway';

export function PutawayCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createTask, { isLoading: isCreating }] = useCreatePutawayTaskMutation();

  const handleBack = () => {
    navigate(INBOUND.PUTAWAY);
  };

  const handleSubmit = async (data: PutawayFormData) => {
    try {
      const createData: CreatePutawayTaskRequest = {
        productId: data.productId,
        fromLocationId: data.fromLocationId,
        quantity: data.quantity,
        batchNumber: data.batchNumber || undefined,
        serialNumber: data.serialNumber || undefined,
        expiryDate: data.expiryDate || undefined,
        suggestedLocationId: data.suggestedLocationId || undefined,
        priority: data.priority,
        notes: data.notes || undefined,
      };

      const result = await createTask(createData).unwrap();

      if (result.data?.id) {
        navigate(`${INBOUND.PUTAWAY}/${result.data.id}`);
      } else {
        navigate(INBOUND.PUTAWAY);
      }
    } catch (error) {
      console.error('Failed to create putaway task:', error);
    }
  };

  return (
    <div className="sales-order-details">
      <header className="sales-order-details__header">
        <div className="sales-order-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="sales-order-details__title-section">
            <h1 className="sales-order-details__title">
              {t('putaway.createTask', 'Create Putaway Task')}
            </h1>
            <span className="sales-order-details__subtitle">
              {t('putaway.createTaskSubtitle', 'Fill in the details to create a new putaway task')}
            </span>
          </div>
        </div>
      </header>

      <div className="sales-order-details__content">
        <PutawayForm onSubmit={handleSubmit} loading={isCreating} />
      </div>
    </div>
  );
}
