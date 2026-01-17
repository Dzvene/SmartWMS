import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { INBOUND } from '@/constants/routes';
import { useGetReturnOrderByIdQuery } from '@/api/modules/returns';
import type { ReturnOrderStatus, ReturnType, ReturnCondition, ReturnDisposition } from '@/api/modules/returns';

const STATUS_COLORS: Record<ReturnOrderStatus, string> = {
  Pending: 'warning',
  InTransit: 'info',
  Received: 'info',
  InProgress: 'info',
  Complete: 'success',
  Cancelled: 'error',
};

const RETURN_TYPE_LABELS: Record<ReturnType, string> = {
  CustomerReturn: 'Customer Return',
  SupplierReturn: 'Supplier Return',
  InternalTransfer: 'Internal Transfer',
  Damaged: 'Damaged',
  Recall: 'Recall',
};

const CONDITION_LABELS: Record<ReturnCondition, string> = {
  Unknown: 'Unknown',
  Good: 'Good',
  Refurbished: 'Refurbished',
  Damaged: 'Damaged',
  Defective: 'Defective',
  Destroyed: 'Destroyed',
};

const DISPOSITION_LABELS: Record<ReturnDisposition, string> = {
  Pending: 'Pending',
  ReturnToStock: 'Return to Stock',
  Quarantine: 'Quarantine',
  Scrap: 'Scrap',
  ReturnToSupplier: 'Return to Supplier',
  Donate: 'Donate',
  Repair: 'Repair',
};

export function ReturnsDetails() {
  const t = useTranslate();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const { data: response, isLoading } = useGetReturnOrderByIdQuery(id!, { skip: !id });
  const returnOrder = response?.data;

  const handleBack = () => {
    navigate(INBOUND.RETURNS);
  };

  if (isLoading) {
    return (
      <div className="page page--form">
        <div className="page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!returnOrder) {
    return (
      <div className="page page--form">
        <div className="page__error">{t('returns.notFound', 'Return order not found')}</div>
      </div>
    );
  }

  const progressPercent = returnOrder.totalQuantityExpected > 0
    ? Math.round((returnOrder.totalQuantityReceived / returnOrder.totalQuantityExpected) * 100)
    : 0;

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
              {t('returns.return', 'Return')} #{returnOrder.returnNumber}
            </h1>
            <p className="page__subtitle">
              <span className={`status-badge status-badge--${STATUS_COLORS[returnOrder.status]}`}>
                {returnOrder.status}
              </span>
            </p>
          </div>
        </div>
      </header>

      <div className="page__content">
        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('returns.information', 'Return Information')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.returnNumber', 'Return #')}</span>
                <span className="detail-field__value">{returnOrder.returnNumber}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.rmaNumber', 'RMA #')}</span>
                <span className="detail-field__value">{returnOrder.rmaNumber || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.type', 'Type')}</span>
                <span className="detail-field__value">{RETURN_TYPE_LABELS[returnOrder.returnType]}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.status', 'Status')}</span>
                <span className="detail-field__value">{returnOrder.status}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.originalOrder', 'Original Order')}</span>
                <span className="detail-field__value">{returnOrder.originalSalesOrderNumber || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.customer', 'Customer')}</span>
                <span className="detail-field__value">{returnOrder.customerName || '-'}</span>
              </div>
            </div>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('returns.progress', 'Progress')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.totalLines', 'Total Lines')}</span>
                <span className="detail-field__value">{returnOrder.totalLines}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.expected', 'Expected')}</span>
                <span className="detail-field__value">{returnOrder.totalQuantityExpected}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.received', 'Received')}</span>
                <span className="detail-field__value">{returnOrder.totalQuantityReceived}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.progressPercent', 'Progress')}</span>
                <span className="detail-field__value">{progressPercent}%</span>
              </div>
            </div>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('returns.shipping', 'Shipping Information')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.carrier', 'Carrier')}</span>
                <span className="detail-field__value">{returnOrder.carrierCode || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.tracking', 'Tracking #')}</span>
                <span className="detail-field__value">
                  {returnOrder.trackingNumber ? (
                    <code className="code">{returnOrder.trackingNumber}</code>
                  ) : '-'}
                </span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.receivingLocation', 'Receiving Location')}</span>
                <span className="detail-field__value">{returnOrder.receivingLocationCode || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.assignedTo', 'Assigned To')}</span>
                <span className="detail-field__value">{returnOrder.assignedToUserName || '-'}</span>
              </div>
            </div>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('returns.dates', 'Dates')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.requestedDate', 'Requested')}</span>
                <span className="detail-field__value">
                  {returnOrder.requestedDate ? new Date(returnOrder.requestedDate).toLocaleDateString() : '-'}
                </span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.receivedDate', 'Received')}</span>
                <span className="detail-field__value">
                  {returnOrder.receivedDate ? new Date(returnOrder.receivedDate).toLocaleDateString() : '-'}
                </span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.processedDate', 'Processed')}</span>
                <span className="detail-field__value">
                  {returnOrder.processedDate ? new Date(returnOrder.processedDate).toLocaleDateString() : '-'}
                </span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('returns.rmaExpiry', 'RMA Expiry')}</span>
                <span className="detail-field__value">
                  {returnOrder.rmaExpiryDate ? new Date(returnOrder.rmaExpiryDate).toLocaleDateString() : '-'}
                </span>
              </div>
            </div>
          </div>
        </section>

        {returnOrder.lines && returnOrder.lines.length > 0 && (
          <section className="form-section">
            <div className="form-section__header">
              <h3 className="form-section__title">{t('returns.lines', 'Return Lines')}</h3>
            </div>
            <div className="form-section__content">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>{t('common.sku', 'SKU')}</th>
                    <th>{t('common.product', 'Product')}</th>
                    <th>{t('returns.expected', 'Expected')}</th>
                    <th>{t('returns.received', 'Received')}</th>
                    <th>{t('returns.accepted', 'Accepted')}</th>
                    <th>{t('returns.rejected', 'Rejected')}</th>
                    <th>{t('returns.condition', 'Condition')}</th>
                    <th>{t('returns.disposition', 'Disposition')}</th>
                  </tr>
                </thead>
                <tbody>
                  {returnOrder.lines.map((line) => (
                    <tr key={line.id}>
                      <td>{line.lineNumber}</td>
                      <td><span className="sku">{line.sku}</span></td>
                      <td>{line.productName || '-'}</td>
                      <td>{line.quantityExpected}</td>
                      <td>{line.quantityReceived}</td>
                      <td>{line.quantityAccepted}</td>
                      <td>{line.quantityRejected}</td>
                      <td>{CONDITION_LABELS[line.condition]}</td>
                      <td>{DISPOSITION_LABELS[line.disposition]}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}

        {(returnOrder.notes || returnOrder.internalNotes) && (
          <section className="form-section">
            <div className="form-section__header">
              <h3 className="form-section__title">{t('common.notes', 'Notes')}</h3>
            </div>
            <div className="form-section__content">
              {returnOrder.notes && (
                <div className="detail-field">
                  <span className="detail-field__label">{t('returns.notes', 'Notes')}</span>
                  <span className="detail-field__value">{returnOrder.notes}</span>
                </div>
              )}
              {returnOrder.internalNotes && (
                <div className="detail-field">
                  <span className="detail-field__label">{t('returns.internalNotes', 'Internal Notes')}</span>
                  <span className="detail-field__value">{returnOrder.internalNotes}</span>
                </div>
              )}
            </div>
          </section>
        )}
      </div>
    </div>
  );
}

export default ReturnsDetails;
