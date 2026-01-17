import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { OUTBOUND } from '@/constants/routes';
import { PackingForm, type PackingFormData } from './PackingForm';
import { useCreatePackingTaskMutation } from '@/api/modules/packing';
import type { CreatePackingTaskRequest } from '@/api/modules/packing';

export function PackingCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createTask, { isLoading: isCreating }] = useCreatePackingTaskMutation();

  const handleBack = () => {
    navigate(OUTBOUND.PACKING);
  };

  const handleSubmit = async (data: PackingFormData) => {
    try {
      const createData: CreatePackingTaskRequest = {
        salesOrderId: data.salesOrderId,
        packingStationId: data.packingStationId || undefined,
        priority: data.priority,
        notes: data.notes || undefined,
      };

      const result = await createTask(createData).unwrap();

      if (result.data?.id) {
        navigate(`${OUTBOUND.PACKING}/${result.data.id}`);
      } else {
        navigate(OUTBOUND.PACKING);
      }
    } catch (error) {
      console.error('Failed to create packing task:', error);
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
              {t('packing.createTask', 'Create Packing Task')}
            </h1>
            <span className="sales-order-details__subtitle">
              {t('packing.createTaskSubtitle', 'Fill in the details to create a new packing task')}
            </span>
          </div>
        </div>
      </header>

      <div className="sales-order-details__content">
        <PackingForm onSubmit={handleSubmit} loading={isCreating} />
      </div>
    </div>
  );
}
