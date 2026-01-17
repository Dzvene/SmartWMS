import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { INBOUND } from '@/constants/routes';
import { ReceivingForm, type ReceivingFormData } from './ReceivingForm';
import { useCreateGoodsReceiptMutation } from '@/api/modules/receiving';
import type { CreateGoodsReceiptRequest } from '@/api/modules/receiving';

export function ReceivingCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createReceipt, { isLoading: isCreating }] = useCreateGoodsReceiptMutation();

  const handleBack = () => {
    navigate(INBOUND.RECEIVING);
  };

  const handleSubmit = async (data: ReceivingFormData) => {
    try {
      const createData: CreateGoodsReceiptRequest = {
        warehouseId: data.warehouseId,
        supplierId: data.supplierId || undefined,
        carrierName: data.carrierName || undefined,
        trackingNumber: data.trackingNumber || undefined,
        deliveryNote: data.deliveryNote || undefined,
        notes: data.notes || undefined,
      };

      const result = await createReceipt(createData).unwrap();

      // Navigate to the created receipt or back to list
      if (result.data?.id) {
        navigate(`${INBOUND.RECEIVING}/${result.data.id}`);
      } else {
        navigate(INBOUND.RECEIVING);
      }
    } catch (error) {
      console.error('Failed to create receipt:', error);
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
              {t('receiving.createReceipt', 'Create Receipt')}
            </h1>
            <span className="sales-order-details__subtitle">
              {t('receiving.createReceiptSubtitle', 'Fill in the details to start a new receiving process')}
            </span>
          </div>
        </div>
      </header>

      <div className="sales-order-details__content">
        <ReceivingForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
