import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { INVENTORY } from '@/constants/routes';
import { useGetAdjustmentByIdQuery } from '@/api/modules/adjustments';

export function AdjustmentDetails() {
  const t = useTranslate();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const { data: response, isLoading } = useGetAdjustmentByIdQuery(id!, { skip: !id });
  const adjustment = response?.data;

  const handleBack = () => {
    navigate(INVENTORY.ADJUSTMENTS);
  };

  if (isLoading) {
    return (
      <div className="page page--form">
        <div className="page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!adjustment) {
    return (
      <div className="page page--form">
        <div className="page__error">{t('adjustments.notFound', 'Adjustment not found')}</div>
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
              {t('adjustments.adjustment', 'Adjustment')} #{adjustment.adjustmentNumber}
            </h1>
            <p className="page__subtitle">
              {adjustment.adjustmentType} - {adjustment.status}
            </p>
          </div>
        </div>
      </header>

      <div className="page__content">
        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('adjustments.details', 'Details')}</h3>
          </div>
          <div className="form-section__content">
            <div className="detail-grid">
              <div className="detail-field">
                <span className="detail-field__label">{t('common.warehouse', 'Warehouse')}</span>
                <span className="detail-field__value">{adjustment.warehouseName || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('adjustments.type', 'Type')}</span>
                <span className="detail-field__value">{adjustment.adjustmentType}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.status', 'Status')}</span>
                <span className="detail-field__value">{adjustment.status}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('adjustments.totalLines', 'Total Lines')}</span>
                <span className="detail-field__value">{adjustment.totalLines}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('adjustments.qtyChange', 'Qty Change')}</span>
                <span className="detail-field__value">{adjustment.totalQuantityChange}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.createdBy', 'Created By')}</span>
                <span className="detail-field__value">{adjustment.createdByUserName || '-'}</span>
              </div>
              <div className="detail-field">
                <span className="detail-field__label">{t('common.createdAt', 'Created At')}</span>
                <span className="detail-field__value">
                  {new Date(adjustment.createdAt).toLocaleString()}
                </span>
              </div>
              {adjustment.reasonNotes && (
                <div className="detail-field detail-field--full">
                  <span className="detail-field__label">{t('adjustments.reason', 'Reason')}</span>
                  <span className="detail-field__value">{adjustment.reasonNotes}</span>
                </div>
              )}
              {adjustment.notes && (
                <div className="detail-field detail-field--full">
                  <span className="detail-field__label">{t('common.notes', 'Notes')}</span>
                  <span className="detail-field__value">{adjustment.notes}</span>
                </div>
              )}
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}

export default AdjustmentDetails;
