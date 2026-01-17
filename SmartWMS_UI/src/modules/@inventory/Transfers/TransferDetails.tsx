import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { INVENTORY } from '@/constants/routes';
import { useGetTransferByIdQuery } from '@/api/modules/transfers';

export function TransferDetails() {
  const t = useTranslate();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const { data: response, isLoading } = useGetTransferByIdQuery(id!, { skip: !id });
  const transfer = response?.data;

  const handleBack = () => {
    navigate(INVENTORY.TRANSFERS);
  };

  if (isLoading) {
    return (
      <div className="page page--form">
        <div className="page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!transfer) {
    return (
      <div className="page page--form">
        <div className="page__error">{t('transfers.notFound', 'Transfer not found')}</div>
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
              {t('transfers.transfer', 'Transfer')} #{transfer.transferNumber}
            </h1>
            <p className="page__subtitle">
              {transfer.transferType} - {transfer.status}
            </p>
          </div>
        </div>
      </header>

      <div className="page__content">
        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('transfers.details', 'Details')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('transfers.sourceWarehouse', 'Source')}</span>
                <span className="detail-field__value">{transfer.sourceWarehouseName || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('transfers.destinationWarehouse', 'Destination')}</span>
                <span className="detail-field__value">{transfer.destinationWarehouseName || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('transfers.type', 'Type')}</span>
                <span className="detail-field__value">{transfer.transferType}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.priority', 'Priority')}</span>
                <span className="detail-field__value">{transfer.priority}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.status', 'Status')}</span>
                <span className="detail-field__value">{transfer.status}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('transfers.progress', 'Progress')}</span>
                <span className="detail-field__value">{transfer.completedLines}/{transfer.totalLines}</span>
              </div>
              {transfer.scheduledDate && (
                <div className="detail-field">
                  <span className="detail-field__label">{t('transfers.scheduledDate', 'Scheduled')}</span>
                  <span className="detail-field__value">
                    {new Date(transfer.scheduledDate).toLocaleDateString()}
                  </span>
                </div>
              )}
              <div className="detail-field">
                <span className="detail-field__label">{t('transfers.assignedTo', 'Assigned To')}</span>
                <span className="detail-field__value">{transfer.assignedUserName || '-'}</span>
              </div>
              {transfer.notes && (
                <div className="detail-field detail-field--full">
                  <span className="detail-field__label">{t('common.notes', 'Notes')}</span>
                  <span className="detail-field__value">{transfer.notes}</span>
                </div>
              )}
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}

export default TransferDetails;
