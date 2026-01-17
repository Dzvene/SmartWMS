import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { INBOUND } from '@/constants/routes';
import { ReturnsForm, type ReturnsFormData } from './ReturnsForm';
import { useCreateReturnOrderMutation } from '@/api/modules/returns';
import type { CreateReturnOrderRequest } from '@/api/modules/returns';

export function ReturnsCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createReturn, { isLoading: isCreating }] = useCreateReturnOrderMutation();

  const handleBack = () => {
    navigate(INBOUND.RETURNS);
  };

  const handleSubmit = async (data: ReturnsFormData) => {
    try {
      const createData: CreateReturnOrderRequest = {
        customerId: data.customerId,
        returnType: data.returnType,
        receivingLocationId: data.receivingLocationId || undefined,
        requestedDate: data.requestedDate || undefined,
        rmaNumber: data.rmaNumber || undefined,
        rmaExpiryDate: data.rmaExpiryDate || undefined,
        reasonDescription: data.reasonDescription || undefined,
        notes: data.notes || undefined,
      };

      const result = await createReturn(createData).unwrap();

      if (result.data?.id) {
        navigate(`${INBOUND.RETURNS}/${result.data.id}`);
      } else {
        navigate(INBOUND.RETURNS);
      }
    } catch (error) {
      console.error('Failed to create return:', error);
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
              {t('returns.createRMA', 'Create RMA')}
            </h1>
            <span className="sales-order-details__subtitle">
              {t('returns.createRMASubtitle', 'Fill in the details to create a new return authorization')}
            </span>
          </div>
        </div>
      </header>

      <div className="sales-order-details__content">
        <ReturnsForm onSubmit={handleSubmit} loading={isCreating} />
      </div>
    </div>
  );
}
