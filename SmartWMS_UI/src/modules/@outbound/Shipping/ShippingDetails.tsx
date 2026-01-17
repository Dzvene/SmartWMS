import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { OUTBOUND } from '@/constants/routes';
import { useGetShipmentByIdQuery } from '@/api/modules/fulfillment';
import type { ShipmentStatus } from '@/api/modules/fulfillment';

const STATUS_COLORS: Record<ShipmentStatus, string> = {
  Pending: 'warning',
  ReadyToShip: 'info',
  Shipped: 'info',
  Delivered: 'success',
  Cancelled: 'error',
};

export function ShippingDetails() {
  const t = useTranslate();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const { data: response, isLoading } = useGetShipmentByIdQuery(id!, { skip: !id });
  const shipment = response?.data;

  const handleBack = () => {
    navigate(OUTBOUND.SHIPPING);
  };

  if (isLoading) {
    return (
      <div className="page page--form">
        <div className="page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!shipment) {
    return (
      <div className="page page--form">
        <div className="page__error">{t('shipment.notFound', 'Shipment not found')}</div>
      </div>
    );
  }

  return (
    <div className="page page--form">
      <header className="page__header">
        <div className="page__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="page__title-section">
            <h1 className="page__title">
              {t('shipment.shipment', 'Shipment')} #{shipment.shipmentNumber}
            </h1>
            <p className="page__subtitle">
              <span className={`status-badge status-badge--${STATUS_COLORS[shipment.status]}`}>
                {shipment.status}
              </span>
            </p>
          </div>
        </div>
      </header>

      <div className="page__content">
        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('shipment.information', 'Shipment Information')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('shipment.shipmentNumber', 'Shipment #')}</span>
                <span className="detail-field__value">{shipment.shipmentNumber}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('shipment.orderNumber', 'Order #')}</span>
                <span className="detail-field__value">{shipment.salesOrderNumber || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.customer', 'Customer')}</span>
                <span className="detail-field__value">{shipment.customerName || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.status', 'Status')}</span>
                <span className="detail-field__value">{shipment.status}</span>
              </div>
            </div>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('shipment.carrierDetails', 'Carrier Details')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('shipment.carrier', 'Carrier')}</span>
                <span className="detail-field__value">{shipment.carrierName || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('shipment.service', 'Service')}</span>
                <span className="detail-field__value">{shipment.carrierServiceName || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('shipment.tracking', 'Tracking #')}</span>
                <span className="detail-field__value">
                  {shipment.trackingNumber ? (
                    <code className="code">{shipment.trackingNumber}</code>
                  ) : '-'}
                </span>
              </div>
            </div>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('shipment.packageDetails', 'Package Details')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('shipment.packages', 'Packages')}</span>
                <span className="detail-field__value">{shipment.packageCount}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('shipment.totalWeight', 'Total Weight')}</span>
                <span className="detail-field__value">
                  {shipment.totalWeight} {shipment.weightUnit || 'kg'}
                </span>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}

export default ShippingDetails;
