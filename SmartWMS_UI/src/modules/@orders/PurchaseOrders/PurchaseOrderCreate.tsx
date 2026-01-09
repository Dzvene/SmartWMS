import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { ORDERS } from '@/constants/routes';
import { PurchaseOrderForm, type PurchaseOrderFormData } from './PurchaseOrderForm';
import { useCreatePurchaseOrderMutation } from '@/api/modules/orders';
import './PurchaseOrderDetails.scss';

export function PurchaseOrderCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createPurchaseOrder, { isLoading: isCreating }] = useCreatePurchaseOrderMutation();

  const handleBack = () => {
    navigate(ORDERS.PURCHASE_ORDERS);
  };

  const handleSubmit = async (data: PurchaseOrderFormData) => {
    try {
      const result = await createPurchaseOrder({
        supplierId: data.supplierId,
        warehouseId: data.warehouseId,
        expectedDate: data.expectedDate || undefined,
        externalReference: data.externalReference || undefined,
        receivingDockId: data.receivingDockId || undefined,
        notes: data.notes || undefined,
        internalNotes: data.internalNotes || undefined,
        lines: data.lines.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          expectedBatchNumber: line.expectedBatchNumber || undefined,
          expectedExpiryDate: line.expectedExpiryDate || undefined,
          notes: line.notes || undefined,
        })),
      }).unwrap();

      if (result.data?.id) {
        navigate(`${ORDERS.PURCHASE_ORDERS}/${result.data.id}`);
      } else {
        navigate(ORDERS.PURCHASE_ORDERS);
      }
    } catch (error) {
      console.error('Failed to create purchase order:', error);
    }
  };

  return (
    <div className="purchase-order-details">
      <header className="purchase-order-details__header">
        <div className="purchase-order-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="purchase-order-details__title-section">
            <h1 className="purchase-order-details__title">
              {t('orders.createPurchaseOrder', 'Create Purchase Order')}
            </h1>
            <span className="purchase-order-details__subtitle">
              {t('orders.createPurchaseOrderSubtitle', 'Fill in the details to create a new purchase order')}
            </span>
          </div>
        </div>
      </header>

      <div className="purchase-order-details__content">
        <PurchaseOrderForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
