import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { ORDERS } from '@/constants/routes';
import { SalesOrderForm, type SalesOrderFormData } from './SalesOrderForm';
import { useCreateSalesOrderMutation } from '@/api/modules/orders';
import './SalesOrderDetails.scss';

export function SalesOrderCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createSalesOrder, { isLoading: isCreating }] = useCreateSalesOrderMutation();

  const handleBack = () => {
    navigate(ORDERS.SALES_ORDERS);
  };

  const handleSubmit = async (data: SalesOrderFormData) => {
    try {
      // Create order first (without lines for simpler API)
      const result = await createSalesOrder({
        customerId: data.customerId,
        warehouseId: data.warehouseId,
        priority: data.priority,
        requiredDate: data.requiredDate || undefined,
        externalReference: data.externalReference || undefined,
        shipToName: data.shipToName || undefined,
        shipToAddressLine1: data.shipToAddressLine1 || undefined,
        shipToAddressLine2: data.shipToAddressLine2 || undefined,
        shipToCity: data.shipToCity || undefined,
        shipToRegion: data.shipToRegion || undefined,
        shipToPostalCode: data.shipToPostalCode || undefined,
        shipToCountryCode: data.shipToCountryCode || undefined,
        carrierCode: data.carrierCode || undefined,
        serviceLevel: data.serviceLevel || undefined,
        shippingInstructions: data.shippingInstructions || undefined,
        notes: data.notes || undefined,
        internalNotes: data.internalNotes || undefined,
        lines: data.lines.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          requiredBatchNumber: line.requiredBatchNumber || undefined,
          requiredExpiryDate: line.requiredExpiryDate || undefined,
          notes: line.notes || undefined,
        })),
      }).unwrap();

      // Navigate to the created order or back to list
      if (result.data?.id) {
        navigate(`${ORDERS.SALES_ORDERS}/${result.data.id}`);
      } else {
        navigate(ORDERS.SALES_ORDERS);
      }
    } catch (error) {
      console.error('Failed to create sales order:', error);
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
              {t('orders.createSalesOrder', 'Create Sales Order')}
            </h1>
            <span className="sales-order-details__subtitle">
              {t('orders.createSalesOrderSubtitle', 'Fill in the details to create a new sales order')}
            </span>
          </div>
        </div>
      </header>

      <div className="sales-order-details__content">
        <SalesOrderForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
